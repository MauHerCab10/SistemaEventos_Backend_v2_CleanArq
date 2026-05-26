namespace SistemaEventos.Application.DTOs;

public class AuthTokensDto
{
    public int IdUsuario { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}