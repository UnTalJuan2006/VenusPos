using VenusPos.Application.DTOs.ConfiguracionPrecio;

namespace VenusPos.Application.Interfaces.Services
{
    public interface IConfiguracionPrecioService
    {
        Task<IEnumerable<ConfiguracionPrecioDTO>> ObtenerTodosAsync();
        Task<ConfiguracionPrecioDTO> ActualizarAsync(int id, ActualizarConfiguracionPrecioDTO dto);
    }
}
