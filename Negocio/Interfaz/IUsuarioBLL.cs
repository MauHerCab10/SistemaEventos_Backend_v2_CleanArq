using System;
using System.Collections.Generic;
using System.Text;
using Transversal.DTOs;
using Transversal.Models;

namespace Negocio.Interfaz
{
    public interface IUsuarioBLL
    {
        Task<Respuesta<UsuarioResponseDTO>> AutenticarUsuario(UsuarioLoginRequestDTO dtoUsuario);

        Task<Respuesta<UsuarioResponseDTO>> RegistrarUsuario(UsuarioRegistroRequestDTO dtoUsuario);

        Task<Respuesta<UsuarioResponseDTO>> OlvidoSuContrasena(string email);

        Task<Respuesta<UsuarioResponseDTO>> ActualizarContrasenaAntigua(string guidAcceso, string nuevaContrasena, string confirmacionContrasena);

        Task<Respuesta<UsuarioResponseDTO>> ConfirmarCuenta(string guidAcceso);

        Task<Respuesta<UsuarioResponseDTO>> AutenticarUsuarioGoogle(UsuarioGoogleRequestDTO dtoUsuario);

        Task<Respuesta<UsuarioResponseDTO>> RegistrarUsuarioGoogle(UsuarioGoogleRequestDTO dtoUsuario);
    }
}