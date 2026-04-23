using Microsoft.EntityFrameworkCore;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Infrastructure.Data;

namespace VenusPos.Infrastructure.Repositories
{
    public class ConfiguracionPrecioRepository : IConfiguracionPrecioRepository
    {
        private readonly VenusPosDbContext _context;

        public ConfiguracionPrecioRepository(VenusPosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ConfiguracionPrecio>> ObtenerTodosAsync()
        {
            return await _context.ConfiguracionesPrecios.ToListAsync();
        }

        public async Task<ConfiguracionPrecio?> ObtenerPorIdAsync(int id)
        {
            return await _context.ConfiguracionesPrecios.FindAsync(id);
        }

        public async Task<ConfiguracionPrecio?> ObtenerPorClaveAsync(string clave)
        {
            return await _context.ConfiguracionesPrecios
                .FirstOrDefaultAsync(c => c.Clave == clave);
        }

        public async Task<ConfiguracionPrecio> ActualizarAsync(ConfiguracionPrecio config)
        {
            config.FechaActualizacion = DateTime.UtcNow;
            _context.ConfiguracionesPrecios.Update(config);
            await _context.SaveChangesAsync();
            return config;
        }
    }
}
