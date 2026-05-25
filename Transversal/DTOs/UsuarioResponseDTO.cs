using System;
using System.Collections.Generic;
using System.Text;

namespace Transversal.DTOs
{
    public class UsuarioResponseDTO
    {
        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;
    }
}