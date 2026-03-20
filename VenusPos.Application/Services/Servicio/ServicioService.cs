
using VenusPos.Application.DTOs.Servicio;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;
using VenusPos.Domain.Entities;

namespace VenusPos.Application.Services.Servicio
{
    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _repo;
       

        public ServicioService(IServicioRepository repo)
        {
            _repo = repo;
            
        } 
        public async Task<IEnumerable<ServicioDTO>> ObtenerTodosAsync()
        {
            var servicios = await _repo.ObtenerTodosAsync();
            return servicios.Select(MapToDTO);
        }
        public async Task<ServicioDTO?> ObtenerPorIdAsync(int id)
        {
            var servicio = await _repo.ObtenerPorIdAsync(id);
            return servicio is null ? null : MapToDTO(servicio);
        }

        public async Task<ServicioDTO> CrearAsync(CrearServicioDTO dto)
        {
            var servicio = new Domain.Entities.Servicio
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Activo = dto.Activo,
                FechaCreacion = DateTime.UtcNow
            };
            var creado = await _repo.CrearAsync(servicio);
            return MapToDTO(creado);
        }

        public async Task<ServicioDTO> ActualizarAsync(int id, ActualizarServicioDTO dto)
        {
            var servicio = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Servicio con id {id} no encontrado.");
            servicio.Nombre = dto.Nombre;
            servicio.Descripcion = dto.Descripcion;
            servicio.Precio = dto.Precio;
            servicio.Activo = dto.Activo;
            servicio.FechaActualizacion = DateTime.UtcNow;
            var actualizado = await _repo.ActualizarAsync(servicio);
            return MapToDTO(actualizado);
        }   

        public async Task<bool> EliminarAsync(int id)
        {
            return await _repo.EliminarAsync(id);
        }   

        public static ServicioDTO MapToDTO(Domain.Entities.Servicio servicio)
        {
            return new ServicioDTO
            {
                Id = servicio.Id,
                Nombre = servicio.Nombre,
                Descripcion = servicio.Descripcion,
                Precio = servicio.Precio,
                Activo = servicio.Activo,
                FechaCreacion = servicio.FechaCreacion
            };
        }
    }
}
