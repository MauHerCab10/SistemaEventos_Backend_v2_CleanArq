using System;
using System.Collections.Generic;
using System.Text;

namespace Transversal.DTOs
{
    public class UsuarioLoginRequestDTO
    {
        public required string Email { get; set; }

        public required string Contrasena { get; set; }
    }
}