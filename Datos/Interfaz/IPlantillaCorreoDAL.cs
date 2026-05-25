using System;
using System.Collections.Generic;
using System.Text;
using Transversal.Models;

namespace Datos.Interfaz
{
    public interface IPlantillaCorreoDAL
    {
        Task<List<PlantillaCorreo>> ObtenerPlantillasCorreo();
    }
}