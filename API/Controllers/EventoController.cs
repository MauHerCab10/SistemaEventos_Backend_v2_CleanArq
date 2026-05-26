using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Application.UseCases.Interfaces;

namespace SistemaEventos.Server.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class EventoController : ControllerBase
{
    private readonly IEventoService _eventoService;

    public EventoController(IEventoService eventoService)
    {
        _eventoService = eventoService;
    }

    [HttpGet("ConsultarEventosDisponibles")]
    public async Task<IActionResult> ConsultarEventosDisponibles(string idUsuario)
    {
        var respuesta = await _eventoService.ConsultarEventosDisponiblesAsync(idUsuario, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpPost("CrearEvento")]
    public async Task<IActionResult> CrearEvento([FromBody] EventoDto evento)
    {
        var respuesta = await _eventoService.CrearEventoAsync(evento, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpPut("ModificarEvento")]
    public async Task<IActionResult> ModificarEvento([FromBody] EventoDto evento)
    {
        var respuesta = await _eventoService.ModificarEventoAsync(evento, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpDelete("EliminarEvento")]
    public async Task<IActionResult> EliminarEvento(int idEvento)
    {
        var respuesta = await _eventoService.EliminarEventoAsync(idEvento, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpPost("InscripcionAEvento")]
    public async Task<IActionResult> InscripcionAEvento(int idEvento, int idUsuario)
    {
        var respuesta = await _eventoService.InscripcionAEventoAsync(idEvento, idUsuario, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpPost("DimisionDeEvento")]
    public async Task<IActionResult> DimisionDeEvento(int idEvento, int idUsuario)
    {
        var respuesta = await _eventoService.DimisionDeEventoAsync(idEvento, idUsuario, HttpContext.RequestAborted);
        return Ok(respuesta);
    }
}