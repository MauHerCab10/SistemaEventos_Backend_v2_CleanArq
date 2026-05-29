using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.Interfaces.Persistence;

public interface IAutorizacionRepository
{
    Task<HistorialRefreshToken?> ConsultarUltimoHistorialRefreshTokensPorUsuario(
        int idUsuario,
        string? accessToken = null,
        string? refreshToken = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> GuardarHistorialRefreshTokenDeUsuario(
        HistorialRefreshToken historialRefreshToken,
        CancellationToken cancellationToken = default
    );

    Task<bool> ActualizarHistorialRefreshTokenDeUsuario(
        int idHistorialToken,
        string accessToken,
        CancellationToken cancellationToken = default
    );

    Task<bool> EliminarHistorialRefreshTokensPorUsuario(
        int idUsuario,
        CancellationToken cancellationToken = default
    );

}