namespace SistemaEventos.Server.Services;

public interface ICookieService
{
    void SetCookieAccessToken(string token);

    void SetCookieRefreshToken(string token);

    void EliminarCookiesDelUsuario();
}