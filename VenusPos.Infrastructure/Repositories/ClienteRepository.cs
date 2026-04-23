using Microsoft.EntityFrameworkCore;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Infrastructure.Data;

namespace VenusPos.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly VenusPosDbContext _context;

        public ClienteRepository(VenusPosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosAsync()
            => await _context.Clientes.ToListAsync();

        public async Task<Cliente?> ObtenerPorIdAsync(int id)
            => await _context.Clientes.FindAsync(id);

        public async Task<Cliente?> ObtenerPorEmailAsync(string email)
            => await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);

        public async Task<Cliente> CrearClienteAsync(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<Cliente> ActualizarClienteAsync(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<bool> ExisteEmailAsync(string email)
            => await _context.Clientes.AnyAsync(c => c.Email == email);

        public async Task<bool> EliminarClienteAsync(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Mascota)
                    .ThenInclude(m => m.ReservaMascotas)
                .Include(c => c.Reserva)
                    .ThenInclude(r => r.ReservaMascotas)
                .Include(c => c.Ventas)
                    .ThenInclude(v => v.Detalles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente is null)
                return false;

            // Eliminar en cascada manual
            // 1. Eliminar ReservaMascota de las reservas del cliente
            foreach (var reserva in cliente.Reserva)
            {
                _context.ReservaMascotas.RemoveRange(reserva.ReservaMascotas);
            }

            // 2. Eliminar ReservaMascota de las mascotas del cliente
            foreach (var mascota in cliente.Mascota)
            {
                _context.ReservaMascotas.RemoveRange(mascota.ReservaMascotas);
            }

            // 3. Eliminar VentaDetalles
            foreach (var venta in cliente.Ventas)
            {
                _context.VentaDetalles.RemoveRange(venta.Detalles);
            }

            // 4. Eliminar Ventas
            _context.Ventas.RemoveRange(cliente.Ventas);

            // 5. Eliminar Reservas
            _context.Reservas.RemoveRange(cliente.Reserva);

            // 6. Eliminar Mascotas
            _context.Mascotas.RemoveRange(cliente.Mascota);

            // 7. Finalmente eliminar Cliente
            _context.Clientes.Remove(cliente);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}