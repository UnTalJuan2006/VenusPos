using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.DTOs.Ventas;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentaController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentaController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        // GET /api/venta
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerTodas()
            => Ok(await _ventaService.ObtenerTodasAsync());

        // GET /api/venta/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var venta = await _ventaService.ObtenerPorIdAsync(id);
            return venta is null ? NotFound() : Ok(venta);
        }

        // GET /api/venta/caja/{idCaja}
        [HttpGet("caja/{idCaja:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorCaja(int idCaja)
            => Ok(await _ventaService.ObtenerPorCajaAsync(idCaja));

        // GET /api/venta/pendientes
        [HttpGet("pendientes")]
        [Authorize]
        public async Task<IActionResult> ObtenerPendientes()
            => Ok(await _ventaService.ObtenerPendientesAsync());

        // GET /api/venta/reserva/{idReserva}
        [HttpGet("reserva/{idReserva:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorReserva(int idReserva)
        {
            var venta = await _ventaService.ObtenerPorReservaAsync(idReserva);
            return venta is null ? NotFound() : Ok(venta);
        }

        // POST /api/venta
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RegistrarVenta([FromBody] RegistrarVentaDTO dto)
        {
            try
            {
                var venta = await _ventaService.RegistrarVentaAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = venta.Id }, venta);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/venta/desde-reserva (endpoint público para clientes)
        [HttpPost("desde-reserva")]
        [AllowAnonymous]
        public async Task<IActionResult> RegistrarVentaDesdeReserva([FromBody] RegistrarVentaDTO dto)
        {
            try
            {
                // Por seguridad, solo permitimos efectivo desde este endpoint público
                if (!dto.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Solo se permite el método de pago en Efectivo desde este endpoint" });
                }

                var venta = await _ventaService.RegistrarVentaAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = venta.Id }, venta);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/venta/{id}/confirmar
        [HttpPost("{id:int}/confirmar")]
        [Authorize]
        public async Task<IActionResult> ConfirmarVenta(int id)
        {
            try
            {
                var venta = await _ventaService.ConfirmarVentaAsync(id);
                return Ok(venta);
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

        // PUT /api/venta/{id}/estado
        [HttpPut("{id:int}/estado")]
        [Authorize]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarVentaDTO dto)
        {
            try
            {
                var venta = await _ventaService.ActualizarEstadoAsync(id, dto);
                return Ok(venta);
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

        // DELETE /api/venta/{id}
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> AnularVenta(int id)
        {
            try
            {
                var resultado = await _ventaService.AnularVentaAsync(id);
                return resultado ? NoContent() : NotFound();
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
    }
}
