using System;
using System.Collections.Generic;
using System.Text;

using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface IClienteRepository
    {
        Task<IEnumerable<Cliente>> ObtenerTodosAsync();
        Task<Cliente?> ObtenerPorIdAsync(int id);
        Task<Cliente?> ObtenerPorEmailAsync(String email);
        Task<Cliente> CrearClienteAsync(Cliente cliente);
        Task<Cliente> ActualizarClienteAsync(Cliente cliente);
        Task<bool> ExisteEmailAsync(string email);
        Task<bool> EliminarClienteAsync(int id); 
    }
}
