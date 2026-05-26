namespace SistemaEventos.Application.DTOs;

public class UsuarioLoginRequestDto
{
    public required string Email { get; set; }

    public required string Contrasena { get; set; }
}