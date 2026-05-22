using Microsoft.EntityFrameworkCore;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Infrastructure.Data;

namespace VenusPos.Infrastructure.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly VenusPosDbContext _context;

        public EmpleadoRepository(VenusPosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Empleado>> ObtenerTodosAsync()
            => await _context.Empleados.ToListAsync();

        public async Task<Empleado?> ObtenerPorIdAsync(int id)
            => await _context.Empleados.FindAsync(id);

        public async Task<Empleado?> ObtenerPorEmailAsync(string email)
            => await _context.Empleados
                .FirstOrDefaultAsync(e => e.Email == email);

        public async Task<Empleado> CrearAsync(Empleado empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<Empleado> ActualizarAsync(Empleado empleado)
        {
            _context.Empleados.Update(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado is null) return false;
            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteEmailAsync(string email)
            => await _context.Empleados.AnyAsync(e => e.Email == email);

        public async Task<Empleado> InactivarEmpleadoAsync(Empleado empleado)
        {
            _context.Empleados.Update(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }
    }
}