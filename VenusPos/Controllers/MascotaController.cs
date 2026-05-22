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
        private readonly IAzureBlobStorageService _azureStorage;

        public MascotaController(
            IMascotaService service,
            IWebHostEnvironment env,
            IAzureBlobStorageService azureStorage)
        {
            _service = service;
            _env = env;
            _azureStorage = azureStorage;
        }

        // GET /api/Mascota
        [HttpGet]
        [AllowAnonymous]
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

        // POST /api/Mascota/upload-imagen - Subir imagen a Azure Blob Storage
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

            try
            {
                // Generar nombre único para el archivo
                var nombreArchivo = $"{Guid.NewGuid()}{extension}";

                // Determinar el Content-Type según la extensión
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };

                // Subir a Azure Blob Storage
                using var stream = imagen.OpenReadStream();
                var urlAzure = await _azureStorage.SubirArchivoAsync(
                    stream,
                    nombreArchivo,
                    contentType,
                    "mascotas"
                );

                return Ok(new { url = urlAzure });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al subir imagen: {ex.Message}" });
            }
        }
    }
}