using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.DTOs.Historial;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistorialController : ControllerBase
    {
        private readonly IHistorialService _service;

        public HistorialController(IHistorialService service)
        {
            _service = service;
        }

        // GET: api/historial
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _service.ObtenerTodosAsync());

        // GET: api/historial/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var historial = await _service.ObtenerPorIdAsync(id);
            return historial is null ? NotFound() : Ok(historial);
        }

        // GET: api/historial/mascota/{idMascota}
        [HttpGet("mascota/{idMascota:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorMascota(int idMascota)
            => Ok(await _service.ObtenerPorMascotaAsync(idMascota));

        // GET: api/historial/empleado/{idEmpleado}
        [HttpGet("empleado/{idEmpleado:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorEmpleado(int idEmpleado)
            => Ok(await _service.ObtenerPorEmpleadoAsync(idEmpleado));

        // GET: api/historial/reserva/{idReserva}
        [HttpGet("reserva/{idReserva:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorReserva(int idReserva)
            => Ok(await _service.ObtenerPorReservaAsync(idReserva));

        // POST: api/historial
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Crear([FromBody] CrearHistorialDTO dto)
        {
            try
            {
                var creado = await _service.CrearAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/historial/{id}
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarHistorialDTO dto)
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
    }
}
