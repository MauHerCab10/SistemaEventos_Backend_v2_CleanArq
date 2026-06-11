using Microsoft.AspNetCore.Http.Extensions;
using SistemaEventos.Application.Interfaces.Services;

namespace SistemaEventos.Server.Services;

public class AccountUrlBuilder : IAccountUrlBuilder, IFrontendUrlBuilder
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

    //Construye la URL completa para la confirmación de cuenta
    public string ArmarUrlConfirmacionCuenta(string guidAcceso)
    {
        var baseUrl = GetBackendBaseUrl();
        var completeUrl = $"{baseUrl}/api/Usuario/ConfirmarCuenta?guidAcceso={guidAcceso}";
        return completeUrl;
    }

    //Construye la URL completa para el restablecimiento de contraseña
    public string ArmarUrlRestablecerContrasena(string guidAcceso)
    {
        var baseUrl = GetFrontendBaseUrl();
        var completeUrl = $"{baseUrl}/password?guidAcceso={guidAcceso}";
        return completeUrl;
    }

    //Construye la URL completa para redirigir al usuario al login después de confirmar su cuenta
    public string ArmarUrlLoginConfirmacion(bool confirmacionExitosa)
    {
        var baseUrl = GetFrontendBaseUrl();
        var estadoConfirmacion = confirmacionExitosa ? "ok" : "error";
        var completeUrl = $"{baseUrl}/login?confirmacion={estadoConfirmacion}";
        return completeUrl;
    }

    #region Métodos PRIVADOS
    //Obtiene la URL base del Frontend dependiendo del entorno de ejecución (Desarrollo o Producción), utilizando la configuración de la aplicación para obtener la URL correspondiente
    private string GetFrontendBaseUrl()
    {
        return _environment.IsDevelopment()
            ? _configuration["Frontend_URLs:Desarrollo"] ?? string.Empty
            : _configuration["Frontend_URLs:Produccion"] ?? string.Empty;
    }

    //Obtiene la URL base del Backend a partir de la URL de la petición actual, para obtener el dominio y el puerto
    //Si no se puede obtener a partir de la petición, utiliza la configuración de la aplicación para obtener la URL base del API
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
    #endregion

}