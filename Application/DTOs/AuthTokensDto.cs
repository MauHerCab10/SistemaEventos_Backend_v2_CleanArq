namespace SistemaEventos.Application.DTOs;

public class AuthTokensDTO
{
    public int IdUsuario { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}