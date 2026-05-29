using Microsoft.Extensions.DependencyInjection;
using SistemaEventos.Application.UseCases.Implementations;
using SistemaEventos.Application.UseCases.Interfaces;

namespace SistemaEventos.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAutorizacionService, AutorizacionService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IEventoService, EventoService>();

        return services;
    }
}