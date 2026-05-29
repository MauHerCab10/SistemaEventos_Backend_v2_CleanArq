using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;

namespace SistemaEventos.Application.UseCases.Interfaces;

public interface IAutorizacionService
{
    Task<Respuesta<AuthTokensDTO>> GenerarTokensConCredenciales(string email, CancellationToken cancellationToken = default);

    //Task<Respuesta<AuthTokensDTO>> GenerarTokensConRefreshTokenAnterior(int idUsuario, string accessToken, string refreshToken, CancellationToken cancellationToken = default);

    Task<Respuesta<AuthTokensDTO>> ActualizarAccessTokenConRefreshTokenAnterior(int idUsuario, string accessToken, string refreshToken, CancellationToken cancellationToken = default);

    Task<DateTime?> ConsultarFechaVencimientoRefreshToken(int idUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> CerrarSesion(int idUsuario, CancellationToken cancellationToken = default);

    bool ValidarToken(string accessToken);
}