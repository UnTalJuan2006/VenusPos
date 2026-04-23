using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.DTOs.ConfiguracionPrecio;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConfiguracionPrecioController : ControllerBase
    {
        private readonly IConfiguracionPrecioService _service;

        public ConfiguracionPrecioController(IConfiguracionPrecioService service)
        {
            _service = service;
        }

        // GET /api/ConfiguracionPrecio
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _service.ObtenerTodosAsync());

        // PUT /api/ConfiguracionPrecio/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarConfiguracionPrecioDTO dto)
        {
            try
            {
                var actualizada = await _service.ActualizarAsync(id, dto);
                return Ok(actualizada);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
