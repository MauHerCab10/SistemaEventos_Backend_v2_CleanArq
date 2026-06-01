using Microsoft.Data.SqlClient;

namespace SistemaEventos.Infrastructure.Persistence;

//Atributos en los q se guardan la Conexión y Transacción activas del flujo actual para que los diferentes repositorios las reutilicen
public class SqlConnectionContext
{
    public SqlConnection? Connection { get; set; }

    public SqlTransaction? Transaction { get; set; }
}