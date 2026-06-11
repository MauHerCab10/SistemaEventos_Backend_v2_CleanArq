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

    // El constructor recibe la configuración de la aplicación para obtener la clave de caché, así como las dependencias necesarias para acceder a la caché y al repositorio de plantillas de correo
    public PlantillaCorreoProvider(
        IConfiguration configuration,
        IMemoryCache memoryCache,
        IPlantillaCorreoRepository plantillaCorreoRepository)
    {
        _cacheKey = configuration["Plantillas_Correos_Cache_Key"]!;
        _memoryCache = memoryCache;
        _plantillaCorreoRepository = plantillaCorreoRepository;
    }

    //Obtiene la plantilla de correo correspondiente al tipo de plantilla solicitado, utilizando caché para optimizar el rendimiento y reducir las consultas a la BD
    public async Task<PlantillaCorreo?> ObtenerPlantillaPorTipo(PlantillasCorreoEnum tipoPlantilla, CancellationToken cancellationToken = default)
    {
        var plantillas = await ObtenerPlantillas(cancellationToken);
        return plantillas.FirstOrDefault(plantilla => plantilla.Nombre == tipoPlantilla.ToString());
    }

    #region Métodos PRIVADOS
    //Obtiene de BD las plantillas de los correos a enviar ('ConfirmarCorreo' y 'RestablecerContrasena') y las deja cargadas en caché para posteriores usos
    private async Task<List<PlantillaCorreo>> ObtenerPlantillas(CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(_cacheKey, out List<PlantillaCorreo>? plantillas) && plantillas is not null)
        {
            return plantillas;
        }

        plantillas = await _plantillaCorreoRepository.ObtenerPlantillasCorreo(cancellationToken);
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
    #endregion

}