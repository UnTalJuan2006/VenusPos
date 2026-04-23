using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenusPos.Application.DTOs.Cliente;
using VenusPos.Application.DTOs.Empleado;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ClienteController : ControllerBase 
    {
        private readonly IClienteService _service;
        private readonly IWebHostEnvironment _env;

        public ClienteController(IClienteService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }


        //Get/api/cliente
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _service.ObtenerTodosAsync());
        
        //Get/api/cliente/{id}
        [HttpGet("{id:int}")]
        [Authorize]

        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var cliente = await _service.ObtenerPorIdAsync(id);
            return cliente is null ? NotFound() : Ok(cliente);
        }

        //Post/api/cliente
        [HttpPost]

        public async Task<IActionResult> CrearCliente([FromBody] CrearClienteDTO dto)
        {
            try
            {
                var creado = await _service.CrearClienteAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //Put/api/cliente/{id}
        [HttpPut("{id:int}")]
        [Authorize]

        public async Task<IActionResult> ActualizarCliente(int id, [FromBody] ActualizarClienteDTO dto)
        {
            try
            {
                return Ok(await _service.ActualizarClienteAsync(id, dto));
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        //DELETE/api/cliente/{id}
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                return await _service.EliminarClienteAsync(id) ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        // POST /api/Cliente/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] IngresoClienteDTO dto)
        {
            try
            {
                var cliente = await _service.LoginAsync(dto);
                return Ok(cliente);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

    }
}
