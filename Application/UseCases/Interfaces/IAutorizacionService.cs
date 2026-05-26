using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;

namespace SistemaEventos.Application.UseCases.Interfaces;

public interface IAutorizacionService
{
    Task<Respuesta<AuthTokensDto>> GenerarTokensConCredencialesAsync(string email, CancellationToken cancellationToken = default);

    Task<Respuesta<AuthTokensDto>> GenerarTokensConRefreshTokenAnteriorAsync(int idUsuario, string accessToken, string refreshToken, CancellationToken cancellationToken = default);

    Task<Respuesta<AuthTokensDto>> ActualizarAccessTokenConRefreshTokenAnteriorAsync(int idUsuario, string accessToken, string refreshToken, CancellationToken cancellationToken = default);

    Task<DateTime?> ConsultarFechaVencimientoRefreshTokenAsync(int idUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> CerrarSesionAsync(int idUsuario, CancellationToken cancellationToken = default);

    bool ValidarToken(string accessToken);
}