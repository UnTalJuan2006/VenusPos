using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.Interfaces.Services;
using VenusPos.Application.DTOs.Caja;


namespace VenusPos.Controllers
{

    [ApiController]
    [Route("api/[controller]")]

    public class CajaController : ControllerBase
    {
        private readonly ICajaService _cajaService;

        public CajaController(ICajaService cajaService)
        {
            _cajaService = cajaService;
        }

        // GET /api/caja
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _cajaService.ObtenerTodosAsync());

        // GET /api/caja/historial
        [HttpGet("historial")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerHistorial()
            => Ok(await _cajaService.ObtenerHistorialAsync());

        // GET /api/caja/abierta
        [HttpGet("abierta")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerCajaAbierta()
        {
            var cajaAbierta = await _cajaService.ObtenerCajaAbiertaAsync();
            return cajaAbierta is null ? NotFound() : Ok(cajaAbierta);
        }

        // GET /api/caja/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var caja = await _cajaService.ObtenerPorIdAsync(id);
            return caja is null ? NotFound() : Ok(caja);
        }

        // GET /api/caja/empleado/{idEmpleado}
        [HttpGet("empleado/{idEmpleado:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerCajaAbiertaPorEmpleado(int idEmpleado)
        {
            var caja = await _cajaService.ObtenerCajaAbiertaPorEmpleadoAsync(idEmpleado);
            return caja is null ? NotFound() : Ok(caja);
        }

        // GET /api/caja/fecha/{fecha}
        [HttpGet("fecha/{fecha:datetime}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorFecha(DateTime fecha)
        {
            var caja = await _cajaService.ObtenerPorFechaAsync(fecha);
            return caja is null ? NotFound() : Ok(caja);
        }

        // POST /api/caja/abrir
        [HttpPost("abrir")]
        [Authorize]
        public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaDTO dto)
        {
            try
            {
                var caja = await _cajaService.AbrirCajaAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = caja.Id }, caja);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/caja/{id}/cerrar
        [HttpPut("{id:int}/cerrar")]
        [Authorize]
        public async Task<IActionResult> CerrarCaja(int id, [FromBody] CerrarCajaDTO dto)
        {
            try
            {
                var caja = await _cajaService.CerrarCajaAsync(id, dto);
                return Ok(caja);
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

        // POST /api/caja/{id}/recalcular
        [HttpPost("{id:int}/recalcular")]
        [Authorize]
        public async Task<IActionResult> RecalcularTotales(int id)
        {
            try
            {
                await _cajaService.RecalcularTotalesAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


    }
}
