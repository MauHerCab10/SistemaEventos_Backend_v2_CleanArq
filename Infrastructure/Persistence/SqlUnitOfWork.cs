using Microsoft.Data.SqlClient;
using SistemaEventos.Application.Interfaces.Persistence;

namespace SistemaEventos.Infrastructure.Persistence;

public class SqlUnitOfWork : IUnitOfWork
{
    private readonly SqlConnectionContext _connectionContext;
    private readonly SqlConnectionFactory _connectionFactory;

    public SqlUnitOfWork(SqlConnectionContext connectionContext, SqlConnectionFactory connectionFactory)
    {
        _connectionContext = connectionContext;
        _connectionFactory = connectionFactory;
    }

    //SE RECOMIENDA DEJARLO, ya q es una sobrecarga útil para operaciones transaccionales sin retorno
    public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync<object?>(
            async ct =>
            {
                await action(ct);
                return null;
            },
            cancellationToken);
    }

    //Sobrecarga genérica de ExecuteAsync para manejar la conexión y transaccionarla de forma eficiente y segura para los métodos de todos los repositorios
    //Si ya existe una conexión activa, se reutiliza; de lo contrario, se crea una nueva conexión y se inicia una transacción
    //El método recibe una función asíncrona q representa la acción a ejecutar dentro de la transacción
    //Al finalizar la acción, valida si toda la transacción fue exitosa o se revierte en caso de error, asegurando q todos los recursos se liberen adecuadamente
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        if (_connectionContext.Connection is not null)
        {
            return await action(cancellationToken);
        }

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction) await connection.BeginTransactionAsync(cancellationToken);

        _connectionContext.Connection = connection;
        _connectionContext.Transaction = transaction;

        try
        {
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            _connectionContext.Connection = null;
            _connectionContext.Transaction = null;
        }
    }

}