using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SistemaEventos.Infrastructure.Persistence;

//Crea una nueva conexión SQL cuando todavía no existe una
public class SqlConnectionFactory
{
    private readonly string _connectionString;

    // Constructor que recibe la configuración de la aplicación para obtener la cadena de conexión a la BD
    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ConexionSQL")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'ConexionSQL'.");
    }

    // Crea y devuelve una nueva instancia de SqlConnection utilizando la cadena de conexión proporcionada en la configuración
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}