using Microsoft.EntityFrameworkCore;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Infrastructure.Data;


namespace VenusPos.Infrastructure.Repositories
{
    public class ServicioRepository : IServicioRepository
    {
        private readonly VenusPosDbContext _context;
        public ServicioRepository(VenusPosDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Servicio>> ObtenerTodosAsync()
            => await _context.Servicios.ToListAsync();
        public async Task<Servicio?> ObtenerPorIdAsync(int id)
            => await _context.Servicios.FindAsync(id);
        public async Task<Servicio> CrearAsync(Servicio servicio)
        {
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();
            return servicio;
        }
        public async Task<Servicio> ActualizarAsync(Servicio servicio)
        {
            _context.Servicios.Update(servicio);
            await _context.SaveChangesAsync();
            return servicio;
        }
        public async Task<bool> EliminarAsync(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio is null) return false;
            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
