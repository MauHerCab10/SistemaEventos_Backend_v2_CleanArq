namespace SistemaEventos.Application.Interfaces.Services;

public interface IAccountUrlBuilder
{
    string BuildConfirmacionCuentaUrl(string guidAcceso);

    string BuildRestablecerContrasenaUrl(string guidAcceso);
}