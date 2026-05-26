namespace SistemaEventos.Application.Common.Models;

public class Respuesta<T>
{
    public bool IsSuccess { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public T? Valor { get; set; }

    public static Respuesta<T> Ok(T? valor, string mensaje = "")
    {
        return new Respuesta<T>
        {
            IsSuccess = true,
            Mensaje = mensaje,
            Valor = valor
        };
    }

    public static Respuesta<T> Fail(string mensaje)
    {
        return new Respuesta<T>
        {
            IsSuccess = false,
            Mensaje = mensaje,
            Valor = default
        };
    }
}