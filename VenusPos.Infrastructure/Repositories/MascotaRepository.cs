using Microsoft.EntityFrameworkCore;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Infrastructure.Data;

namespace VenusPos.Infrastructure.Repositories
{
    public class MascotaRepository : IMascotaRepository
    {
        private readonly VenusPosDbContext _context;

        public MascotaRepository(VenusPosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Mascota>> ObtenerTodosAsync()
            => await _context.Mascotas
                .Include(m => m.Cliente)
                .ToListAsync();

        public async Task<Mascota?> ObtenerPorIdAsync(int id)
            => await _context.Mascotas
                .Include(m => m.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task<IEnumerable<Mascota>> ObtenerPorClienteAsync(int idCliente)
            => await _context.Mascotas
                .Include(m => m.Cliente)
                .Where(m => m.IdCliente == idCliente)
                .ToListAsync();

        public async Task<Mascota> CrearMascotaAsync(Mascota mascota)
        {
            _context.Mascotas.Add(mascota);
            await _context.SaveChangesAsync();
            return mascota;
        }

        public async Task<Mascota> ActualizarMascotaAsync(Mascota mascota)
        {
            _context.Mascotas.Update(mascota);
            await _context.SaveChangesAsync();
            return mascota;
        }

        public async Task<bool> EliminarMascotaAsync(int id)
        {
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota is null) return false;
            _context.Mascotas.Remove(mascota);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}