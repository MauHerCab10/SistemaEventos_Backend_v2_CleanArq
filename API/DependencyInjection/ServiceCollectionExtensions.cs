using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SistemaEventos.Application.DependencyInjection;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Infrastructure.DependencyInjection;
using SistemaEventos.Server.Middleware;
using SistemaEventos.Server.Services;

namespace SistemaEventos.Server.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string CorsPolicyName = "PolicyCORS";

    public static IServiceCollection InicializarServidor(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Add services to the container.

        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<IAccountUrlBuilder, AccountUrlBuilder>();
        services.AddScoped<IFrontendUrlBuilder, AccountUrlBuilder>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"] ?? string.Empty))
            };

            //configuración para obtener el AccessToken de las Cookies
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    //Primero intenta leer la Cookie del header Authorization
                    var token = context.Request.Headers["Authorization"]
                        .FirstOrDefault()?
                        .Split(" ")
                        .Last();

                    ////Si no está en el header, busca como tal en la Cookie
                    //if (string.IsNullOrEmpty(token))
                    //    token = context.Request.Cookies["cookieAccessToken"];

                    //Si encontró el valor del AccessToken, entonces lo asigna y lo retorna
                    if (!string.IsNullOrEmpty(token))
                        context.Token = token;

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Autenticación fallida: {context.Exception.Message}");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy
                    .WithOrigins(
                        configuration["Frontend_URLs:Desarrollo"] ?? string.Empty,
                        configuration["Frontend_URLs:Produccion"] ?? string.Empty)
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static WebApplication UsarServidor(this WebApplication app, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
        }

        app.UseCors(CorsPolicyName);
        app.UseHttpsRedirection();
        app.UseRouting();

        //Middlewares (el orden de ejecución va de arriba para abajo)
        app.UseMiddleware<AdministradorHeadersMiddleware>();
        //app.UseMiddleware<SessionTimeoutMiddleware>(); //se apaga ya q en el Frontend se valida la actividad del usuario (este middleware solo tiene en cuenta las peticiones q lleguen al Backend)

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}