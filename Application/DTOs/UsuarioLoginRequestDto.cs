namespace SistemaEventos.Application.DTOs;

public class UsuarioLoginRequestDTO
{
    public required string Email { get; set; }

    public required string Contrasena { get; set; }
}