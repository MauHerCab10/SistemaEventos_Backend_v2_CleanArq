using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using SistemaEventos.Application.Interfaces.Services;

namespace SistemaEventos.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasher
{
    public string Hash(string value)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        var config = new Argon2Config
        {
            Type = Argon2Type.HybridAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 4,
            MemoryCost = 1024 * 64,
            Lanes = 4,
            Threads = Environment.ProcessorCount,
            Salt = salt,
            Password = Encoding.UTF8.GetBytes(value),
            HashLength = 32
        };

        return Argon2.Hash(config);
    }

    public bool Verify(string plainText, string hash)
    {
        return Argon2.Verify(hash, plainText);
    }

    public string GenerarGuid()
    {
        var guid = Guid.NewGuid().ToString("N");
        var data = guid + DateTime.Now.Ticks;

        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = sha.ComputeHash(bytes);

        var tokenBuilder = new StringBuilder();
        foreach (var value in hash)
        {
            tokenBuilder.Append(value.ToString("x2"));
        }

        return tokenBuilder.ToString();
    }
}