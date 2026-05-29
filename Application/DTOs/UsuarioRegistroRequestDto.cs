namespace SistemaEventos.Application.DTOs;

public class UsuarioRegistroRequestDTO
{
    public string NombreApellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;
}