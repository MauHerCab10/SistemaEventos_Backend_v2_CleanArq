using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;

namespace SistemaEventos.Application.UseCases.Interfaces;

public interface IUsuarioService
{
    Task<Respuesta<UsuarioResponseDto>> AutenticarUsuarioAsync(UsuarioLoginRequestDto dtoUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDto>> RegistrarUsuarioAsync(UsuarioRegistroRequestDto dtoUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDto>> OlvidoSuContrasenaAsync(string email, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDto>> ActualizarContrasenaAntiguaAsync(string guidAcceso, string nuevaContrasena, string confirmacionContrasena, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDto>> ConfirmarCuentaAsync(string guidAcceso, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDto>> AutenticarUsuarioGoogleAsync(UsuarioGoogleRequestDto dtoUsuario, CancellationToken cancellationToken = default);

    Task<Respuesta<UsuarioResponseDto>> RegistrarUsuarioGoogleAsync(UsuarioGoogleRequestDto dtoUsuario, CancellationToken cancellationToken = default);
}