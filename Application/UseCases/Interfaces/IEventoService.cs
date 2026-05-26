using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;

namespace SistemaEventos.Application.UseCases.Interfaces;

public interface IEventoService
{
    Task<Respuesta<List<EventoDto>>> ConsultarEventosDisponiblesAsync(string idUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> CrearEventoAsync(EventoDto evento, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> ModificarEventoAsync(EventoDto evento, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> EliminarEventoAsync(int idEvento, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> InscripcionAEventoAsync(int idEvento, int idUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<bool>> DimisionDeEventoAsync(int idEvento, int idUsuario, CancellationToken cancellationToken = default);
}