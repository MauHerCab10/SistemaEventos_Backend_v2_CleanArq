using System;
using System.Collections.Generic;
using System.Text;

namespace Transversal.DTOs
{
    public class ActualizarContrasenaDTO
    {
        public required string GuidAcceso { get; set; }

        public required string NuevaContrasena { get; set; }

        public required string ConfirmacionContrasena { get; set; }
    }
}