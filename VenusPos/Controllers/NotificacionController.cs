using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.DTOs.Notificacion;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionController : ControllerBase
    {
        private readonly INotificacionService _service;

        public NotificacionController(INotificacionService service)
        {
            _service = service;
        }

        // GET /api/notificacion
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _service.ObtenerTodosAsync());

        // GET /api/notificacion/no-leidas
        [HttpGet("no-leidas")]
        [Authorize]
        public async Task<IActionResult> ObtenerNoLeidas()
            => Ok(await _service.ObtenerNoLeidasAsync());

        // GET /api/notificacion/contador
        [HttpGet("contador")]
        [Authorize]
        public async Task<IActionResult> ContarNoLeidas()
            => Ok(new { count = await _service.ContarNoLeidasAsync() });

        // GET /api/notificacion/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var notificacion = await _service.ObtenerPorIdAsync(id);
            return notificacion is null ? NotFound() : Ok(notificacion);
        }

        // POST /api/notificacion
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Crear([FromBody] CrearNotificacionDTO dto)
        {
            try
            {
                var creada = await _service.CrearAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/notificacion/{id}/marcar-leida
        [HttpPut("{id:int}/marcar-leida")]
        [Authorize]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            try
            {
                var notificacion = await _service.MarcarLeidaAsync(id);
                return Ok(notificacion);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/notificacion/marcar-todas-leidas
        [HttpPut("marcar-todas-leidas")]
        [Authorize]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            await _service.MarcarTodasLeidasAsync();
            return NoContent();
        }

        // DELETE /api/notificacion/{id}
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Eliminar(int id)
        {
            return await _service.EliminarAsync(id) ? NoContent() : NotFound();
        }
    }
}
