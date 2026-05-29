using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using SistemaEventos.Application.Interfaces.Services;

namespace SistemaEventos.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasher
{
    //Encripta la Contraseña generando un Hash seguro[Hashing con salt + algoritmo lento(PBKDF2, bcrypt o Argon2)] (Argon2 es recomendado por OWASP y NIST)
    public string EncriptarContraseña(string value)
    {
        //Salt aleatorio 16 bytes
        var salt = RandomNumberGenerator.GetBytes(16);

        var config = new Argon2Config
        {
            Type = Argon2Type.HybridAddressing, //Argon2 → más seguro
            Version = Argon2Version.Nineteen,
            TimeCost = 4, //Cantidad de iteraciones
            MemoryCost = 1024 * 64, //64 MB de RAM (recomendado)
            Lanes = 4, //Número de hilos paralelos permitidos (recomendado)
            Threads = Environment.ProcessorCount, //Número de hilos a utilizar
            Salt = salt,
            Password = Encoding.UTF8.GetBytes(value), //Contraseña a encriptar
            HashLength = 32 //256 bits de salida
        };

        //Contraseña hasheada generada
        string encodedHash = Argon2.Hash(config);
        return encodedHash;
    }

    //Verifica la contraseña con Hash suministrada
    public bool VerificarContrasena(string contrasenaPlana, string contrasenaHashAlmacenada)
    {
        return Argon2.Verify(contrasenaHashAlmacenada, contrasenaPlana);
    }

    //Genera el GUID q va estar incorporado en la URL para 'ConfirmarCuenta' y 'RestablecerContrasena'
    public string GenerarGuid()
    {
        //Generar un GUID único
        var guid = Guid.NewGuid().ToString("N"); //N = texto sin guiones

        // Agregar un componente variable (marca de tiempo)
        var data = guid + DateTime.Now.Ticks;

        // Crear un hash SHA256
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = sha.ComputeHash(bytes);

            // Convertir el hash a una cadena hexadecimal
            StringBuilder tokenBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                tokenBuilder.Append(b.ToString("x2"));
            }

            //Token final generado
            string token = tokenBuilder.ToString();
            return token;
        }
    }

}