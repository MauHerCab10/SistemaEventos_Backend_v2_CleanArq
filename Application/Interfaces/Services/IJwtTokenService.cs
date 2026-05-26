namespace SistemaEventos.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerarAccessToken(int idUsuario);

    string GenerarRefreshToken();

    bool ValidarToken(string accessToken);

    int? ObtenerIdUsuario(string accessToken);

    DateTime? ObtenerFechaExpiracionAccessToken(string accessToken);

    DateTime ObtenerFechaExpiracionRefreshToken(DateTime fechaCreacion);
}