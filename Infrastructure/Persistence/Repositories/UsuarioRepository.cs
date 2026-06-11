using System.Data.Common;
using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Domain.Entities;
using SistemaEventos.Infrastructure.Persistence;

namespace SistemaEventos.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : SqlRepositoryBase, IUsuarioRepository
{
    public UsuarioRepository(SqlConnectionContext connectionContext, SqlConnectionFactory connectionFactory)
        : base(connectionContext, connectionFactory)
    {
    }


    public Task<bool> RegistrarUsuario(Usuario usuario, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_InsertarUsuario", connection, transaction);
            command.Parameters.AddWithValue("@NombreApellido", usuario.NombreApellido);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@ContrasenaHash", usuario.ContrasenaHash);
            command.Parameters.AddWithValue("@Restablecer", usuario.Restablecer);
            command.Parameters.AddWithValue("@Confirmado", usuario.Confirmado);
            command.Parameters.AddWithValue("@GuidAcceso", usuario.GuidAcceso);
            command.Parameters.AddWithValue("@GuidValidado", usuario.GuidValidado);
            command.Parameters.AddWithValue("@FechaCreacionGuid", usuario.FechaCreacionGuid);
            command.Parameters.AddWithValue("@FechaExpiracionGuid", usuario.FechaExpiracionGuid);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }


    public Task<Usuario?> ConsultarUsuarioPorEmail(string email, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_ConsultarUsuarioPorEmail", connection, transaction);
            command.Parameters.AddWithValue("@Email", email);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken)
                ? MapUsuario(reader)
                : null;
        }, cancellationToken);
    }


    public Task<Usuario?> ConsultarUsuarioPorGuid(string guidUsuario, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_ConsultarUsuarioPorGuid", connection, transaction);
            command.Parameters.AddWithValue("@GuidUsuario", guidUsuario);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken)
                ? MapUsuario(reader)
                : null;
        }, cancellationToken);
    }


    public Task<bool> RestablecerContrasena(Usuario usuarioRestablecido, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_RestablecerContrasena", connection, transaction);
            command.Parameters.AddWithValue("@IdUsuario", usuarioRestablecido.IdUsuario);
            command.Parameters.AddWithValue("@GuidAcceso", usuarioRestablecido.GuidAcceso);
            command.Parameters.AddWithValue("@FechaCreacionGuid", usuarioRestablecido.FechaCreacionGuid);
            command.Parameters.AddWithValue("@FechaExpiracionGuid", usuarioRestablecido.FechaExpiracionGuid);
            command.Parameters.AddWithValue("@ContrasenaHash", usuarioRestablecido.ContrasenaHash);
            command.Parameters.AddWithValue("@Restablecer", usuarioRestablecido.Restablecer);
            command.Parameters.AddWithValue("@Confirmado", usuarioRestablecido.Confirmado);
            command.Parameters.AddWithValue("@GuidValidado", usuarioRestablecido.GuidValidado);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }


    public Task<bool> ActualizarContrasenaAntigua(string guidAcceso, string contrasenaHash, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_ActualizarContrasenaAntigua", connection, transaction);
            command.Parameters.AddWithValue("@GuidAcceso", guidAcceso);
            command.Parameters.AddWithValue("@ContrasenaHash", contrasenaHash);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }


    public Task<bool> ConfirmarCuenta(string guidAcceso, CancellationToken cancellationToken = default)
    {
        return ManageConnection(async (connection, transaction) =>
        {
            await using var command = CreateStoredProcedureCommand("sp_ConfirmarCuenta", connection, transaction);
            command.Parameters.AddWithValue("@GuidAcceso", guidAcceso);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }, cancellationToken);
    }

    #region Métodos PRIVADOS
    private static Usuario MapUsuario(DbDataReader reader)
    {
        return new Usuario
        {
            IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
            NombreApellido = reader["NombreApellido"].ToString() ?? string.Empty,
            Email = reader["Email"].ToString() ?? string.Empty,
            ContrasenaHash = reader["ContrasenaHash"].ToString() ?? string.Empty,
            Restablecer = Convert.ToBoolean(reader["Restablecer"]),
            Confirmado = Convert.ToBoolean(reader["Confirmado"]),
            GuidAcceso = reader["GuidAcceso"].ToString() ?? string.Empty,
            GuidValidado = Convert.ToBoolean(reader["Validado"]),
            GuidActivo = Convert.ToBoolean(reader["EstaActivo"])
        };
    }
    #endregion

}