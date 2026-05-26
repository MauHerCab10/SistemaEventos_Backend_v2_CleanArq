using SistemaEventos.Domain.Entities;
using SistemaEventos.Domain.Enums;

namespace SistemaEventos.Application.Interfaces.Services;

public interface IPlantillaCorreoProvider
{
    Task<PlantillaCorreo?> ObtenerPorTipoAsync(PlantillasCorreoEnum tipoPlantilla, CancellationToken cancellationToken = default);
}