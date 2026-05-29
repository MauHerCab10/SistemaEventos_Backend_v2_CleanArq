using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Application.UseCases.Interfaces;
using SistemaEventos.Server.Extensions;

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
    public async Task<IActionResult> ConsultarEventosDisponibles()
    {
        var idUsuario = HttpContext.GetAuthenticatedUserId();
        if (!idUsuario.HasValue)
        {
            return BadRequest(new { isSuccess = false, mensaje = "No fue posible determinar el usuario autenticado." });
        }

        var respuesta = await _eventoService.ConsultarEventosDisponibles(idUsuario.Value, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpPost("CrearEvento")]
    public async Task<IActionResult> CrearEvento([FromBody] EventoDTO evento)
    {
        var idUsuario = HttpContext.GetAuthenticatedUserId();
        if (!idUsuario.HasValue)
        {
            return BadRequest(new { isSuccess = false, mensaje = "No fue posible determinar el usuario autenticado." });
        }

        evento.IdUsuarioCreacion = idUsuario.Value;
        var respuesta = await _eventoService.CrearEvento(evento, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpPut("ModificarEvento")]
    public async Task<IActionResult> ModificarEvento([FromBody] EventoDTO evento)
    {
        var respuesta = await _eventoService.ModificarEvento(evento, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpDelete("EliminarEvento")]
    public async Task<IActionResult> EliminarEvento(int idEvento)
    {
        var respuesta = await _eventoService.EliminarEvento(idEvento, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpPost("InscripcionAEvento")]
    public async Task<IActionResult> InscripcionAEvento(int idEvento)
    {
        var idUsuario = HttpContext.GetAuthenticatedUserId();
        if (!idUsuario.HasValue)
        {
            return BadRequest(new { isSuccess = false, mensaje = "No fue posible determinar el usuario autenticado." });
        }

        var respuesta = await _eventoService.InscripcionAEvento(idEvento, idUsuario.Value, HttpContext.RequestAborted);
        return Ok(respuesta);
    }

    [HttpPost("DimisionDeEvento")]
    public async Task<IActionResult> DimisionDeEvento(int idEvento)
    {
        var idUsuario = HttpContext.GetAuthenticatedUserId();
        if (!idUsuario.HasValue)
        {
            return BadRequest(new { isSuccess = false, mensaje = "No fue posible determinar el usuario autenticado." });
        }

        var respuesta = await _eventoService.DimisionDeEvento(idEvento, idUsuario.Value, HttpContext.RequestAborted);
        return Ok(respuesta);
    }
}