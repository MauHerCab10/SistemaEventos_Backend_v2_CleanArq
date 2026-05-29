using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Infrastructure.Configuration;

namespace SistemaEventos.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettingsOptions _jwtSettings;

    //El constructor recibe la configuración de JwtSettingsOptions a través de IOptions, lo que permite acceder a los valores configurados del "appsettings.json" relacionados con JWT
    public JwtTokenService(IOptions<JwtSettingsOptions> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    //Genera ÚNICAMENTE el AccesToken
    public string GenerarAccessToken(int idUsuario)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var userClaims = new ClaimsIdentity();
        userClaims.AddClaim(new Claim("IdUsuario", idUsuario.ToString()));
        userClaims.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        var credencialesToken = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256Signature
        );

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = userClaims,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessToken_ExpirationTime),
            SigningCredentials = credencialesToken
        };

        //Creación del AccessToken
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);
        string tokenCreado = tokenHandler.WriteToken(tokenConfig);

        return tokenCreado;

    }

    //Genera ÚNICAMENTE el RefreshToken
    public string GenerarRefreshToken()
    {
        var byteArray = new byte[64];

        // Generar bytes aleatorios criptográficamente seguros
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(byteArray);

        // Convertir a Base64 (para transporte seguro en JSON, HTTP, etc.)
        var base64Token = Convert.ToBase64String(byteArray);

        // Agregar entropía adicional (fecha, GUID)
        var extraData = Guid.NewGuid().ToString("N") + DateTime.Now.Ticks;

        // Crear un Hash SHA512 para reforzar la integridad del Toekn
        using (var sha = SHA512.Create())
        {
            //Creación del RefreshToken
            var combined = Encoding.UTF8.GetBytes(base64Token + extraData);
            var hash = sha.ComputeHash(combined);
            var refreshToken = Convert.ToBase64String(hash);

            return refreshToken;
        }
    }

    //Valida si el AccessToken ingresado es válido para realizar peticiones q requieran autorización
    public bool ValidarToken(string accessToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, //verifica la firma del token usando la clave secreta (SecretKey). Esto garantiza que nadie haya modificado el token
            ValidateIssuer = true, //comprueba que el token proviene del emisor correcto ("sistemaeventos-api.com")
            ValidIssuer = _jwtSettings.Issuer, //comprueba que el token proviene del emisor correcto ("sistemaeventos-api.com")
            ValidateAudience = true, //asegura que el token esté destinado a esta API ("sistemaeventos-app.com")
            ValidAudience = _jwtSettings.Audience, //valor esperado de la audiencia (JwtSettings:Audience)
            ValidateLifetime = false, //valor esperado de la audiencia (JwtSettings:Audience)
            ClockSkew = TimeSpan.Zero, //elimina la tolerancia por desfase de reloj
            NameClaimType = ClaimTypes.NameIdentifier, //indican qué claim se usará como nombre del usuario
            RoleClaimType = ClaimTypes.Role, //indican qué claim se usará como rol del usuario
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)) //la clave secreta que se usa para validar la firma del token. Si no coincide, el token es inválido
        };

        try
        {
            //El método ValidateToken verifica si el token es válido o no
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(accessToken, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    //Extrae el IdUsuario del AccessToken para identificar al usuario que realiza la petición
    public int? ObtenerIdUsuario(string accessToken)
    {
        var token = ReadToken(accessToken);
        var rawValue = token?.Claims.FirstOrDefault(claim => claim.Type == "IdUsuario")?.Value;

        return int.TryParse(rawValue, out var idUsuario)
            ? idUsuario
            : null;
    }

    // Extrae la fecha de expiración del AccessToken para validar si el token aún es válido o si se requiere un nuevo AccessToken usando el RefreshToken
    public DateTime? ObtenerFechaExpiracionAccessToken(string accessToken)
    {
        var token = ReadToken(accessToken);
        return token?.ValidTo.AddHours(_jwtSettings.CantidadHorasRestarZonaHoraria);
    }

    //Calcula la fecha de expiración del RefreshToken sumando el tiempo de expiración configurado en JwtSettings al momento de creación del RefreshToken
    public DateTime ObtenerFechaExpiracionRefreshToken(DateTime fechaCreacion)
    {
        var fechaExpiracion = fechaCreacion.AddMinutes(_jwtSettings.RefreshToken_ExpirationTime);
        return fechaExpiracion;
    }

    // Lee el AccessToken y devuelve un objeto JwtSecurityToken que contiene los claims y la información de dicho AccessToken. Si el token no es válido, devuelve null
    private JwtSecurityToken? ReadToken(string accessToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ReadJwtToken(accessToken);
        }
        catch
        {
            return null;
        }
    }
}