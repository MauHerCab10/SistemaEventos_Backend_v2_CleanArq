using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Domain.Entities;
using SistemaEventos.Infrastructure.Persistence;

namespace SistemaEventos.Infrastructure.Persistence.Repositories;

public class PlantillaCorreoRepository : SqlRepositoryBase, IPlantillaCorreoRepository
{
    public PlantillaCorreoRepository(SqlConnectionContext connectionContext, SqlConnectionFactory connectionFactory)
        : base(connectionContext, connectionFactory)
    {
    }

    public Task<List<PlantillaCorreo>> ObtenerPlantillasCorreoAsync(CancellationToken cancellationToken = default)
    {
        return WithConnectionAsync(async (connection, transaction) =>
        {
            var correoPlantillas = new List<PlantillaCorreo>();

            await using var command = CreateStoredProcedureCommand("sp_ObtenerPlantillasCorreo", connection, transaction);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                correoPlantillas.Add(new PlantillaCorreo
                {
                    Nombre = reader["NombrePlantillaCorreo"].ToString() ?? string.Empty,
                    Asunto = reader["AsuntoPlantillaCorreo"].ToString() ?? string.Empty,
                    Cuerpo = reader["CuerpoPlantillaCorreo"].ToString() ?? string.Empty
                });
            }

            return correoPlantillas;
        }, cancellationToken);
    }
}