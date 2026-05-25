using System;
using System.Collections.Generic;
using System.Text;
using Transversal.Models;

namespace Servicio.Interfaz
{
    public interface IUtilidades
    {
        string GenerarGuid();

        string EncriptarContraseña(string contrasena);

        bool VerificarContrasena(string contrasena, string contrasenaHashGuardada);

        bool EnviarCorreo(InfoCorreo request);

        DateTime FechaHoraActualColombia();
    }
}