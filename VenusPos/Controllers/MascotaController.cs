using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.DTOs.Mascota;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MascotaController : ControllerBase
    {
        private readonly IMascotaService _service;
        private readonly IWebHostEnvironment _env;

        public MascotaController(IMascotaService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // GET /api/Mascota
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _service.ObtenerTodosAsync());

        // GET /api/Mascota/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var mascota = await _service.ObtenerPorIdAsync(id);
            return mascota is null ? NotFound() : Ok(mascota);
        }

        // GET /api/Mascota/cliente/{idCliente}
        [HttpGet("cliente/{idCliente:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerPorCliente(int idCliente)
            => Ok(await _service.ObtenerPorClienteAsync(idCliente));

        // POST /api/Mascota
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Crear([FromBody] CrearMascotaDTO dto)
        {
            try
            {
                var creada = await _service.CrearAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
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

        // PUT /api/Mascota/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarMascotaDTO dto)
        {
            try
            {
                return Ok(await _service.ActualizarAsync(id, dto));
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

        // DELETE /api/Mascota/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            return await _service.EliminarAsync(id) ? NoContent() : NotFound();
        }

        // POST /api/Mascota/upload-imagen
        [HttpPost("upload-imagen")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadImagen(IFormFile imagen)
        {
            if (imagen is null || imagen.Length == 0)
                return BadRequest(new { message = "No se recibio ningun archivo." });

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
                return BadRequest(new { message = "Formato no permitido. Usa JPG, PNG o WebP." });

            const long maxBytes = 3L * 1024 * 1024;
            if (imagen.Length > maxBytes)
                return BadRequest(new { message = "El archivo supera el tamaño maximo de 3 MB." });

            var carpeta = Path.Combine(_env.WebRootPath, "img", "mascotas");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            return Ok(new { url = $"/img/mascotas/{nombreArchivo}" });
        }
    }
}