using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface IConfiguracionPrecioRepository
    {
        Task<IEnumerable<ConfiguracionPrecio>> ObtenerTodosAsync();
        Task<ConfiguracionPrecio?> ObtenerPorIdAsync(int id);
        Task<ConfiguracionPrecio?> ObtenerPorClaveAsync(string clave);
        Task<ConfiguracionPrecio> ActualizarAsync(ConfiguracionPrecio config);
    }
}
