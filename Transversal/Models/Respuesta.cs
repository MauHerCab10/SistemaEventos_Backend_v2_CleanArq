using System;
using System.Collections.Generic;
using System.Text;

namespace Transversal.Models
{
    public class Respuesta<T>
    {
        public bool IsSuccess { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public T Valor { get; set; } = default!;
    }
}