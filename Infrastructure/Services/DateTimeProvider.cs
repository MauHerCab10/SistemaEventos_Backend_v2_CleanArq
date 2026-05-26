using Microsoft.Extensions.Options;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Infrastructure.Configuration;

namespace SistemaEventos.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    private readonly JwtSettingsOptions _jwtSettings;

    public DateTimeProvider(IOptions<JwtSettingsOptions> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public DateTime GetCurrentDateTime()
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_jwtSettings.TimeZone);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
    }
}