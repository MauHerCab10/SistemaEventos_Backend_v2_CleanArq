using System;
using System.Collections.Generic;
using System.Text;

namespace Servicio.Interfaz
{
    public interface ICookieService
    {
        void SetCookieAccessToken(string token);

        void SetCookieRefreshToken(string token);

        void EliminarCookiesDelUsuario();
    }
}