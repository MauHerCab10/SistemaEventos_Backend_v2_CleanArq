using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Infrastructure.DependencyInjection;
using SistemaEventos.Server.Middleware;
using SistemaEventos.Server.Services;

namespace SistemaEventos.Server.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string CorsPolicyName = "PolicyCORS";

    public static IServiceCollection AddServer(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddInfrastructure(configuration);
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<IAccountUrlBuilder, AccountUrlBuilder>();

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

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var cookieToken = context.Request.Cookies["cookieAccessToken"];
                    if (!string.IsNullOrWhiteSpace(cookieToken))
                    {
                        context.Token = cookieToken;
                        return Task.CompletedTask;
                    }

                    var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = authorizationHeader[7..].Trim();
                    }

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

    public static WebApplication UseServer(this WebApplication app, IWebHostEnvironment environment)
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
        app.UseMiddleware<TokenRefreshMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}