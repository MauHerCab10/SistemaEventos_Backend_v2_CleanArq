using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Infrastructure.Configuration;
using SistemaEventos.Infrastructure.Persistence;
using SistemaEventos.Infrastructure.Persistence.Repositories;
using SistemaEventos.Infrastructure.Services;

namespace SistemaEventos.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettingsOptions>(configuration.GetSection("JwtSettings"));
        services.Configure<ServidorEmailOptions>(configuration.GetSection("ServidorEmail"));

        services.AddMemoryCache();

        services.AddSingleton<SqlConnectionFactory>();
        services.AddScoped<SqlConnectionContext>();
        services.AddScoped<IUnitOfWork, SqlUnitOfWork>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IAutorizacionRepository, AutorizacionRepository>();
        services.AddScoped<IEventoRepository, EventoRepository>();
        services.AddScoped<IPlantillaCorreoRepository, PlantillaCorreoRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddSingleton<IGuidAccessSettings, GuidAccessSettings>();
        services.AddScoped<IPlantillaCorreoProvider, PlantillaCorreoProvider>();

        return services;
    }
}