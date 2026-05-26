using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.Interfaces.Persistence;

public interface IPlantillaCorreoRepository
{
    Task<List<PlantillaCorreo>> ObtenerPlantillasCorreoAsync(CancellationToken cancellationToken = default);
}