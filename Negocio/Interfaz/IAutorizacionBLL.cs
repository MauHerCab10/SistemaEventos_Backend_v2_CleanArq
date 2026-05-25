using System;
using System.Collections.Generic;
using System.Text;
using Transversal.Models;

namespace Negocio.Interfaz
{
    public interface IAutorizacionBLL
    {
        Task<Respuesta<Usuario>> GenerarAccessTokenYRefreshTokenConCredenciales(string email);

        Task<Respuesta<Usuario>> GenerarAccessTokenYRefreshTokenConRefreshTokenAnterior(int idUsuario, string accessToken, string refreshToken);

        Task<DateTime?> ConsultarFechaVencimientoRefreshToken(int idUsuario);

        Task<Respuesta<Usuario>> ActualizarAccessTokenConRefreshTokenAnterior(int idUsuario, string accessToken, string refreshToken);

        Task<Respuesta<Usuario>> CerrarSesion(int idUsuario);

        bool ValidarToken(string token);
    }
}