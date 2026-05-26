using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SistemaEventos.Infrastructure.Persistence;

public class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ConexionSQL")
            ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'ConexionSQL'.");
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}