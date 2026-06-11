namespace SistemaEventos.Server.Services;

public class CookieService : ICookieService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    //Creación de la cookie de AccessToken
    public void SetCookieAccessToken(string token)
    {
        var context = GetHttpContext();
        var cookieOptionsAT = BuildCookieOptions(_configuration.GetValue<int>("JwtSettings:AccessToken_ExpirationTime")); //AccessToken
        context.Response.Cookies.Append("cookieAccessToken", token, cookieOptionsAT);
    }

    //Creación de la cookie de RefreshToken
    public void SetCookieRefreshToken(string token)
    {
        var context = GetHttpContext();
        var cookieOptionsRT = BuildCookieOptions(_configuration.GetValue<int>("JwtSettings:RefreshToken_ExpirationTime")); //RefreshToken
        context.Response.Cookies.Append("cookieRefreshToken", token, cookieOptionsRT);
    }

    //Eliminación de las cookies en el navegador del usuario cuando la respuesta llegue al frontend
    public void EliminarCookiesDelUsuario()
    {
        var context = GetHttpContext();

        context.Response.Cookies.Delete("cookieAccessToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        });

        context.Response.Cookies.Delete("cookieRefreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        });
    }


    #region Métodos PRIVADOS
    //Método auxiliar para obtener el HttpContext actual, lanzando una excepción si no está disponible
    private HttpContext GetHttpContext()
    {
        return _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No hay HttpContext disponible.");
    }

    //Método para configurar las opciones de la cookie
    private static CookieOptions BuildCookieOptions(int expirationMinutes)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes),
            Path = "/"
        };
    }
    #endregion

}