using System;
using System.Collections.Generic;
using System.Text;

namespace Transversal.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        public string NombreApellido { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Contrasena { get; set; } = string.Empty;

        public string ContrasenaHash { get; set; } = string.Empty;

        public bool Restablecer { get; set; }

        public bool Confirmado { get; set; }

        public string GuidAcceso { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime FechaCreacionGuid { get; set; }

        public DateTime FechaExpiracionGuid { get; set; }

        public bool GuidActivo { get; set; }

        public bool GuidValidado { get; set; }
    }
}