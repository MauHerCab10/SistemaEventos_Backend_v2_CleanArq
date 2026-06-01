using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Application.Mappings;
using SistemaEventos.Application.UseCases.Interfaces;

namespace SistemaEventos.Application.UseCases.Implementations;

public class EventoService : IEventoService
{
    private readonly IEventoRepository _eventoRepository;

    public EventoService(IEventoRepository eventoRepository)
    {
        _eventoRepository = eventoRepository;
    }

    //Devuelve todos los eventos disponibles en la BD
    public async Task<Respuesta<List<EventoDTO>>> ConsultarEventosDisponibles(int idUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var listaEventosDisponibles = await _eventoRepository.ConsultarEventosDisponibles(idUsuario, cancellationToken);

            if (listaEventosDisponibles.Count == 0)
            {
                return Respuesta<List<EventoDTO>>.Fail($"No se encontró ningún evento para el usuario '{idUsuario}'.");
            }

            return Respuesta<List<EventoDTO>>.Ok(listaEventosDisponibles.Select(evento => evento.ToDTO()).ToList());
        }
        catch (Exception exception)
        {
            return Respuesta<List<EventoDTO>>.Fail(exception.Message);
        }
    }

    //Crea un nuevo evento en la BD
    public async Task<Respuesta<bool>> CrearEvento(EventoDTO evento, CancellationToken cancellationToken = default)
    {
        try
        {
            var entidad = evento.ToEntity();

            var respuesta = await _eventoRepository.CrearEvento(entidad, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Evento '{evento.NombreEvento}' creado exitosamente.")
                : Respuesta<bool>.Fail("Error al momento de la creación del evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    //Actualiza un evento existente en la BD
    public async Task<Respuesta<bool>> ModificarEvento(EventoDTO evento, CancellationToken cancellationToken = default)
    {
        try
        {
            var entidad = evento.ToEntity();

            var respuesta = await _eventoRepository.ModificarEvento(entidad, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Evento '{evento.NombreEvento}' actualizado exitosamente.")
                : Respuesta<bool>.Fail("Error al momento de la actualización del evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    //Elimina un evento existente en la BD
    public async Task<Respuesta<bool>> EliminarEvento(int idEvento, CancellationToken cancellationToken = default)
    {
        try
        {
            var respuesta = await _eventoRepository.EliminarEvento(idEvento, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Evento '{idEvento}' eliminado exitosamente.")
                : Respuesta<bool>.Fail("Error al momento de eliminar el evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    //Permite a un usuario inscribirse en un evento específico
    public async Task<Respuesta<bool>> InscripcionAEvento(int idEvento, int idUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var respuesta = await _eventoRepository.InscripcionAEvento(idEvento, idUsuario, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Usuario '{idUsuario}' inscrito al evento '{idEvento}' satisfactoriamente.")
                : Respuesta<bool>.Fail("Error al momento de la inscripción del usuario al evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

    //Permite a un usuario darse de baja de un evento específico
    public async Task<Respuesta<bool>> DimisionDeEvento(int idEvento, int idUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var respuesta = await _eventoRepository.DimisionDeEvento(idEvento, idUsuario, cancellationToken);
            return respuesta
                ? Respuesta<bool>.Ok(true, $"Usuario '{idUsuario}' dado de baja del evento '{idEvento}' satisfactoriamente.")
                : Respuesta<bool>.Fail("Error al momento del usuario darse de baja del evento.");
        }
        catch (Exception exception)
        {
            return Respuesta<bool>.Fail(exception.Message);
        }
    }

}