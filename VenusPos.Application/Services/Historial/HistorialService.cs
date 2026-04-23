using VenusPos.Application.DTOs.Historial;
using VenusPos.Application.DTOs.Mascota;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Application.Services.Historial
{
    public class HistorialService : IHistorialService
    {
        private readonly IHistorialRepository _repo;

        public HistorialService(IHistorialRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<HistorialDTO>> ObtenerTodosAsync()
        {
            var historiales = await _repo.ObtenerTodosAsync();
            return historiales.Select(MapToDTO);
        }

        public async Task<HistorialDTO?> ObtenerPorIdAsync(int id)
        {
            var historial = await _repo.ObtenerPorIdAsync(id);
            return historial is null ? null : MapToDTO(historial);
        }

        public async Task<IEnumerable<HistorialDTO>> ObtenerPorMascotaAsync(int idMascota)
        {
            var historiales = await _repo.ObtenerPorMascotaAsync(idMascota);
            return historiales.Select(MapToDTO);
        }

        public async Task<IEnumerable<HistorialDTO>> ObtenerPorEmpleadoAsync(int idEmpleado)
        {
            var historiales = await _repo.ObtenerPorEmpleadoAsync(idEmpleado);
            return historiales.Select(MapToDTO);
        }

        public async Task<IEnumerable<HistorialDTO>> ObtenerPorReservaAsync(int idReserva)
        {
            var historiales = await _repo.ObtenerPorReservaAsync(idReserva);
            return historiales.Select(MapToDTO);
        }

        public async Task<HistorialDTO> CrearAsync(CrearHistorialDTO dto)
        {
            var historial = new Domain.Entities.Historial
            {
                IdMascota = dto.IdMascota,
                IdEmpleado = dto.IdEmpleado,
                IdReserva = dto.IdReserva,
                Recomendaciones = dto.Recomendaciones,
                FechaAtencion = DateTime.UtcNow,
                FechaCreacion = DateTime.UtcNow
            };

            var creado = await _repo.CrearAsync(historial);
            return MapToDTO(creado);
        }

        public async Task<HistorialDTO> ActualizarAsync(int id, ActualizarHistorialDTO dto)
        {
            var historial = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Historial con id {id} no encontrado.");

            historial.IdEmpleado = dto.IdEmpleado;
            historial.Recomendaciones = dto.Recomendaciones;
            historial.FechaActualizacion = DateTime.UtcNow;

            var actualizado = await _repo.ActualizarAsync(historial);
            return MapToDTO(actualizado);
        }

        private HistorialDTO MapToDTO(Domain.Entities.Historial h) => new HistorialDTO
        {
            Id = h.Id,

            // ✅ Mascota completa
            Mascota = new MascotaDTO
            {
                Id = h.Mascota.Id,
                Nombre = h.Mascota.Nombre,
                Raza = h.Mascota.Raza,
                Tamaño = h.Mascota.Tamaño.ToString(),
                Edad = h.Mascota.Edad,
                TipoPelaje = h.Mascota.TipoPelaje.ToString(),
                Observaciones = h.Mascota.Observaciones,
                Imagen = h.Mascota.Imagen,
                IdCliente = h.Mascota.IdCliente,
                NombreCliente = h.Mascota.Cliente?.Nombre ?? string.Empty,
                FechaNacimiento = h.Mascota.FechaNacimiento,
                FechaCreacion = h.Mascota.FechaCreacion,
            },

            IdEmpleado = h.IdEmpleado,
            NombreEmpleado = h.Empleado?.Nombre ?? string.Empty,
            CargoEmpleado = h.Empleado?.Cargo ?? string.Empty,
            ImagenEmpleado = h.Empleado?.Imagen ?? string.Empty,
            Email = h.Empleado?.Email ?? string.Empty,

            // ✅ Reserva (puede ser null)
            IdReserva = h.IdReserva,
            FechaReserva = h.Reserva?.FechaReserva,
            CodigoReserva = h.Reserva?.CodigoReserva,

            // ✅ Historial
            Recomendaciones = h.Recomendaciones,
            FechaAtencion = h.FechaAtencion,
            FechaCreacion = h.FechaCreacion,
            FechaActualizacion = h.FechaActualizacion ?? h.FechaCreacion
        };
    }
}