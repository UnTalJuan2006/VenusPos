using VenusPos.Application.DTOs.Mascota;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;
using VenusPos.Domain.Enums.Mascota;

namespace VenusPos.Application.Services.Mascota
{
    public class MascotaService : IMascotaService
    {
        private readonly IMascotaRepository _repo;

        public MascotaService(IMascotaRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<MascotaDTO>> ObtenerPorClienteAsync(int idCliente)
        {
            var mascotas = await _repo.ObtenerPorClienteAsync(idCliente);
            return mascotas.Select(MapToDTO);
        }
        public async Task<IEnumerable<MascotaDTO>> ObtenerTodosAsync()
        {
            var mascotas = await _repo.ObtenerTodosAsync();
            return mascotas.Select(MapToDTO);
        }

        public async Task<MascotaDTO?> ObtenerPorIdAsync(int id)
        {
            var mascota = await _repo.ObtenerPorIdAsync(id);
            return mascota is null ? null : MapToDTO(mascota);
        }

        public async Task<MascotaDTO> CrearAsync(CrearMascotaDTO dto)
        {
            var mascota = new Domain.Entities.Mascota
            {
                Nombre = dto.Nombre,
                Raza = dto.Raza,
                Tamaño = Enum.Parse<EnumTamaño>(dto.Tamano),
                Edad = dto.Edad,
                TipoPelaje = Enum.Parse<EnumTipoPelaje>(dto.TipoPelaje),
                Observaciones = dto.Observaciones,
                FechaNacimiento = dto.FechaNacimiento,
                Imagen = dto.Imagen,
                IdCliente = dto.IdCliente,
                FechaCreacion = DateTime.UtcNow
            };
            var creada = await _repo.CrearMascotaAsync(mascota);
            return MapToDTO(creada);
        }

        public async Task<MascotaDTO> ActualizarAsync(int id, ActualizarMascotaDTO dto)
        {
            var mascota = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Mascota con id {id} no encontrada.");

            mascota.Nombre = dto.Nombre;
            mascota.Raza = dto.Raza;
            mascota.Tamaño = Enum.Parse<EnumTamaño>(dto.Tamano);
            mascota.Edad = dto.Edad;
            mascota.TipoPelaje = Enum.Parse<EnumTipoPelaje>(dto.TipoPelaje);
            mascota.Observaciones = dto.Observaciones;
            mascota.FechaNacimiento = dto.FechaNacimiento;
            mascota.Imagen = dto.Imagen;
            mascota.FechaActualizacion = DateTime.UtcNow;

            var actualizada = await _repo.ActualizarMascotaAsync(mascota);
            return MapToDTO(actualizada);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            return await _repo.EliminarMascotaAsync(id);
        }

        private static MascotaDTO MapToDTO(Domain.Entities.Mascota m) => new()
        {
            Id = m.Id,
            Nombre = m.Nombre,
            Raza = m.Raza,
            Tamaño = m.Tamaño.ToString(),
            Edad = m.Edad,
            TipoPelaje = m.TipoPelaje.ToString(),
            Observaciones = m.Observaciones,
            FechaNacimiento = m.FechaNacimiento,
            FechaCreacion = m.FechaCreacion,
            Imagen = m.Imagen,
            IdCliente = m.IdCliente,
            NombreCliente = m.Cliente?.Nombre ?? string.Empty
        };
    }
}