using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Application.UseCases.Interfaces;
using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.UseCases.Implementations;

public class EventoService : IEventoService
{
    private readonly IEventoRepository _eventoRepository;

    public EventoService(IEventoRepository eventoRepository)
    {
        _eventoRepository = eventoRepository;
    }

    public async Task<Respuesta<List<EventoDto>>> ConsultarEventosDisponiblesAsync(string idUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var listaEventosDisponibles = await _eventoRepository.ConsultarEventosDisponiblesAsync(idUsuario, cancellationToken);

            if (listaEventosDisponibles.Count == 0)
            {
                return Respuesta<List<EventoDto>>.Fail($"No se encontro ningun evento para el usuario '{idUsuario}'.");
            }

            return Respuesta<List<EventoDto>>.Ok(listaEventosDisponibles.Select(Map).ToList());
        }
        catch (Exception exception)
        {
            return Respuesta<List<EventoDto>>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<bool>> CrearEventoAsync(EventoDto evento, CancellationToken cancellationToken = default)
    {
        try
        {
            var entidad = Map(evento);
            entidad.SincronizarFechaHora();

            var respuesta = await _eventoRepository.CrearEventoAsync(entidad, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Evento '{evento.NombreEvento}' creado exitosamente.")
                : Respuesta<bool>.Fail("Error al momento de la creacion del evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<bool>> ModificarEventoAsync(EventoDto evento, CancellationToken cancellationToken = default)
    {
        try
        {
            var entidad = Map(evento);
            entidad.SincronizarFechaHora();

            var respuesta = await _eventoRepository.ModificarEventoAsync(entidad, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Evento '{evento.NombreEvento}' actualizado exitosamente.")
                : Respuesta<bool>.Fail("Error al momento de la actualizacion del evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<bool>> EliminarEventoAsync(int idEvento, CancellationToken cancellationToken = default)
    {
        try
        {
            var respuesta = await _eventoRepository.EliminarEventoAsync(idEvento, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Evento '{idEvento}' eliminado exitosamente.")
                : Respuesta<bool>.Fail("Error al momento de eliminar el evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<bool>> InscripcionAEventoAsync(int idEvento, int idUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var respuesta = await _eventoRepository.InscripcionAEventoAsync(idEvento, idUsuario, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Usuario '{idUsuario}' inscrito al evento '{idEvento}' satisfactoriamente.")
                : Respuesta<bool>.Fail("Error al momento de la inscripcion del usuario al evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<bool>> DimisionDeEventoAsync(int idEvento, int idUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var respuesta = await _eventoRepository.DimisionDeEventoAsync(idEvento, idUsuario, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Usuario '{idUsuario}' dado de baja del evento '{idEvento}' satisfactoriamente.")
                : Respuesta<bool>.Fail("Error al momento del usuario darse de baja del evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    private static Evento Map(EventoDto dto)
    {
        return new Evento
        {
            IdEvento = dto.IdEvento,
            NombreEvento = dto.NombreEvento,
            Descripcion = dto.Descripcion,
            Fecha = dto.Fecha,
            Hora = dto.Hora,
            FechaHora = dto.FechaHora,
            Direccion_Ubicacion = dto.Direccion_Ubicacion,
            CapMaxPermitida = dto.CapMaxPermitida,
            IdUsuarioCreacion = dto.IdUsuarioCreacion,
            CantidadAsistentes = dto.CantidadAsistentes,
            CuposDisponibles = dto.CuposDisponibles,
            EsUsuarioInscrito = dto.EsUsuarioInscrito
        };
    }

    private static EventoDto Map(Evento evento)
    {
        return new EventoDto
        {
            IdEvento = evento.IdEvento,
            NombreEvento = evento.NombreEvento,
            Descripcion = evento.Descripcion,
            Fecha = evento.Fecha,
            Hora = evento.Hora,
            FechaHora = evento.FechaHora,
            Direccion_Ubicacion = evento.Direccion_Ubicacion,
            CapMaxPermitida = evento.CapMaxPermitida,
            IdUsuarioCreacion = evento.IdUsuarioCreacion,
            CantidadAsistentes = evento.CantidadAsistentes,
            CuposDisponibles = evento.CuposDisponibles,
            EsUsuarioInscrito = evento.EsUsuarioInscrito
        };
    }
}