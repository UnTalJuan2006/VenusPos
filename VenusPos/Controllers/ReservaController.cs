using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.DTOs.Reserva;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservaController : ControllerBase
    {
        private readonly IReservaService _service;

        public ReservaController(IReservaService service)
        {
            _service = service;
        }

        // GET /api/Reserva
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _service.ObtenerTodosAsync());

        // GET /api/Reserva/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var reserva = await _service.ObtenerPorIdAsync(id);
            return reserva is null ? NotFound() : Ok(reserva);
        }

        // GET /api/Reserva/codigo/{codigo}
        [HttpGet("codigo/{codigo}")]
        public async Task<IActionResult> ObtenerPorCodigo(string codigo)
        {
            var reserva = await _service.ObtenerPorCodigoAsync(codigo);
            return reserva is null ? NotFound() : Ok(reserva);
        }

        // GET /api/Reserva/cliente/{idCliente}
        [HttpGet("cliente/{idCliente:int}")]
        public async Task<IActionResult> ObtenerPorCliente(int idCliente)
            => Ok(await _service.ObtenerPorClienteAsync(idCliente));

        // POST /api/Reserva
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Crear([FromBody] CrearReservaDTO dto)
        {
            try
            {
                var creada = await _service.CrearReservaAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
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

        // POST /api/Reserva/{id}/confirmar
        [HttpPost("{id:int}/confirmar")]
        [AllowAnonymous]
        public async Task<IActionResult> Confirmar(int id)
        {
            try
            {
                var confirmada = await _service.ConfirmarReservaAsync(id);
                return Ok(confirmada);
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

        // PUT /api/Reserva/{id}/estado
        [HttpPut("{id:int}/estado")]
        [Authorize]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoReservaDTO dto)
        {
            try
            {
                var actualizada = await _service.ActualizarEstadoAsync(id, dto);
                return Ok(actualizada);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/Reserva/{id}
        [HttpPut("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> ActualizarReserva(int id, [FromBody] ActualizarReservaDTO dto)
        {
            try
            {
                var actualizada = await _service.ActualizarReservaAsync(id, dto);
                return Ok(actualizada);
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

        // DELETE /api/Reserva/{id}
        [HttpDelete("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                return await _service.CancelarReservaAsync(id) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/Reserva/disponibilidad?fecha=2025-03-24&duracionMinutos=120&idEmpleado=1&tamañoMascota=Grande
        [HttpGet("disponibilidad")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerDisponibilidad(
            [FromQuery] DateTime fecha,
            [FromQuery] int duracionMinutos,
            [FromQuery] int? idEmpleado = null,
            [FromQuery] string? tamañoMascota = null)
        {
            VenusPos.Domain.Enums.Mascota.EnumTamaño? tamaño = null;
            if (!string.IsNullOrEmpty(tamañoMascota) &&
                Enum.TryParse<VenusPos.Domain.Enums.Mascota.EnumTamaño>(tamañoMascota, out var tamañoParsed))
            {
                tamaño = tamañoParsed;
            }

            var horarios = await _service.ObtenerHorariosDisponiblesAsync(fecha, duracionMinutos, idEmpleado, tamaño);
            return Ok(horarios);
        }

        // POST /api/Reserva/calcular-precio
        [HttpPost("calcular-precio")]
        [AllowAnonymous]
        public async Task<IActionResult> CalcularPrecio([FromBody] CalcularPrecioRequestDTO dto)
        {
            try
            {
                var calculo = await _service.CalcularPrecioReservaAsync(dto.IdMascota, dto.IdsServicios);
                return Ok(calculo);
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
        //GET /api/Reserva/mascota/{idMascota}

        [HttpGet("mascota/{idMascota:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerPorMascota(int idMascota)
            => Ok(await _service.ObtenerPorMascotaAsync(idMascota));

        // GET /api/Reserva/sin-venta
        [HttpGet("sin-venta")]
        [Authorize]
        public async Task<IActionResult> ObtenerSinVenta()
            => Ok(await _service.ObtenerReservasSinVentaAsync());
    }
}
