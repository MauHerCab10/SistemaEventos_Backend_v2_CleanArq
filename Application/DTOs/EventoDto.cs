namespace SistemaEventos.Application.DTOs;

public class EventoDto
{
    public int IdEvento { get; set; }

    public string NombreEvento { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Fecha { get; set; } = string.Empty;

    public string Hora { get; set; } = string.Empty;

    public DateTime? FechaHora { get; set; }

    public string Direccion_Ubicacion { get; set; } = string.Empty;

    public int CapMaxPermitida { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public int CantidadAsistentes { get; set; }

    public int CuposDisponibles { get; set; }

    public bool EsUsuarioInscrito { get; set; }
}