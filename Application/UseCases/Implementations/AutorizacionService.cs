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

    //Método encargado de generar un nuevo AccessToken y RefreshToken para el usuario utilizando sus credenciales (email), garantizando que el usuario exista antes de generar los tokens
    public async Task<Respuesta<AuthTokensDTO>> GenerarTokensConCredenciales(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorEmail(email, cancellationToken);
            if (usuarioEncontrado is null)
            {
                return Respuesta<AuthTokensDTO>.Fail("Usuario no encontrado. Favor validar los datos ingresados.");
            }

            return await GuardarNuevaSesion(usuarioEncontrado.IdUsuario, cancellationToken);
        }
        catch (Exception exception)
        {
            return Respuesta<AuthTokensDTO>.Fail(exception.Message);
        }
    }

    ////Método encargado de generar un nuevo AccessToken y RefreshToken para el usuario utilizando el RefreshToken anterior, garantizando que el RefreshToken suministrado exista y esté activo para ese usuario antes de generar los nuevos tokens
    //public async Task<Respuesta<AuthTokensDTO>> GenerarTokensConRefreshTokenAnterior(int idUsuario, string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var refreshTokenEncontrado = await _autorizacionRepository
    //            .ConsultarUltimoHistorialRefreshTokensPorUsuario(idUsuario, accessToken, refreshToken, cancellationToken);

    //        if (refreshTokenEncontrado is null)
    //        {
    //            return Respuesta<AuthTokensDTO>.Fail("El RefreshToken suministrado no existe o no se encuentra activo para ese usuario.");
    //        }

    //        return await GuardarNuevaSesion(idUsuario, cancellationToken);
    //    }
    //    catch (Exception exception)
    //    {
    //        return Respuesta<AuthTokensDTO>.Fail(exception.Message);
    //    }
    //}

    //Método encargado de generar un nuevo AccessToken para el usuario utilizando el RefreshToken anterior, sin necesidad de generar un nuevo RefreshToken
    //Antes de actualizar el AccessToken, se valida que el AccessToken y el RefreshToken suministrados existan y estén activos para ese usuario
    public async Task<Respuesta<AuthTokensDTO>> ActualizarAccessTokenConRefreshTokenAnterior(int idUsuario, string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshTokenEncontrado = await _autorizacionRepository
                .ConsultarUltimoHistorialRefreshTokensPorUsuario(idUsuario, accessToken, refreshToken, cancellationToken);

            if (refreshTokenEncontrado is null)
            {
                return Respuesta<AuthTokensDTO>.Fail("El AccessToken y/o el RefreshToken suministrados no existen, o el RefreshToken no se encuentra activo para ese usuario.");
            }

            var nuevoAccessToken = _jwtTokenService.GenerarAccessToken(idUsuario);
            var actualizado = await _unitOfWork.EjecutarAccion(
                ct => _autorizacionRepository.ActualizarHistorialRefreshTokenDeUsuario(refreshTokenEncontrado.IdHistorialToken, nuevoAccessToken, ct),
                cancellationToken);

            if (!actualizado)
            {
                return Respuesta<AuthTokensDTO>.Fail("No fue posible actualizar el AccessToken del usuario.");
            }

            return Respuesta<AuthTokensDTO>.Ok(
                new AuthTokensDTO
                {
                    IdUsuario = idUsuario,
                    AccessToken = nuevoAccessToken,
                    RefreshToken = refreshTokenEncontrado.RefreshToken
                },
                "AccessToken actualizado correctamente.");
        }
        catch (Exception exception)
        {
            return Respuesta<AuthTokensDTO>.Fail(exception.Message);
        }
    }

    //Método encargado de consultar la fecha de vencimiento del RefreshToken activo del usuario
    public async Task<DateTime?> ConsultarFechaVencimientoRefreshToken(int idUsuario, CancellationToken cancellationToken = default)
    {
        var refreshTokenEncontrado = await _autorizacionRepository.ConsultarUltimoHistorialRefreshTokensPorUsuario(idUsuario, cancellationToken: cancellationToken);
        return refreshTokenEncontrado?.FechaExpiracion;
    }

    //Método encargado de eliminar todos los tokens activos del usuario para cerrar su sesión, garantizando que no queden tokens válidos después de la operación
    public async Task<Respuesta<bool>> CerrarSesion(int idUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokensUsuario = await _autorizacionRepository.ConsultarUltimoHistorialRefreshTokensPorUsuario(idUsuario, cancellationToken: cancellationToken);
            if (tokensUsuario is null)
            {
                return Respuesta<bool>.Fail($"No existen tokens activos del usuario '{idUsuario}' para eliminar.");
            }

            var esExitoso = await _unitOfWork.EjecutarAccion(
                ct => _autorizacionRepository.EliminarHistorialRefreshTokensPorUsuario(idUsuario, ct),
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

    //Este método se encarga de validar un AccessToken utilizando el servicio de generación y validación de tokens JWT
    public bool ValidarToken(string accessToken)
    {
        return _jwtTokenService.ValidarToken(accessToken);
    }


    #region Métodos PRIVADOS
    //Se genera un nuevo AccessToken y RefreshToken para el usuario, y guardar el nuevo RefreshToken en la BD
    //Antes de guardar el nuevo RefreshToken, se eliminan los tokens anteriores del usuario para garantizar que solo exista un par de tokens activo por usuario
    private async Task<Respuesta<AuthTokensDTO>> GuardarNuevaSesion(int idUsuario, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.GenerarAccessToken(idUsuario);
        var refreshToken = _jwtTokenService.GenerarRefreshToken();
        var fechaCreacion = _dateTimeProvider.ObtenerDateTimeActual();
        var fechaExpiracion = _jwtTokenService.ObtenerFechaExpiracionRefreshToken(fechaCreacion);

        var historialRefreshToken = new HistorialRefreshToken
        {
            IdUsuario = idUsuario,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            FechaCreacion = fechaCreacion,
            FechaExpiracion = fechaExpiracion
        };

        var guardado = await _unitOfWork.EjecutarAccion(
            async ct =>
            {
                await _autorizacionRepository.EliminarHistorialRefreshTokensPorUsuario(idUsuario, ct);
                return await _autorizacionRepository.GuardarHistorialRefreshTokenDeUsuario(historialRefreshToken, ct);
            },
            cancellationToken);

        if (!guardado)
        {
            return Respuesta<AuthTokensDTO>.Fail("Error al momento de generar el AccessToken y el RefreshToken.");
        }

        return Respuesta<AuthTokensDTO>.Ok(
            new AuthTokensDTO
            {
                IdUsuario = idUsuario,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            },
            "AccessToken y RefreshToken generados correctamente.");
    }
    #endregion

}