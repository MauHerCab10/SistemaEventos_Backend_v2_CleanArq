using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaEventos.Infrastructure.Persistence;

public abstract class SqlRepositoryBase
{
    private readonly SqlConnectionContext _connectionContext;
    private readonly SqlConnectionFactory _connectionFactory;

    protected SqlRepositoryBase(SqlConnectionContext connectionContext, SqlConnectionFactory connectionFactory)
    {
        _connectionContext = connectionContext;
        _connectionFactory = connectionFactory;
    }

    //Si ya existe una conexión activa, se reutiliza; de lo contrario, se crea una nueva conexión, para evitar repetir en cada método de repositorio el código de crear y abrir la conexión
    protected async Task<T> ManageConnection<T>(Func<SqlConnection, SqlTransaction?, Task<T>> action, CancellationToken cancellationToken)
    {
        if (_connectionContext.Connection is not null)
        {
            return await action(_connectionContext.Connection, _connectionContext.Transaction);
        }

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return await action(connection, null);
    }

    //Crea un SqlCommand configurado para ejecutar un SP, utilizando la conexión y transacción (si existe) proporcionadas, y establece el tipo de comando como StoredProcedure
    protected SqlCommand CreateStoredProcedureCommand(string storedProcedure, SqlConnection connection, SqlTransaction? transaction)
    {
        var command = transaction is null
            ? new SqlCommand(storedProcedure, connection)
            : new SqlCommand(storedProcedure, connection, transaction);

        command.CommandType = CommandType.StoredProcedure;
        return command;
    }
}