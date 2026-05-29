using System.Globalization;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Domain.Entities;

namespace SistemaEventos.Application.Mappings;

public static class EventoMappingExtensions
{
    private const string FechaFormato = "dd/MM/yyyy";
    private const string HoraFormato = "HH:mm";

    //de EventoDTO a Evento
    public static Evento ToEntity(this EventoDTO dto)
    {
        return new Evento
        {
            IdEvento = dto.IdEvento,
            NombreEvento = dto.NombreEvento,
            Descripcion = dto.Descripcion,
            FechaHora = NormalizarFechaHora(dto),
            DireccionUbicacion = dto.Direccion_Ubicacion,
            CapMaxPermitida = dto.CapMaxPermitida,
            IdUsuarioCreacion = dto.IdUsuarioCreacion,
            CantidadAsistentes = dto.CantidadAsistentes,
            CuposDisponibles = dto.CuposDisponibles,
            EsUsuarioInscrito = dto.EsUsuarioInscrito
        };
    }

    //de Evento a EventoDTO
    public static EventoDTO ToDTO(this Evento evento)
    {
        return new EventoDTO
        {
            IdEvento = evento.IdEvento,
            NombreEvento = evento.NombreEvento,
            Descripcion = evento.Descripcion,
            Fecha = evento.FechaHora.ToString(FechaFormato, CultureInfo.InvariantCulture),
            Hora = evento.FechaHora.ToString(HoraFormato, CultureInfo.InvariantCulture),
            FechaHora = evento.FechaHora,
            Direccion_Ubicacion = evento.DireccionUbicacion,
            CapMaxPermitida = evento.CapMaxPermitida,
            IdUsuarioCreacion = evento.IdUsuarioCreacion,
            CantidadAsistentes = evento.CantidadAsistentes,
            CuposDisponibles = evento.CuposDisponibles,
            EsUsuarioInscrito = evento.EsUsuarioInscrito
        };
    }

    //Normaliza la fecha y la hora del Evento y devuelve un único DateTime
    private static DateTime NormalizarFechaHora(EventoDTO dto)
    {
        if (!string.IsNullOrEmpty(dto.Fecha) && !string.IsNullOrEmpty(dto.Hora))
        {
            return DateTime.ParseExact(
                $"{dto.Fecha} {dto.Hora}",
                $"{FechaFormato} {HoraFormato}",
                CultureInfo.InvariantCulture);
        }

        if (dto.FechaHora.HasValue)
        {
            return dto.FechaHora.Value;
        }

        throw new FormatException("Los campos Fecha y Hora son obligatorios.");
    }
}