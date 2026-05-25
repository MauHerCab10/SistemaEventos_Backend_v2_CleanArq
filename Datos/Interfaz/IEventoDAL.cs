using System;
using System.Collections.Generic;
using System.Text;
using Transversal.Models;

namespace Datos.Interfaz
{
    public interface IEventoDAL
    {
        Task<List<Evento>> ConsultarEventosDisponibles(string idUsuario);

        Task<bool> CrearEvento(Evento evento);

        Task<bool> ModificarEvento(Evento evento);

        Task<bool> EliminarEvento(int idEvento);

        Task<bool> InscripcionAEvento(int idEvento, int idUsuario);

        Task<bool> DimisionDeEvento(int idEvento, int idUsuario);
    }
}