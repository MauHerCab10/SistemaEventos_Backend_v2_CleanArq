using System;
using System.Collections.Generic;
using System.Text;
using Transversal.Models;

namespace Datos.Interfaz
{
    public interface IUsuarioDAL
    {
        Task<bool> RegistrarUsuario(Usuario usuario);

        Task<Usuario> ConsultarUsuarioPorEmail(string email);

        Task<Usuario> ConsultarUsuarioPorGuid(string guidUsuario);

        Task<bool> RestablecerContrasena(Usuario usuarioRestablecido);

        Task<bool> ActualizarContrasenaAntigua(string guidAcceso, string contrasenaHash);

        Task<bool> ConfirmarCuenta(string guidAcceso);
    }
}