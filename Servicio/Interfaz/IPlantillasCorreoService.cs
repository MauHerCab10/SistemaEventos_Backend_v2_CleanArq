using System;
using System.Collections.Generic;
using System.Text;
using Transversal.Models;

namespace Servicio.Interfaz
{
    public interface IPlantillasCorreoService
    {
        Task<List<PlantillaCorreo>> CargarPlantillasCorreoDesdeDB();
    }
}