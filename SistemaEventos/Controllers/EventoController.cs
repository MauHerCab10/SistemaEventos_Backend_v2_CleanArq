using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Negocio.Interfaz;
using Transversal.Models;

namespace SistemaEventos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EventoController : ControllerBase
    {
        private readonly IEventoBLL _eventoBLL;

        public EventoController(IEventoBLL eventoBLL)
        {
            _eventoBLL = eventoBLL;
        }

        [HttpGet("ConsultarEventosDisponibles")]
        public async Task<IActionResult> ConsultarEventosDisponibles(string idUsuario)
        {
            var respuesta = await _eventoBLL.ConsultarEventosDisponibles(idUsuario);
            return Ok(respuesta);
        }

        [HttpPost("CrearEvento")]
        public async Task<IActionResult> CrearEvento(Evento evento)
        {
            var respuesta = await _eventoBLL.CrearEvento(evento);
            return Ok(respuesta);
        }

        [HttpPut("ModificarEvento")]
        public async Task<IActionResult> ModificarEvento(Evento evento)
        {
            var respuesta = await _eventoBLL.ModificarEvento(evento);
            return Ok(respuesta);
        }

        [HttpDelete("EliminarEvento")]
        public async Task<IActionResult> EliminarEvento(int idEvento)
        {
            var respuesta = await _eventoBLL.EliminarEvento(idEvento);
            return Ok(respuesta);
        }

        [HttpPost("InscripcionAEvento")]
        public async Task<IActionResult> InscripcionAEvento(int idEvento, int idUsuario)
        {
            var respuesta = await _eventoBLL.InscripcionAEvento(idEvento, idUsuario);
            return Ok(respuesta);
        }

        [HttpPost("DimisionDeEvento")]
        public async Task<IActionResult> DimisionDeEvento(int idEvento, int idUsuario)
        {
            var respuesta = await _eventoBLL.DimisionDeEvento(idEvento, idUsuario);
            return Ok(respuesta);
        }

    }
}