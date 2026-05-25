using System;
using System.Collections.Generic;
using System.Text;
using Transversal.Models;

namespace Negocio.Interfaz
{
    public interface IEventoBLL
    {
        Task<Respuesta<List<Evento>>> ConsultarEventosDisponibles(string idUsuario);

        Task<Respuesta<bool>> CrearEvento(Evento evento);

        Task<Respuesta<bool>> ModificarEvento(Evento evento);

        Task<Respuesta<bool>> EliminarEvento(int idEvento);

        Task<Respuesta<bool>> InscripcionAEvento(int idEvento, int idUsuario);

        Task<Respuesta<bool>> DimisionDeEvento(int idEvento, int idUsuario);
    }
}