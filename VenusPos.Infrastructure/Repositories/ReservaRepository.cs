using Microsoft.EntityFrameworkCore;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Domain.Enums.Mascota;
using VenusPos.Domain.Enums.Reserva;
using VenusPos.Infrastructure.Data;

namespace VenusPos.Infrastructure.Repositories
{
    public class ReservaRepository : IReservaRepository
    {
        private readonly VenusPosDbContext _context;

        public ReservaRepository(VenusPosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reserva>> ObtenerTodosAsync()
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .ToListAsync();
        }

        public async Task<Reserva?> ObtenerPorIdAsync(int id)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Reserva?> ObtenerPorCodigoAsync(string codigo)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .FirstOrDefaultAsync(r => r.CodigoReserva == codigo);
        }

        public async Task<IEnumerable<Reserva>> ObtenerPorClienteAsync(int idCliente)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .Where(r => r.IdCliente == idCliente)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> ObtenerPorEmpleadoAsync(int idEmpleado)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .Where(r => r.IdEmpleado == idEmpleado)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> ObtenerPorEmpleadoYFechaAsync(int idEmpleado, DateTime fecha)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .Where(r => r.IdEmpleado == idEmpleado
                    && r.FechaReserva.Date == fecha.Date
                    && r.Estado != EnumEstado.Cancelada)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> ObtenerPorFechaAsync(DateTime fecha)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .Where(r => r.FechaReserva.Date == fecha.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> ObtenerPorFechaYTamañoAsync(DateTime fecha, EnumTamaño tamaño)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .Where(r => r.FechaReserva.Date == fecha.Date
                    && r.Estado != EnumEstado.Cancelada
                    && r.ReservaMascotas.Any(rm => rm.Mascota.Tamaño == tamaño))
                .ToListAsync();
        }

        public async Task<int> ObtenerSiguienteSecuencialAsync(DateTime fecha)
        {
            var ultimaReserva = await _context.Reservas
                .Where(r => r.FechaReserva.Date == fecha.Date && r.CodigoReserva != null)
                .OrderByDescending(r => r.CodigoReserva)
                .FirstOrDefaultAsync();

            if (ultimaReserva?.CodigoReserva == null)
                return 1;

            // Extraer NNN del formato RES-YYYYMMDD-NNN
            var partes = ultimaReserva.CodigoReserva.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out int secuencial))
            {
                return secuencial + 1;
            }

            return 1;
        }

        public async Task<Reserva> CrearAsync(Reserva reserva)
        {
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            return reserva;
        }

        public async Task<Reserva> ActualizarAsync(Reserva reserva)
        {
            reserva.FechaActualizacion = DateTime.UtcNow;
            _context.Reservas.Update(reserva);
            await _context.SaveChangesAsync();
            return reserva;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return false;

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Reserva>> ObtenerPorMascotaAsync(int idMascota)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .Where(r => r.ReservaMascotas.Any(rm => rm.IdMascota == idMascota))
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> ObtenerReservasSinVentaAsync()
        {
            // Obtener IDs de reservas que ya tienen venta
            var reservasConVenta = await _context.Ventas
                .Select(v => v.IdReserva)
                .ToListAsync();

            // Obtener reservas confirmadas o pendientes que NO tienen venta
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaMascotas)
                    .ThenInclude(rm => rm.Mascota)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .Where(r => (r.Estado == EnumEstado.Confirmada || r.Estado == EnumEstado.Pendiente)
                         && !reservasConVenta.Contains(r.Id))
                .OrderBy(r => r.FechaReserva)
                .ThenBy(r => r.HoraInicio)
                .ToListAsync();
        }
    }
}
