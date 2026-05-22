using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.DTOs.Empleado;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadoController : ControllerBase
    {
        private readonly IEmpleadoService _service;
        private readonly IWebHostEnvironment _env;
        private readonly IAzureBlobStorageService _azureStorage;

        public EmpleadoController(
            IEmpleadoService service,
            IWebHostEnvironment env,
            IAzureBlobStorageService azureStorage)
        {
            _service = service;
            _env = env;
            _azureStorage = azureStorage;
        }

        // GET /api/Empleado
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _service.ObtenerTodosAsync());

        // GET /api/Empleado/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var empleado = await _service.ObtenerPorIdAsync(id);
            return empleado is null ? NotFound() : Ok(empleado);
        }

        // POST /api/Empleado
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearEmpleadoDTO dto)
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

        // PUT /api/Empleado/{id}
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarEmpleadoDTO dto)
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

        // DELETE /api/Empleado/{id}
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
        // PUT /api/Empleado/inactivar/{id}
        [HttpPut("inactivar/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Inactivar(int id, [FromBody] InactivarEmpleadoDTO dto)
        {
            try
            {
                return Ok(await _service.InactivarAsync(id, dto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT /api/Empleado/reactivar/{id}
        [HttpPut("reactivar/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Reactivar(int id)
        {
            try
            {
                return Ok(await _service.ReactivarAsync(id));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }




        // POST /api/Empleado/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginEmpleadoDTO dto)
        {
            try
            {
                var token = await _service.LoginAsync(dto);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // POST /api/Empleado/upload-imagen
        //
        // Flujo completo con Azure Blob Storage:
        //   1. Frontend selecciona archivo → lo envía aquí como multipart/form-data
        //   2. Este endpoint valida y sube el archivo a Azure Blob Storage (contenedor: empleados)
        //   3. Devuelve { url: "https://tuaccount.blob.core.windows.net/empleados/xxx.jpg" }
        //   4. Frontend guarda esa URL en la variable imagenUrl
        //   5. Al crear/editar el empleado, esa URL viaja en el campo "imagen" del JSON
        //   6. El Service la guarda en la BD directamente desde el DTO
        // ══════════════════════════════════════════════════════════════════════
        [HttpPost("upload-imagen")]
        [Authorize]
        public async Task<IActionResult> UploadImagen(IFormFile imagen)
        {
            if (imagen is null || imagen.Length == 0)
                return BadRequest(new { message = "No se recibió ningún archivo." });

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
                return BadRequest(new { message = "Formato no permitido. Usa JPG, PNG o WebP." });

            const long maxBytes = 3L * 1024 * 1024;
            if (imagen.Length > maxBytes)
                return BadRequest(new { message = "El archivo supera el tamaño máximo de 3 MB." });

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
                    "empleados"
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