using System;
using System.Collections.Generic;
using System.Text;

namespace Transversal.Models
{
    public class PlantillaCorreo
    {
        public required string Nombre { get; set; }

        public required string Asunto { get; set; }

        public required string Cuerpo { get; set; }
    }
}