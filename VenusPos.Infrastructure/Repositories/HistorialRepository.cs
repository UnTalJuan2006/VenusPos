using Microsoft.EntityFrameworkCore;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Infrastructure.Data;

namespace VenusPos.Infrastructure.Repositories
{
    public class HistorialRepository : IHistorialRepository
    {
        private readonly VenusPosDbContext _context;

        public HistorialRepository(VenusPosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Historial>> ObtenerTodosAsync()
        {
            return await _context.Historiales
                .Include(h => h.Empleado)
                .Include(h => h.Mascota)
                    .ThenInclude(m => m.Cliente)
                .Include(h => h.Reserva)
                .OrderByDescending(h => h.FechaAtencion)
                .ToListAsync();
        }

        public async Task<Historial?> ObtenerPorIdAsync(int id)
        {
            return await _context.Historiales
                .Include(h => h.Empleado)
                .Include(h => h.Mascota)
                    .ThenInclude(m => m.Cliente)
                .Include(h => h.Reserva)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<IEnumerable<Historial>> ObtenerPorClienteAsync(int idCliente)
        {
            return await _context.Historiales
                .Include(h => h.Empleado)
                .Include(h => h.Mascota)
                    .ThenInclude(m => m.Cliente)
                .Include(h => h.Reserva)
                .Where(h => h.Mascota.IdCliente == idCliente)
                .OrderByDescending(h => h.FechaAtencion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Historial>> ObtenerPorEmpleadoAsync(int idEmpleado)
        {
            return await _context.Historiales
                .Include(h => h.Empleado)
                .Include(h => h.Mascota)
                    .ThenInclude(m => m.Cliente)
                .Include(h => h.Reserva)
                .Where(h => h.IdEmpleado == idEmpleado)
                .OrderByDescending(h => h.FechaAtencion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Historial>> ObtenerPorMascotaAsync(int idMascota)
        {
            return await _context.Historiales
                .Include(h => h.Empleado)
                .Include(h => h.Mascota)
                    .ThenInclude(m => m.Cliente)
                .Include(h => h.Reserva)
                .Where(h => h.IdMascota == idMascota)
                .OrderByDescending(h => h.FechaAtencion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Historial>> ObtenerPorReservaAsync(int idReserva)
        {
            return await _context.Historiales
                .Include(h => h.Empleado)
                .Include(h => h.Mascota)
                    .ThenInclude(m => m.Cliente)
                .Include(h => h.Reserva)
                .Where(h => h.IdReserva == idReserva)
                .OrderByDescending(h => h.FechaAtencion)
                .ToListAsync();
        }

        public async Task<Historial> CrearAsync(Historial historial)
        {
            historial.FechaCreacion = DateTime.Now;
            _context.Historiales.Add(historial);
            await _context.SaveChangesAsync();

            // Recargar con relaciones
            return await ObtenerPorIdAsync(historial.Id) ?? historial;
        }

        public async Task<Historial> ActualizarAsync(Historial historial)
        {
            historial.FechaActualizacion = DateTime.Now;
            _context.Historiales.Update(historial);
            await _context.SaveChangesAsync();

            // Recargar con relaciones
            return await ObtenerPorIdAsync(historial.Id) ?? historial;
        }
    }
}
