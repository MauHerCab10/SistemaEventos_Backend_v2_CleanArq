using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.Interfaces.Persistence;

public interface IUsuarioRepository
{
    Task<bool> RegistrarUsuario(Usuario usuario, CancellationToken cancellationToken = default);

    Task<Usuario?> ConsultarUsuarioPorEmail(string email, CancellationToken cancellationToken = default);

    Task<Usuario?> ConsultarUsuarioPorGuid(string guidUsuario, CancellationToken cancellationToken = default);

    Task<bool> RestablecerContrasena(Usuario usuarioRestablecido, CancellationToken cancellationToken = default);

    Task<bool> ActualizarContrasenaAntigua(string guidAcceso, string contrasenaHash, CancellationToken cancellationToken = default);

    Task<bool> ConfirmarCuenta(string guidAcceso, CancellationToken cancellationToken = default);
}