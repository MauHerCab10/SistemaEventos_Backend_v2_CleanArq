namespace SistemaEventos.Application.Interfaces.Services;

public interface IPasswordHasher
{
    string EncriptarContraseña(string value);

    bool VerificarContrasena(string contrasenaPlana, string contrasenaHashGuardada);

    string GenerarGuid();
}