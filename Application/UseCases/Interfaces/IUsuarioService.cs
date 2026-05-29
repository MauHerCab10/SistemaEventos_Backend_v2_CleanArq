using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;

namespace SistemaEventos.Application.UseCases.Interfaces;

public interface IUsuarioService
{
    Task<Respuesta<UsuarioResponseDTO>> AutenticarUsuario(UsuarioLoginRequestDTO DTOUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDTO>> RegistrarUsuario(UsuarioRegistroRequestDTO DTOUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDTO>> OlvidoSuContrasena(string email, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDTO>> ActualizarContrasenaAntigua(string guidAcceso, string nuevaContrasena, string confirmacionContrasena, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDTO>> ConfirmarCuenta(string guidAcceso, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDTO>> AutenticarUsuarioGoogle(UsuarioGoogleRequestDTO DTOUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDTO>> RegistrarUsuarioGoogle(UsuarioGoogleRequestDTO DTOUsuario, CancellationToken cancellationToken = default);
}