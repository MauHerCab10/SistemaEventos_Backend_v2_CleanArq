using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Domain.Entities;
using SistemaEventos.Domain.Enums;

namespace SistemaEventos.Infrastructure.Services;

public class PlantillaCorreoProvider : IPlantillaCorreoProvider
{
    private readonly string _cacheKey;
    private readonly IMemoryCache _memoryCache;
    private readonly IPlantillaCorreoRepository _plantillaCorreoRepository;

    public PlantillaCorreoProvider(
        IConfiguration configuration,
        IMemoryCache memoryCache,
        IPlantillaCorreoRepository plantillaCorreoRepository)
    {
        _cacheKey = configuration["Plantillas_Correos_Cache_Key"] ?? "PlantillasCorreo";
        _memoryCache = memoryCache;
        _plantillaCorreoRepository = plantillaCorreoRepository;
    }

    public async Task<PlantillaCorreo?> ObtenerPorTipoAsync(PlantillasCorreoEnum tipoPlantilla, CancellationToken cancellationToken = default)
    {
        var plantillas = await ObtenerPlantillasAsync(cancellationToken);
        return plantillas.FirstOrDefault(plantilla => plantilla.Nombre == tipoPlantilla.ToString());
    }

    private async Task<List<PlantillaCorreo>> ObtenerPlantillasAsync(CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(_cacheKey, out List<PlantillaCorreo>? plantillas) && plantillas is not null)
        {
            return plantillas;
        }

        plantillas = await _plantillaCorreoRepository.ObtenerPlantillasCorreoAsync(cancellationToken);
        _memoryCache.Set(
            _cacheKey,
            plantillas,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                Priority = CacheItemPriority.High
            });

        return plantillas;
    }
}