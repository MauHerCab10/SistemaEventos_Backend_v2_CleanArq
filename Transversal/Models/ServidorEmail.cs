using System;
using System.Collections.Generic;
using System.Text;

namespace Transversal.Models
{
    public class ServidorEmail
    {
        public required string Host { get; set; }

        public required string Port { get; set; }

        public required string Username { get; set; }

        public required string Password { get; set; }
    }
}