using Microsoft.Data.SqlClient;
using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Domain.Entities;
using SistemaEventos.Infrastructure.Persistence;

namespace SistemaEventos.Infrastructure.Persistence.Repositories;

public class AutorizacionRepository : SqlRepositoryBase, IAutorizacionRepository
{
    public AutorizacionRepository(SqlConnectionContext connectionContext, SqlConnectionFactory connectionFactory)
        : base(connectionContext, connectionFactory)
    {
    }

    public Task<HistorialRefreshToken?> ConsultarUltimoHistorialRefreshTokensPorUsuarioAsync(
        int idUsuario,
        string? accessToken = null,
        string? refreshToken = null,
        CancellationToken cancellationToken = default)
    {
        return WithConnectionAsync(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_ConsultarUltimoHistorialRefreshTokensPorUsuario", connection, transaction);
            command.Parameters.AddWithValue("@IdUsuario", idUsuario);
            command.Parameters.AddWithValue("@AccessToken", (object?)accessToken ?? DBNull.Value);
            command.Parameters.AddWithValue("@RefreshToken", (object?)refreshToken ?? DBNull.Value);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new HistorialRefreshToken
            {
                IdHistorialToken = Convert.ToInt32(reader["IdHistorialToken"]),
                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                AccessToken = reader["AccessToken"].ToString() ?? string.Empty,
                RefreshToken = reader["RefreshToken"].ToString() ?? string.Empty,
                FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                FechaExpiracion = Convert.ToDateTime(reader["FechaExpiracion"]),
                EstaActivo = Convert.ToBoolean(reader["EstaActivo"])
            };
        }, cancellationToken);
    }

    public Task<bool> GuardarHistorialRefreshTokenDeUsuarioAsync(HistorialRefreshToken historialRefreshToken, CancellationToken cancellationToken = default)
    {
        return WithConnectionAsync(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_GuardarHistorialRefreshTokenDeUsuario", connection, transaction);
            command.Parameters.AddWithValue("@IdUsuario", historialRefreshToken.IdUsuario);
            command.Parameters.AddWithValue("@AccessToken", historialRefreshToken.AccessToken);
            command.Parameters.AddWithValue("@RefreshToken", historialRefreshToken.RefreshToken);
            command.Parameters.AddWithValue("@FechaCreacion", historialRefreshToken.FechaCreacion);
            command.Parameters.AddWithValue("@FechaExpiracion", historialRefreshToken.FechaExpiracion);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }

    public Task<bool> ActualizarHistorialRefreshTokenDeUsuarioAsync(int idHistorialToken, string accessToken, CancellationToken cancellationToken = default)
    {
        return WithConnectionAsync(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_ActualizarHistorialRefreshTokenDeUsuario", connection, transaction);
            command.Parameters.AddWithValue("@IdHistorialToken", idHistorialToken);
            command.Parameters.AddWithValue("@AccessToken", accessToken);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }

    public Task<bool> EliminarHistorialRefreshTokensPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        return WithConnectionAsync(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_EliminarHistorialRefreshTokensPorUsuario", connection, transaction);
            command.Parameters.AddWithValue("@IdUsuario", idUsuario);

            await command.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }, cancellationToken);
    }
}