using Microsoft.AspNetCore.Authorization;
using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Application.UseCases.Interfaces;
using SistemaEventos.Server.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SistemaEventos.Server.Middleware;

public class AdministradorHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public AdministradorHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    //Realiza validaciones previas de seguridad sobre TODAS las llamadas hacia cualquier endpoint
    public async Task InvokeAsync(
        HttpContext context,
        IAutorizacionService autorizacionService,
        ICookieService cookieService,
        IDateTimeProvider dateTimeProvider,
        IJwtTokenService jwtTokenService)
    {
        //Verificar si el endpoint del cual se hace la solicitud requiere autorización
        var endpoint = context.GetEndpoint();

        //Si el endpoint no existe, o no tiene [Authorize] o tiene [AllowAnonymous], entonces saltamos este middleware
        if (endpoint is null || endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null || endpoint.Metadata.GetMetadata<IAuthorizeData>() is null)
        {
            await _next(context);
            return;
        }

        try
        {
            //Headers
            var accessToken = GetAccessToken(context); //AccessToken

            //Cookies
            var refreshToken = GetRefreshToken(context); //RefreshToken

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                await WriteBadRequest(context, "Favor validar que tanto el AccessToken como el RefreshToken sean enviados.");
                return;
            }

            if (!autorizacionService.ValidarToken(accessToken))
            {
                await WriteBadRequest(context, "AccessToken procesado es inválido.");
                return;
            }

            var idUsuario = jwtTokenService.ObtenerIdUsuario(accessToken);
            if (idUsuario is null)
            {
                await WriteBadRequest(context, "No fue posible identificar al usuario del token suministrado.");
                return;
            }

            var fechaActual = dateTimeProvider.ObtenerDateTimeActual();
            var fechaExpiracionAccessToken = jwtTokenService.ObtenerFechaExpiracionAccessToken(accessToken);
            var fechaExpiracionRefreshToken = await autorizacionService.ConsultarFechaVencimientoRefreshToken(idUsuario.Value, context.RequestAborted);

            if (fechaExpiracionRefreshToken is null)
            {
                await WriteBadRequest(context, $"No existe ningún token activo para el usuario '{idUsuario}'. Favor iniciar sesión nuevamente.");
                return;
            }

            if (fechaExpiracionRefreshToken < fechaActual)
            {
                await WriteBadRequest(context, "RefreshToken ya ha expirado. Favor ingresar nuevamente con sus credenciales de acceso.");
                return;
            }

            //Si el AccessToken ya está vencido, pero el RefreshToken sigue aún vigente, se procede a crear un nuevo AccessToken
            Respuesta<AuthTokensDTO> respuesta = null!;
            if (fechaExpiracionAccessToken is not null && fechaExpiracionAccessToken < fechaActual && fechaExpiracionRefreshToken > fechaActual)
            {
                //Por cada petición que requiera autenticación (AccessToken de por medio), se genera un nuevo AccessToken para refrescar su tiempo de expiración, esto debido a su corto tiempo de vida
                respuesta = await autorizacionService.ActualizarAccessTokenConRefreshTokenAnterior(
                    idUsuario.Value,
                    accessToken,
                    refreshToken,
                    context.RequestAborted);

                if (!respuesta.IsSuccess || respuesta.Valor is null)
                {
                    await WriteBadRequest(context, respuesta.Mensaje);
                    return;
                }
            }

            string newAccessToken
                = respuesta is null || respuesta.Valor is null
                ? accessToken
                : respuesta.Valor.AccessToken;

            //Cargar las cookies en el navegador del usuario (quedan actualizadas para la siguiente petición entrante)
            //cookieService.SetCookieAccessToken(newAccessToken);
            cookieService.SetCookieRefreshToken(refreshToken);

            //Variables globales para uso a nivel local en el servidor (nunca se envían al Frontend)
            context.Items["IdUsuario"] = idUsuario.Value.ToString();
            context.Items["AccessToken"] = newAccessToken;
            context.Items["RefreshToken"] = refreshToken;

            ActualizarContextoHttpDelUsuario(context, newAccessToken);

            await _next(context);
        }
        catch (Exception exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new Respuesta<object>
            {
                IsSuccess = false,
                Mensaje = $"Ocurrió un error en la autorización. {exception.Message}"
            });
        }
    }

    #region Métodos PRIVADOS
    //Método encargado de extraer el AccessToken del header Authorization, validando q el formato sea correcto (Bearer {token})
    private static string? GetAccessToken(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorizationHeader[7..].Trim();
        }

        return null;
    }

    //Método encargado de extraer el RefreshToken de las Cookies, validando q la cookie exista y no esté vacía
    private static string? GetRefreshToken(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("cookieRefreshToken", out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
        {
            return refreshToken;
        }

        return null;
    }

    //Método encargado de escribir una respuesta de error con status code 400 (Bad Request) y un mensaje personalizado
    private static async Task WriteBadRequest(HttpContext context, string mensaje)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new Respuesta<object>
        {
            IsSuccess = false,
            Mensaje = mensaje
        });
    }

    //Método encargado de actualizar el contexto HTTP del usuario con los claims extraídos del nuevo AccessToken generado,
    //esto para q el usuario pueda acceder a los recursos autorizados sin necesidad de volver a autenticarse después de q su AccessToken haya expirado y se haya generado uno nuevo usando el RefreshToken
    private void ActualizarContextoHttpDelUsuario(HttpContext context, string newAccessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(newAccessToken);

        var claims = jwtToken.Claims;
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        context.User = principal;
    }
    #endregion

}