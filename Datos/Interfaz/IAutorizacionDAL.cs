using System;
using System.Collections.Generic;
using System.Text;
using Transversal.Models;

namespace Datos.Interfaz
{
    public interface IAutorizacionDAL
    {
        Task<HistorialRefreshToken> ConsultarUltimoHistorialRefreshTokensPorUsuario(int idUsuario, string? accessToken, string? refreshToken);

        Task<bool> GuardarHistorialRefreshTokenDeUsuario(int idUsuario, string accessToken, string refreshToken, DateTime fechaCreacion, DateTime fechaExpiracion);

        Task<bool> ActualizarHistorialRefreshTokenDeUsuario(int idHistorialToken, string accessToken);

        Task<bool> EliminarHistorialRefreshTokensPorUsuario(int idUsuario);
    }
}