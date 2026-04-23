using Microsoft.EntityFrameworkCore;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Infrastructure.Data;

namespace VenusPos.Infrastructure.Repositories
{
    public class CajaRepository : ICajaRepository
    {
        private readonly VenusPosDbContext _context;

        public CajaRepository(VenusPosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Caja>> ObtenerTodosAsync()
        {
            return await _context.Caja
                .Include(c => c.Empleado)
                .OrderByDescending(c => c.FechaApertura)
                .ToListAsync();
        }

        public async Task<IEnumerable<Caja>> ObtenerHistorialAsync()
        {
            return await _context.Caja
                .Include(c => c.Empleado)
                .Where(c => c.Estado == "Cerrada")
                .OrderByDescending(c => c.FechaCierre)
                .ToListAsync();
        }

        public async Task<Caja?> ObtenerCajaAbiertaAsync()
        {
            return await _context.Caja
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(c => c.Estado == "Abierta");
        }

        public async Task<Caja?> ObtenerCajaAbiertaPorEmpleadoAsync(int idEmpleado)
        {
            return await _context.Caja
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(c => c.IdEmpleado == idEmpleado && c.Estado == "Abierta");
        }

        public async Task<Caja?> ObtenerPorIdAsync(int id)
        {
            return await _context.Caja
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Caja?> ObtenerPorEmpleadoAsync(int idEmpleado)
        {
            return await _context.Caja
                .Include(c => c.Empleado)
                .Where(c => c.IdEmpleado == idEmpleado)
                .OrderByDescending(c => c.FechaApertura)
                .FirstOrDefaultAsync();
        }

        public async Task<Caja?> ObtenerPorFechaAsync(DateTime fecha)
        {
            return await _context.Caja
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(c => c.FechaApertura.Date == fecha.Date);
        }

        public async Task<Caja?> ObtenerConVentasYMovimientosAsync(int id)
        {
            return await _context.Caja
                .Include(c => c.Empleado)
                .Include(c => c.Ventas)
                    .ThenInclude(v => v.Cliente)
                .Include(c => c.MovimientosCaja)
                    .ThenInclude(m => m.Empleado)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Caja> CrearAsync(Caja caja)
        {
            _context.Caja.Add(caja);
            await _context.SaveChangesAsync();
            return caja;
        }

        public async Task<Caja> ActualizarAsync(Caja caja)
        {
     
            var cajaExistente = await _context.Caja
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == caja.Id);

            if (cajaExistente == null)
            {
                throw new KeyNotFoundException("Caja no encontrada");
            }

   
            _context.Caja.Attach(caja);

            var entry = _context.Entry(caja);
            entry.Property(c => c.MontoApertura).IsModified = true;
            entry.Property(c => c.MontoCierre).IsModified = true;
            entry.Property(c => c.TotalVentas).IsModified = true;
            entry.Property(c => c.TotalEfectivo).IsModified = true;
            entry.Property(c => c.TotalTarjeta).IsModified = true;
            entry.Property(c => c.TotalTransferencia).IsModified = true;
            entry.Property(c => c.Faltante).IsModified = true;
            entry.Property(c => c.Sobrante).IsModified = true;
            entry.Property(c => c.Estado).IsModified = true;
            entry.Property(c => c.Observaciones).IsModified = true;
            entry.Property(c => c.FechaApertura).IsModified = true;
            entry.Property(c => c.FechaCierre).IsModified = true;

            await _context.SaveChangesAsync();
            return caja;
        }
    }
}
