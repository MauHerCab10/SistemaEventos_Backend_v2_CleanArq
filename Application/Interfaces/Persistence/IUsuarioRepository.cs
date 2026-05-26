using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.Interfaces.Persistence;

public interface IUsuarioRepository
{
    Task<bool> RegistrarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default);

    Task<Usuario?> ConsultarUsuarioPorEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Usuario?> ConsultarUsuarioPorGuidAsync(string guidUsuario, CancellationToken cancellationToken = default);

    Task<bool> RestablecerContrasenaAsync(Usuario usuarioRestablecido, CancellationToken cancellationToken = default);

    Task<bool> ActualizarContrasenaAntiguaAsync(string guidAcceso, string contrasenaHash, CancellationToken cancellationToken = default);

    Task<bool> ConfirmarCuentaAsync(string guidAcceso, CancellationToken cancellationToken = default);
}