using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.Interfaces.Persistence;

public interface IEventoRepository
{
    Task<List<Evento>> ConsultarEventosDisponibles(int idUsuario, CancellationToken cancellationToken = default);

    Task<bool> CrearEvento(Evento evento, CancellationToken cancellationToken = default);

    Task<bool> ModificarEvento(Evento evento, CancellationToken cancellationToken = default);

    Task<bool> EliminarEvento(int idEvento, CancellationToken cancellationToken = default);

    Task<bool> InscripcionAEvento(int idEvento, int idUsuario, CancellationToken cancellationToken = default);

    Task<bool> DimisionDeEvento(int idEvento, int idUsuario, CancellationToken cancellationToken = default);
}