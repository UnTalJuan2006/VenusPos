using VenusPos.Application.DTOs.ConfiguracionPrecio;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Application.Services.ConfiguracionPrecio
{
    public class ConfiguracionPrecioService : IConfiguracionPrecioService
    {
        private readonly IConfiguracionPrecioRepository _repo;

        public ConfiguracionPrecioService(IConfiguracionPrecioRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ConfiguracionPrecioDTO>> ObtenerTodosAsync()
        {
            var configs = await _repo.ObtenerTodosAsync();
            return configs.Select(MapToDTO);
        }

        public async Task<ConfiguracionPrecioDTO> ActualizarAsync(int id, ActualizarConfiguracionPrecioDTO dto)
        {
            var config = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Configuración con id {id} no encontrada.");

            config.Valor = dto.Valor;

            var actualizada = await _repo.ActualizarAsync(config);
            return MapToDTO(actualizada);
        }

        private static ConfiguracionPrecioDTO MapToDTO(Domain.Entities.ConfiguracionPrecio c) => new()
        {
            Id = c.Id,
            Clave = c.Clave,
            Valor = c.Valor,
            Descripcion = c.Descripcion
        };
    }
}
