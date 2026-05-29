namespace SistemaEventos.Server.Extensions;

public static class HttpContextExtensions
{
    // Obtiene el ID del usuario autenticado desde el contexto HTTP
    public static int? GetAuthenticatedUserId(this HttpContext context)
    {
        var idUsuario = context.Items["IdUsuario"]?.ToString();
        return int.TryParse(idUsuario, out var userId)
            ? userId
            : null;
    }
}