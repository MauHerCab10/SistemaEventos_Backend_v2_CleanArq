using System;
using System.Collections.Generic;
using System.Text;

namespace Transversal.Models
{
    public class HistorialRefreshToken
    {
        public int IdHistorialToken { get; set; }

        public int IdUsuario { get; set; }

        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaExpiracion { get; set; }

        public bool EstaActivo { get; set; }
    }
}