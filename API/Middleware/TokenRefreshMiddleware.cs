using Microsoft.AspNetCore.Authorization;
using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Application.UseCases.Interfaces;
using SistemaEventos.Server.Services;

namespace SistemaEventos.Server.Middleware;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRefreshMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAutorizacionService autorizacionService,
        ICookieService cookieService,
        IDateTimeProvider dateTimeProvider,
        IJwtTokenService jwtTokenService)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null
            || endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null
            || endpoint.Metadata.GetMetadata<IAuthorizeData>() is null)
        {
            await _next(context);
            return;
        }

        try
        {
            var accessToken = GetAccessToken(context);
            context.Request.Cookies.TryGetValue("cookieRefreshToken", out var refreshToken);

            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                await WriteBadRequestAsync(context, "Favor validar que tanto el AccessToken como el RefreshToken sean enviados.");
                return;
            }

            if (!autorizacionService.ValidarToken(accessToken))
            {
                await WriteBadRequestAsync(context, "AccessToken procesado es invalido.");
                return;
            }

            var idUsuario = jwtTokenService.ObtenerIdUsuario(accessToken);
            if (idUsuario is null)
            {
                await WriteBadRequestAsync(context, "No fue posible identificar al usuario del token suministrado.");
                return;
            }

            var fechaActual = dateTimeProvider.GetCurrentDateTime();
            var fechaExpiracionAccessToken = jwtTokenService.ObtenerFechaExpiracionAccessToken(accessToken);
            var fechaExpiracionRefreshToken = await autorizacionService.ConsultarFechaVencimientoRefreshTokenAsync(idUsuario.Value, context.RequestAborted);

            if (fechaExpiracionRefreshToken is null)
            {
                await WriteBadRequestAsync(context, $"No existe ningun token activo para el usuario '{idUsuario}'. Favor iniciar sesion nuevamente.");
                return;
            }

            if (fechaExpiracionRefreshToken < fechaActual)
            {
                await WriteBadRequestAsync(context, "RefreshToken ya ha expirado. Favor ingresar nuevamente con sus credenciales de acceso.");
                return;
            }

            var newAccessToken = accessToken;
            if (fechaExpiracionAccessToken is not null
                && fechaExpiracionAccessToken < fechaActual
                && fechaExpiracionRefreshToken > fechaActual)
            {
                var respuesta = await autorizacionService.ActualizarAccessTokenConRefreshTokenAnteriorAsync(
                    idUsuario.Value,
                    accessToken,
                    refreshToken,
                    context.RequestAborted);

                if (!respuesta.IsSuccess || respuesta.Valor is null)
                {
                    await WriteBadRequestAsync(context, respuesta.Mensaje);
                    return;
                }

                newAccessToken = respuesta.Valor.AccessToken;
            }

            cookieService.SetCookieAccessToken(newAccessToken);
            cookieService.SetCookieRefreshToken(refreshToken);

            context.Items["IdUsuario"] = idUsuario.Value.ToString();
            context.Items["AccessToken"] = newAccessToken;
            context.Items["RefreshToken"] = refreshToken;
            context.Request.Headers.Authorization = $"Bearer {newAccessToken}";

            await _next(context);
        }
        catch (Exception exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new Respuesta<object>
            {
                IsSuccess = false,
                Mensaje = $"Ocurrio un error en la autorizacion. {exception.Message}"
            });
        }
    }

    private static string? GetAccessToken(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("cookieAccessToken", out var cookieToken)
            && !string.IsNullOrWhiteSpace(cookieToken))
        {
            return cookieToken;
        }

        var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(authorizationHeader)
            && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorizationHeader[7..].Trim();
        }

        return null;
    }

    private static async Task WriteBadRequestAsync(HttpContext context, string mensaje)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new Respuesta<object>
        {
            IsSuccess = false,
            Mensaje = mensaje
        });
    }
}