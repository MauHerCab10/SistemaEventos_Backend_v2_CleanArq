namespace SistemaEventos.Application.DTOs;

public class ActualizarContrasenaDto
{
    public required string GuidAcceso { get; set; }

    public required string NuevaContrasena { get; set; }

    public required string ConfirmacionContrasena { get; set; }
}