using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Application.UseCases.Interfaces;
using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.UseCases.Implementations;

public class AutorizacionService : IAutorizacionService
{
    private readonly IAutorizacionRepository _autorizacionRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioRepository _usuarioRepository;

    public AutorizacionService(
        IAutorizacionRepository autorizacionRepository,
        IDateTimeProvider dateTimeProvider,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IUsuarioRepository usuarioRepository)
    {
        _autorizacionRepository = autorizacionRepository;
        _dateTimeProvider = dateTimeProvider;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Respuesta<AuthTokensDto>> GenerarTokensConCredencialesAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorEmailAsync(email, cancellationToken);
            if (usuarioEncontrado is null)
            {
                return Respuesta<AuthTokensDto>.Fail("Usuario no encontrado. Favor validar los datos ingresados.");
            }

            return await GuardarNuevaSesionAsync(usuarioEncontrado.IdUsuario, cancellationToken);
        }
        catch (Exception exception)
        {
            return Respuesta<AuthTokensDto>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<AuthTokensDto>> GenerarTokensConRefreshTokenAnteriorAsync(int idUsuario, string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshTokenEncontrado = await _autorizacionRepository
                .ConsultarUltimoHistorialRefreshTokensPorUsuarioAsync(idUsuario, accessToken, refreshToken, cancellationToken);

            if (refreshTokenEncontrado is null)
            {
                return Respuesta<AuthTokensDto>.Fail("El RefreshToken suministrado no existe o no se encuentra activo para ese usuario.");
            }

            return await GuardarNuevaSesionAsync(idUsuario, cancellationToken);
        }
        catch (Exception exception)
        {
            return Respuesta<AuthTokensDto>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<AuthTokensDto>> ActualizarAccessTokenConRefreshTokenAnteriorAsync(int idUsuario, string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshTokenEncontrado = await _autorizacionRepository
                .ConsultarUltimoHistorialRefreshTokensPorUsuarioAsync(idUsuario, accessToken, refreshToken, cancellationToken);

            if (refreshTokenEncontrado is null)
            {
                return Respuesta<AuthTokensDto>.Fail("El AccessToken y/o el RefreshToken suministrados no existen, o el RefreshToken no se encuentra activo para ese usuario.");
            }

            var nuevoAccessToken = _jwtTokenService.GenerarAccessToken(idUsuario);
            var actualizado = await _unitOfWork.ExecuteAsync(
                ct => _autorizacionRepository.ActualizarHistorialRefreshTokenDeUsuarioAsync(refreshTokenEncontrado.IdHistorialToken, nuevoAccessToken, ct),
                cancellationToken);

            if (!actualizado)
            {
                return Respuesta<AuthTokensDto>.Fail("No fue posible actualizar el AccessToken del usuario.");
            }

            return Respuesta<AuthTokensDto>.Ok(
                new AuthTokensDto
                {
                    IdUsuario = idUsuario,
                    AccessToken = nuevoAccessToken,
                    RefreshToken = refreshTokenEncontrado.RefreshToken
                },
                "AccessToken actualizado correctamente.");
        }
        catch (Exception exception)
        {
            return Respuesta<AuthTokensDto>.Fail(exception.Message);
        }
    }

    public async Task<DateTime?> ConsultarFechaVencimientoRefreshTokenAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        var refreshTokenEncontrado = await _autorizacionRepository.ConsultarUltimoHistorialRefreshTokensPorUsuarioAsync(idUsuario, cancellationToken: cancellationToken);
        return refreshTokenEncontrado?.FechaExpiracion;
    }

    public async Task<Respuesta<bool>> CerrarSesionAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokensUsuario = await _autorizacionRepository.ConsultarUltimoHistorialRefreshTokensPorUsuarioAsync(idUsuario, cancellationToken: cancellationToken);
            if (tokensUsuario is null)
            {
                return Respuesta<bool>.Fail($"No existen tokens activos del usuario '{idUsuario}' para eliminar.");
            }

            var esExitoso = await _unitOfWork.ExecuteAsync(
                ct => _autorizacionRepository.EliminarHistorialRefreshTokensPorUsuarioAsync(idUsuario, ct),
                cancellationToken);

            if (!esExitoso)
            {
                return Respuesta<bool>.Fail($"No fue posible cerrar la sesion del usuario '{idUsuario}'.");
            }

            return Respuesta<bool>.Ok(true, $"Se eliminaron todos los tokens del usuario '{idUsuario}'. Sesion cerrada correctamente.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    public bool ValidarToken(string accessToken)
    {
        return _jwtTokenService.ValidarToken(accessToken);
    }

    private async Task<Respuesta<AuthTokensDto>> GuardarNuevaSesionAsync(int idUsuario, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.GenerarAccessToken(idUsuario);
        var refreshToken = _jwtTokenService.GenerarRefreshToken();
        var fechaCreacion = _dateTimeProvider.GetCurrentDateTime();
        var fechaExpiracion = _jwtTokenService.ObtenerFechaExpiracionRefreshToken(fechaCreacion);

        var historialRefreshToken = new HistorialRefreshToken
        {
            IdUsuario = idUsuario,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            FechaCreacion = fechaCreacion,
            FechaExpiracion = fechaExpiracion
        };

        var guardado = await _unitOfWork.ExecuteAsync(
            async ct =>
            {
                await _autorizacionRepository.EliminarHistorialRefreshTokensPorUsuarioAsync(idUsuario, ct);
                return await _autorizacionRepository.GuardarHistorialRefreshTokenDeUsuarioAsync(historialRefreshToken, ct);
            },
            cancellationToken);

        if (!guardado)
        {
            return Respuesta<AuthTokensDto>.Fail("Error al momento de generar el AccessToken y el RefreshToken.");
        }

        return Respuesta<AuthTokensDto>.Ok(
            new AuthTokensDto
            {
                IdUsuario = idUsuario,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            },
            "AccessToken y RefreshToken generados correctamente.");
    }
}