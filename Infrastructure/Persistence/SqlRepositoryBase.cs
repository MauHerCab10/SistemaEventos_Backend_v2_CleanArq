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

    protected async Task<T> WithConnectionAsync<T>(Func<SqlConnection, SqlTransaction?, Task<T>> action, CancellationToken cancellationToken)
    {
        if (_connectionContext.Connection is not null)
        {
            return await action(_connectionContext.Connection, _connectionContext.Transaction);
        }

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return await action(connection, null);
    }

    protected SqlCommand CreateStoredProcedureCommand(string storedProcedure, SqlConnection connection, SqlTransaction? transaction)
    {
        var command = transaction is null
            ? new SqlCommand(storedProcedure, connection)
            : new SqlCommand(storedProcedure, connection, transaction);

        command.CommandType = CommandType.StoredProcedure;
        return command;
    }
}