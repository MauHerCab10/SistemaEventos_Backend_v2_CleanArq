using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Domain.Entities;
using SistemaEventos.Infrastructure.Persistence;

namespace SistemaEventos.Infrastructure.Persistence.Repositories;

public class EventoRepository : SqlRepositoryBase, IEventoRepository
{
    public EventoRepository(SqlConnectionContext connectionContext, SqlConnectionFactory connectionFactory)
        : base(connectionContext, connectionFactory)
    {
    }


    public Task<List<Evento>> ConsultarEventosDisponibles(int idUsuario, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            var listaEventosDisponibles = new List<Evento>();

            await using var command = CreateStoredProcedureCommand("sp_ConsultarEventosDisponibles", connection, transaction);
            command.Parameters.AddWithValue("@IdUsuarioSolicitante", idUsuario);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var fechaHora = Convert.ToDateTime(reader["FechaHora"]);
                listaEventosDisponibles.Add(new Evento
                {
                    IdEvento = Convert.ToInt32(reader["IdEvento"]),
                    NombreEvento = reader["NombreEvento"].ToString() ?? string.Empty,
                    Descripcion = reader["Descripcion"].ToString() ?? string.Empty,
                    FechaHora = fechaHora,
                    DireccionUbicacion = reader["Direccion_Ubicacion"].ToString() ?? string.Empty,
                    CapMaxPermitida = Convert.ToInt32(reader["CapMaxPermitida"]),
                    IdUsuarioCreacion = Convert.ToInt32(reader["IdUsuarioCreacion"]),
                    CantidadAsistentes = Convert.ToInt32(reader["CantidadAsistentes"]),
                    CuposDisponibles = Convert.ToInt32(reader["CuposDisponibles"]),
                    EsUsuarioInscrito = reader["UsuarioInscrito"] != DBNull.Value
                });
            }

            return listaEventosDisponibles;
        }, cancellationToken);
    }


    public Task<bool> CrearEvento(Evento evento, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_CrearEvento", connection, transaction);
            command.Parameters.AddWithValue("@NombreEvento", evento.NombreEvento);
            command.Parameters.AddWithValue("@Descripcion", evento.Descripcion);
            command.Parameters.AddWithValue("@FechaHora", evento.FechaHora);
            command.Parameters.AddWithValue("@Direccion_Ubicacion", evento.DireccionUbicacion);
            command.Parameters.AddWithValue("@CapMaxPermitida", evento.CapMaxPermitida);
            command.Parameters.AddWithValue("@IdUsuarioCreacion", evento.IdUsuarioCreacion);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }


    public Task<bool> ModificarEvento(Evento evento, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_ModificarEvento", connection, transaction);
            command.Parameters.AddWithValue("@IdEvento", evento.IdEvento);
            command.Parameters.AddWithValue("@CapMaxPermitida", evento.CapMaxPermitida);
            command.Parameters.AddWithValue("@FechaHora", evento.FechaHora);
            command.Parameters.AddWithValue("@Direccion_Ubicacion", evento.DireccionUbicacion);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }


    public Task<bool> EliminarEvento(int idEvento, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_EliminarEvento", connection, transaction);
            command.Parameters.AddWithValue("@IdEvento", idEvento);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }


    public Task<bool> InscripcionAEvento(int idEvento, int idUsuario, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_InscripcionEvento", connection, transaction);
            command.Parameters.AddWithValue("@IdEvento", idEvento);
            command.Parameters.AddWithValue("@IdUsuario", idUsuario);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }


    public Task<bool> DimisionDeEvento(int idEvento, int idUsuario, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_DimisionDeEvento", connection, transaction);
            command.Parameters.AddWithValue("@IdEvento", idEvento);
            command.Parameters.AddWithValue("@IdUsuario", idUsuario);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }

}