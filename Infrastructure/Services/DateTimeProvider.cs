using Microsoft.Extensions.Options;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Infrastructure.Configuration;

namespace SistemaEventos.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    private readonly JwtSettingsOptions _jwtSettings;

    //El constructor recibe la configuración de JwtSettingsOptions a través de IOptions, lo que permite acceder a los valores configurados del "appsettings.json" relacionados con JWT
    public DateTimeProvider(IOptions<JwtSettingsOptions> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    // Devuelve la fecha y hora actual en la zona horaria especificada en la configuración de JwtSettings
    public DateTime ObtenerDateTimeActual()
    {
        var infoTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_jwtSettings.TimeZone);
        var timeZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, infoTimeZone);
        return timeZone;
    }
}