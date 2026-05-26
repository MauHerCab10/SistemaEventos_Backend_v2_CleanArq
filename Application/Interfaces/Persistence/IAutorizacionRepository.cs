using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.Interfaces.Persistence;

public interface IAutorizacionRepository
{
    Task<HistorialRefreshToken?> ConsultarUltimoHistorialRefreshTokensPorUsuarioAsync(
        int idUsuario,
        string? accessToken = null,
        string? refreshToken = null,
        CancellationToken cancellationToken = default);

    Task<bool> GuardarHistorialRefreshTokenDeUsuarioAsync(
        HistorialRefreshToken historialRefreshToken,
        CancellationToken cancellationToken = default);

    Task<bool> ActualizarHistorialRefreshTokenDeUsuarioAsync(
        int idHistorialToken,
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<bool> EliminarHistorialRefreshTokensPorUsuarioAsync(
        int idUsuario,
        CancellationToken cancellationToken = default);
}