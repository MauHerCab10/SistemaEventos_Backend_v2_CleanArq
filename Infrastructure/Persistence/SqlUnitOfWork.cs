using Microsoft.Data.SqlClient;
using SistemaEventos.Application.Interfaces.Persistence;

namespace SistemaEventos.Infrastructure.Persistence;

//Patrón de diseño "Unit of Work". Su objetivo es ejecutar una o varias operaciones de repositorio dentro de una misma conexión SQL y una misma transacción, para q se confirme si todo transcurre bien o se revierta todo si algo falla
//Si falla algo, se revierte todo. O se guarda todo o no se guarda nada. Además, al compartir la misma conexión y transacción, se mejora el rendimiento al reducir la sobrecarga de estar abriendo y cerrando múltiples conexiones por cada operación
//Permite q la transacción sea ACID (Atomicidad, Consistencia, Aislamiento y Durabilidad) en las operaciones de la BD, lo q garantiza q los datos se mantengan íntegros y confiables incluso en situaciones de error o concurrencia
public class SqlUnitOfWork : IUnitOfWork
{
    private readonly SqlConnectionContext _connectionContext;
    private readonly SqlConnectionFactory _connectionFactory;

    public SqlUnitOfWork(SqlConnectionContext connectionContext, SqlConnectionFactory connectionFactory)
    {
        _connectionContext = connectionContext;
        _connectionFactory = connectionFactory;
    }

    //Sobrecarga útil para operaciones transaccionales sin retorno. Internamente llama al método genérico 'EjecutarAccion' y devuelve null. SE RECOMIENDA DEJARLO
    public Task EjecutarAccion(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        return EjecutarAccion<object?>(
            async ct =>
            {
                await action(ct);
                return null;
            },
            cancellationToken);
    }

    //Sobrecarga genérica de EjecutarAccion para manejar la conexión y transaccionarla de forma eficiente y segura para los métodos de todos los repositorios
    //Si ya existe una conexión activa, se reutiliza; de lo contrario, se crea una nueva conexión y se inicia una transacción
    //El método recibe una función asíncrona q representa la acción a ejecutar dentro de la transacción
    //Al finalizar la acción, valida si toda la transacción fue exitosa o se revierte en caso de error, asegurando q todos los recursos se liberen adecuadamente
    public async Task<T> EjecutarAccion<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
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