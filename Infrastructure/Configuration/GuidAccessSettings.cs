using Microsoft.Extensions.Configuration;
using SistemaEventos.Application.Interfaces.Services;

namespace SistemaEventos.Infrastructure.Configuration;

public class GuidAccessSettings : IGuidAccessSettings
{
    public GuidAccessSettings(IConfiguration configuration)
    {
        GuidAccesoExpirationMinutes = configuration.GetValue<int>("GuidAcceso_ExpirationTime");
    }

    public int GuidAccesoExpirationMinutes { get; }
}