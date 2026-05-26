using Microsoft.AspNetCore.Http.Extensions;
using SistemaEventos.Application.Interfaces.Services;

namespace SistemaEventos.Server.Services;

public class AccountUrlBuilder : IAccountUrlBuilder
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountUrlBuilder(IConfiguration configuration, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public string BuildConfirmacionCuentaUrl(string guidAcceso)
    {
        var baseUrl = GetBackendBaseUrl();
        return $"{baseUrl}/api/Usuario/ConfirmarCuenta?guidAcceso={guidAcceso}";
    }

    public string BuildRestablecerContrasenaUrl(string guidAcceso)
    {
        return $"{GetFrontendBaseUrl()}/password?guidAcceso={guidAcceso}";
    }

    private string GetFrontendBaseUrl()
    {
        return _environment.IsDevelopment()
            ? _configuration["Frontend_URLs:Desarrollo"] ?? string.Empty
            : _configuration["Frontend_URLs:Produccion"] ?? string.Empty;
    }

    private string GetBackendBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is not null)
        {
            var baseUri = request.GetDisplayUrl();
            var currentPath = request.Path.HasValue ? request.Path.Value : string.Empty;

            if (!string.IsNullOrEmpty(currentPath) && baseUri.EndsWith(currentPath, StringComparison.OrdinalIgnoreCase))
            {
                baseUri = baseUri[..^currentPath.Length];
            }

            if (request.QueryString.HasValue)
            {
                baseUri = baseUri.Split('?', 2)[0];
            }

            return baseUri.TrimEnd('/');
        }

        return _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("No fue posible construir la URL base del API.");
    }
}