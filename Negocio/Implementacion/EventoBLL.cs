using Datos.Interfaz;
using Negocio.Interfaz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Transversal.Models;

namespace Negocio.Implementacion
{
    public class EventoBLL : IEventoBLL
    {
        private readonly IEventoDAL _eventoDAL;

        public EventoBLL(IEventoDAL eventoDAL)
        {
            _eventoDAL = eventoDAL;
        }

        //Devuelve todos los eventos disponibles en la BD
        public async Task<Respuesta<List<Evento>>> ConsultarEventosDisponibles(string idUsuario)
        {
            List<Evento>? listaEventosDisponibles = await _eventoDAL.ConsultarEventosDisponibles(idUsuario);

            if (listaEventosDisponibles == null || listaEventosDisponibles.Count <= 0)
                return new Respuesta<List<Evento>> { IsSuccess = false, Mensaje = $"No se encontró ningún Evento para el usuario '{idUsuario}'." };
            else
                return new Respuesta<List<Evento>> { IsSuccess = true, Valor = listaEventosDisponibles };
        }

        //Crea un nuevo evento en la BD
        public async Task<Respuesta<bool>> CrearEvento(Evento evento)
        {
            string fechaHora = $"{evento.Fecha} {evento.Hora}";
            DateTime fechaCompleta = DateTime.ParseExact(fechaHora, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            evento.FechaHora = fechaCompleta;

            bool respuesta = await _eventoDAL.CrearEvento(evento);

            if (respuesta)
                return new Respuesta<bool> { IsSuccess = true, Mensaje = $"¡Evento '{evento.NombreEvento}' creado exitosamente!" };
            else
                return new Respuesta<bool> { IsSuccess = false, Mensaje = "Error al momento de la creación del Evento." };
        }

        //Actualiza un evento
        public async Task<Respuesta<bool>> ModificarEvento(Evento evento)
        {
            string fechaHora = $"{evento.Fecha} {evento.Hora}";
            DateTime fechaCompleta = DateTime.ParseExact(fechaHora, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            evento.FechaHora = fechaCompleta;

            bool respuesta = await _eventoDAL.ModificarEvento(evento);

            if (respuesta)
                return new Respuesta<bool> { IsSuccess = true, Mensaje = $"¡Evento '{evento.NombreEvento}' actualizado exitosamente!" };
            else
                return new Respuesta<bool> { IsSuccess = false, Mensaje = "Error al momento de la actualización del Evento." };
        }

        //Elimina un evento
        public async Task<Respuesta<bool>> EliminarEvento(int idEvento)
        {
            bool respuesta = await _eventoDAL.EliminarEvento(idEvento);

            if (respuesta)
                return new Respuesta<bool> { IsSuccess = true, Mensaje = $"¡Evento '{idEvento}' eliminado exitosamente!" };
            else
                return new Respuesta<bool> { IsSuccess = false, Mensaje = "Error al momento de eliminar el Evento." };
        }

        //Inscribe a un usuario a un evento
        public async Task<Respuesta<bool>> InscripcionAEvento(int idEvento, int idUsuario)
        {
            bool respuesta = await _eventoDAL.InscripcionAEvento(idEvento, idUsuario);

            if (respuesta)
                return new Respuesta<bool> { IsSuccess = true, Mensaje = $"¡Usuario '{idUsuario}' inscrito al evento '{idEvento}' satisfactoriamente!" };
            else
                return new Respuesta<bool> { IsSuccess = false, Mensaje = "Error al momento de la incripción del usuario al Evento." };
        }

        //Da de baja a un usuario de un evento
        public async Task<Respuesta<bool>> DimisionDeEvento(int idEvento, int idUsuario)
        {
            bool respuesta = await _eventoDAL.DimisionDeEvento(idEvento, idUsuario);

            if (respuesta)
                return new Respuesta<bool> { IsSuccess = true, Mensaje = $"¡Usuario '{idUsuario}' dado de baja del evento '{idEvento}' satisfactoriamente!" };
            else
                return new Respuesta<bool> { IsSuccess = false, Mensaje = "Error al momento del usuario darse de baja del Evento." };
        }

    }
}