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

    public void SetCookieAccessToken(string token)
    {
        var context = GetHttpContext();
        context.Response.Cookies.Append("cookieAccessToken", token, BuildCookieOptions(_configuration.GetValue<int>("JwtSettings:AccessToken_ExpirationTime")));
    }

    public void SetCookieRefreshToken(string token)
    {
        var context = GetHttpContext();
        context.Response.Cookies.Append("cookieRefreshToken", token, BuildCookieOptions(_configuration.GetValue<int>("JwtSettings:RefreshToken_ExpirationTime")));
    }

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

    private HttpContext GetHttpContext()
    {
        return _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HttpContext available.");
    }

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
}