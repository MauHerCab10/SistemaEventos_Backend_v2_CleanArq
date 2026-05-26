namespace SistemaEventos.Domain.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreApellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string ContrasenaHash { get; set; } = string.Empty;

    public bool Restablecer { get; set; }

    public bool Confirmado { get; set; }

    public string GuidAcceso { get; set; } = string.Empty;

    public DateTime FechaCreacionGuid { get; set; }

    public DateTime FechaExpiracionGuid { get; set; }

    public bool GuidActivo { get; set; }

    public bool GuidValidado { get; set; }

    public void PrepararNuevoRegistro(string contrasenaHash, string guidAcceso, DateTime fechaActual, int minutosExpiracion)
    {
        ContrasenaHash = contrasenaHash;
        Restablecer = false;
        Confirmado = false;
        GuidAcceso = guidAcceso;
        FechaCreacionGuid = fechaActual;
        FechaExpiracionGuid = fechaActual.AddMinutes(minutosExpiracion);
        GuidValidado = false;
    }

    public void PrepararRestablecimiento(string guidAcceso, DateTime fechaActual, int minutosExpiracion)
    {
        GuidAcceso = guidAcceso;
        FechaCreacionGuid = fechaActual;
        FechaExpiracionGuid = fechaActual.AddMinutes(minutosExpiracion);
        ContrasenaHash = string.Empty;
        Restablecer = true;
        Confirmado = true;
        GuidValidado = false;
    }
}