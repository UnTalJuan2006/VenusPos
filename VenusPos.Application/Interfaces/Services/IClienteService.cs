using VenusPos.Application.DTOs.Cliente;

namespace VenusPos.Application.Interfaces.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteDTO>> ObtenerTodosAsync();
        Task<ClienteDTO?> ObtenerPorIdAsync(int id);
        Task<ClienteDTO?> ObtenerPorEmailAsync(string email);
        Task<ClienteDTO> CrearClienteAsync(CrearClienteDTO dto);
        Task<ClienteDTO> ActualizarClienteAsync(int id, ActualizarClienteDTO dto);
        Task<bool> EliminarClienteAsync(int id);
    }
}