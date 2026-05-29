namespace SistemaEventos.Domain.Entities;

public class Evento
{
    public int IdEvento { get; set; }

    public string NombreEvento { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; }

    public string DireccionUbicacion { get; set; } = string.Empty;

    public int CapMaxPermitida { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public int CantidadAsistentes { get; set; }

    public int CuposDisponibles { get; set; }

    public bool EsUsuarioInscrito { get; set; }
}