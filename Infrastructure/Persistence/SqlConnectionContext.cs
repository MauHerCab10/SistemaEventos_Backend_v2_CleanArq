using Microsoft.Data.SqlClient;

namespace SistemaEventos.Infrastructure.Persistence;

public class SqlConnectionContext
{
    public SqlConnection? Connection { get; set; }

    public SqlTransaction? Transaction { get; set; }
}