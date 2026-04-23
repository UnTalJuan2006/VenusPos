using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.DTOs.Servicio;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicioController : ControllerBase
    {
        private readonly IServicioService _service;

        public ServicioController(IServicioService service)
        {
            _service = service;
        }

        // GET /api/Servicio
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _service.ObtenerTodosAsync());

        // GET /api/Servicio/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var servicio = await _service.ObtenerPorIdAsync(id);
            return servicio is null ? NotFound() : Ok(servicio);
        }

        // POST /api/Servicio
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Crear([FromBody] CrearServicioDTO dto)
        {
            try
            {
                var creado = await _service.CrearAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/Servicio/{id}
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarServicioDTO dto)
        {
            try
            {
                return Ok(await _service.ActualizarAsync(id, dto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE /api/Servicio/{id}
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                return await _service.EliminarAsync(id) ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}