namespace SistemaEventos.Application.Interfaces.Services;

public interface IAccountUrlBuilder
{
    string ArmarUrlConfirmacionCuenta(string guidAcceso);

    string ArmarUrlRestablecerContrasena(string guidAcceso);
}