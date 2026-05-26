using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.Interfaces.Persistence;

public interface IEventoRepository
{
    Task<List<Evento>> ConsultarEventosDisponiblesAsync(string idUsuario, CancellationToken cancellationToken = default);

    Task<bool> CrearEventoAsync(Evento evento, CancellationToken cancellationToken = default);

    Task<bool> ModificarEventoAsync(Evento evento, CancellationToken cancellationToken = default);

    Task<bool> EliminarEventoAsync(int idEvento, CancellationToken cancellationToken = default);

    Task<bool> InscripcionAEventoAsync(int idEvento, int idUsuario, CancellationToken cancellationToken = default);

    Task<bool> DimisionDeEventoAsync(int idEvento, int idUsuario, CancellationToken cancellationToken = default);
}