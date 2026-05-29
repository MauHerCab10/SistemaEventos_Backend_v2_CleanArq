using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;

namespace SistemaEventos.Application.UseCases.Interfaces;

public interface IEventoService
{
    Task<Respuesta<List<EventoDTO>>> ConsultarEventosDisponibles(int idUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> CrearEvento(EventoDTO evento, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> ModificarEvento(EventoDTO evento, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> EliminarEvento(int idEvento, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> InscripcionAEvento(int idEvento, int idUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> DimisionDeEvento(int idEvento, int idUsuario, CancellationToken cancellationToken = default);
}