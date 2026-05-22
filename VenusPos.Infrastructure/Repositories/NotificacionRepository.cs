using Microsoft.EntityFrameworkCore;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Infrastructure.Data;

namespace VenusPos.Infrastructure.Repositories
{
    public class NotificacionRepository : INotificacionRepository
    {
        private readonly VenusPosDbContext _context;

        public NotificacionRepository(VenusPosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notificacion>> ObtenerTodosAsync()
        {
            return await _context.Notificaciones
                .Include(n => n.Reserva)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notificacion>> ObtenerNoLeidasAsync()
        {
            return await _context.Notificaciones
                .Include(n => n.Reserva)
                .Where(n => !n.Leida)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Notificacion?> ObtenerPorIdAsync(int id)
        {
            return await _context.Notificaciones
                .Include(n => n.Reserva)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Notificacion>> ObtenerPorReservaAsync(int idReserva)
        {
            return await _context.Notificaciones
                .Where(n => n.IdReserva == idReserva)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task<int> ContarNoLeidasAsync()
        {
            return await _context.Notificaciones
                .CountAsync(n => !n.Leida);
        }

        public async Task<Notificacion> CrearAsync(Notificacion notificacion)
        {
            notificacion.FechaCreacion = DateTime.UtcNow;
            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
            return notificacion;
        }

        public async Task<Notificacion> ActualizarAsync(Notificacion notificacion)
        {
            _context.Notificaciones.Update(notificacion);
            await _context.SaveChangesAsync();
            return notificacion;
        }

        public async Task<bool> MarcarTodasLeidasAsync()
        {
            var notificaciones = await _context.Notificaciones
                .Where(n => !n.Leida)
                .ToListAsync();

            foreach (var notificacion in notificaciones)
            {
                notificacion.Leida = true;
                notificacion.FechaLectura = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var notificacion = await _context.Notificaciones.FindAsync(id);
            if (notificacion == null) return false;

            _context.Notificaciones.Remove(notificacion);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
