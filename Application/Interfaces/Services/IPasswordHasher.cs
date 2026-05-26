namespace SistemaEventos.Application.Interfaces.Services;

public interface IPasswordHasher
{
    string Hash(string value);

    bool Verify(string plainText, string hash);

    string GenerarGuid();
}