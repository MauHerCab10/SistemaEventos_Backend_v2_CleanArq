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

    public JwtTokenService(IOptions<JwtSettingsOptions> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerarAccessToken(int idUsuario)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var userClaims = new ClaimsIdentity();
        userClaims.AddClaim(new Claim("IdUsuario", idUsuario.ToString()));
        userClaims.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        var credencialesToken = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = userClaims,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessToken_ExpirationTime),
            SigningCredentials = credencialesToken
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerarRefreshToken()
    {
        var byteArray = new byte[64];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(byteArray);

        var base64Token = Convert.ToBase64String(byteArray);
        var extraData = Guid.NewGuid().ToString("N") + DateTime.Now.Ticks;

        using var sha = SHA512.Create();
        var combined = Encoding.UTF8.GetBytes(base64Token + extraData);
        var hash = sha.ComputeHash(combined);
        return Convert.ToBase64String(hash);
    }

    public bool ValidarToken(string accessToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(accessToken, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public int? ObtenerIdUsuario(string accessToken)
    {
        var token = ReadToken(accessToken);
        var rawValue = token?.Claims.FirstOrDefault(claim => claim.Type == "IdUsuario")?.Value;

        return int.TryParse(rawValue, out var idUsuario)
            ? idUsuario
            : null;
    }

    public DateTime? ObtenerFechaExpiracionAccessToken(string accessToken)
    {
        var token = ReadToken(accessToken);
        return token?.ValidTo.AddHours(_jwtSettings.CantidadHorasRestarZonaHoraria);
    }

    public DateTime ObtenerFechaExpiracionRefreshToken(DateTime fechaCreacion)
    {
        return fechaCreacion.AddMinutes(_jwtSettings.RefreshToken_ExpirationTime);
    }

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