namespace SistemaEventos.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task EjecutarAccion(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);

    Task<T> EjecutarAccion<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}