using Datos.Interfaz;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Servicio.Interfaz;
using System;
using System.Collections.Generic;
using System.Text;
using Transversal.Models;

namespace Servicio.Implementacion
{
    public class PlantillasCorreoService : IPlantillasCorreoService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IPlantillaCorreoDAL _correoPlantillaDAL;

        public PlantillasCorreoService(IConfiguration configuration, IMemoryCache cache, IPlantillaCorreoDAL correoPlantillaDAL)
        {
            _configuration = configuration;
            _cache = cache;
            _correoPlantillaDAL = correoPlantillaDAL;
        }

        //Obtiene de BD las plantillas de los correos a enviar ('ConfirmarCorreo' y 'RestablecerContrasena') y las deja cargadas en caché para posteriores usos
        public async Task<List<PlantillaCorreo>> CargarPlantillasCorreoDesdeDB()
        {
            if (!_cache.TryGetValue(_configuration["Plantillas_Correos_Cache_Key"]!, out List<PlantillaCorreo>? plantillas))
            {
                plantillas = await _correoPlantillaDAL.ObtenerPlantillasCorreo();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24), //setear este valor quemado en AppSettings
                    Priority = CacheItemPriority.High
                };

                _cache.Set(_configuration["Plantillas_Correos_Cache_Key"]!, plantillas, cacheOptions);
            }

            return plantillas ?? new List<PlantillaCorreo>();
        }

    }
}