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
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null)
                return false;
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}