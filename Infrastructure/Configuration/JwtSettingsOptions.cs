namespace SistemaEventos.Infrastructure.Configuration;

public class JwtSettingsOptions
{
    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string TimeZone { get; set; } = "UTC";

    public int CantidadHorasRestarZonaHoraria { get; set; }

    public int AccessToken_ExpirationTime { get; set; }

    public int RefreshToken_ExpirationTime { get; set; }
}