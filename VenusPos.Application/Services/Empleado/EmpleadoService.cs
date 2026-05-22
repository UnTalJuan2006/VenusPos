using VenusPos.Application.DTOs.Empleado;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace VenusPos.Application.Services.Empleado
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IEmpleadoRepository _repo;
        private readonly IConfiguration _config;

        public EmpleadoService(IEmpleadoRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<IEnumerable<EmpleadoDTO>> ObtenerTodosAsync()
        {
            var empleados = await _repo.ObtenerTodosAsync();
            return empleados.Select(MapToDTO);
        }

        public async Task<EmpleadoDTO?> ObtenerPorIdAsync(int id)
        {
            var empleado = await _repo.ObtenerPorIdAsync(id);
            return empleado is null ? null : MapToDTO(empleado);
        }

        public async Task<EmpleadoDTO> CrearAsync(CrearEmpleadoDTO dto)
        {
            if (await _repo.ExisteEmailAsync(dto.Email))
                throw new InvalidOperationException("El email ya está registrado.");

            var empleado = new Domain.Entities.Empleado  
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                Cargo = dto.Cargo,
                FechaNacimiento = dto.FechaNacimiento,
                FechaCreacion = DateTime.UtcNow,
                Imagen = dto.Imagen,
                Activo = dto.Activo
           
            };

            var creado = await _repo.CrearAsync(empleado);
            return MapToDTO(creado);
        }

        public async Task<EmpleadoDTO> ActualizarAsync(int id, ActualizarEmpleadoDTO dto)
        {
            var empleado = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException("Empleado no encontrado.");

            empleado.Nombre = dto.Nombre;
            empleado.Telefono = dto.Telefono;
            empleado.Direccion = dto.Direccion;
            empleado.Cargo = dto.Cargo;
            empleado.FechaNacimiento = dto.FechaNacimiento;
            empleado.Imagen = dto.Imagen;
            empleado.FechaActualizacion = DateTime.UtcNow;
            empleado.Activo = dto.Activo; 

            var actualizado = await _repo.ActualizarAsync(empleado);
            return MapToDTO(actualizado);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            if (await _repo.ObtenerPorIdAsync(id) is null)
                throw new KeyNotFoundException("Empleado no encontrado.");
            return await _repo.EliminarAsync(id);
        }

        public async Task<EmpleadoDTO> InactivarAsync(int id, InactivarEmpleadoDTO dto)
        {
            var empleado = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException("Empleado no encontrado.");

            // Para suspensión temporal, NO cambiar el campo Activo
            // Solo establecer las fechas de suspensión
            empleado.InactivoDesde = dto.InactivoDesde;
            empleado.InactivoHasta = dto.InactivoHasta;
            empleado.FechaActualizacion = DateTime.UtcNow;

            var actualizado = await _repo.ActualizarAsync(empleado);
            return MapToDTO(actualizado);
        }

        public async Task<EmpleadoDTO> ReactivarAsync(int id)
        {
            var empleado = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException("Empleado no encontrado.");

            empleado.Activo = true;
            empleado.InactivoDesde = default;
            empleado.InactivoHasta = default;
            empleado.FechaActualizacion = DateTime.UtcNow;

            var actualizado = await _repo.ActualizarAsync(empleado);
            return MapToDTO(actualizado);
        }

        public async Task<string> LoginAsync(LoginEmpleadoDTO dto)
        {
            var empleado = await _repo.ObtenerPorEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

            if (!VerifyPassword(dto.Password, empleado.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            return GenerarToken(empleado);
        }

        // ── Privados ──────────────────────────────────────────────────────────

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string hash)
            => HashPassword(password) == hash;

        private string GenerarToken(Domain.Entities.Empleado empleado)  // ✅ corregido
        {
            var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, empleado.Id.ToString()),
                new Claim(ClaimTypes.Email,          empleado.Email),
                new Claim(ClaimTypes.Name,           empleado.Nombre),
                new Claim(ClaimTypes.Role,           empleado.Cargo),
                
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static EmpleadoDTO MapToDTO(Domain.Entities.Empleado e) => new()
        {
            Id = e.Id,
            Nombre = e.Nombre,
            Email = e.Email,
            Telefono = e.Telefono,
            Direccion = e.Direccion,
            Cargo = e.Cargo,
            FechaNacimiento = e.FechaNacimiento,
            FechaCreacion = e.FechaCreacion,
            Imagen = e.Imagen,
            Activo = e.Activo,
            InactivoDesde = e.InactivoDesde,
            InactivoHasta = e.InactivoHasta
        };
    }
}