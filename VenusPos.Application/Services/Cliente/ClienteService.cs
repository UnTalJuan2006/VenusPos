using VenusPos.Application.DTOs.Cliente;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;
using VenusPos.Domain.Entities;

namespace VenusPos.Application.Services.Cliente
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repo;

        public ClienteService(IClienteRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ClienteDTO>> ObtenerTodosAsync()
        {
            var clientes = await _repo.ObtenerTodosAsync();
            return clientes.Select(MapToDTO);
        }

        public async Task<ClienteDTO?> ObtenerPorIdAsync(int id)
        {
            var cliente = await _repo.ObtenerPorIdAsync(id);
            return cliente is null ? null : MapToDTO(cliente);
        }

        public async Task<ClienteDTO?> ObtenerPorEmailAsync(string email)
        {
            var cliente = await _repo.ObtenerPorEmailAsync(email);
            return cliente is null ? null : MapToDTO(cliente);
        }

        public async Task<ClienteDTO> CrearClienteAsync(CrearClienteDTO dto)
        {
            if (await _repo.ExisteEmailAsync(dto.Email))
                throw new InvalidOperationException("El email ya está registrado.");

            var cliente = new Domain.Entities.Cliente
            {
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                Email = dto.Email,
                FechaCreacion = DateTime.UtcNow
            };

            var creado = await _repo.CrearClienteAsync(cliente);
            return MapToDTO(creado);
        }

        public async Task<ClienteDTO> ActualizarClienteAsync(int id, ActualizarClienteDTO dto)
        {
            var cliente = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Cliente con id {id} no encontrado.");

            if (dto.Email != cliente.Email && await _repo.ExisteEmailAsync(dto.Email))
                throw new InvalidOperationException("El email ya está en uso por otro cliente.");

            cliente.Nombre = dto.Nombre;
            cliente.Telefono = dto.Telefono;
            cliente.Direccion = dto.Direccion;
            cliente.Email = dto.Email;

            var actualizado = await _repo.ActualizarClienteAsync(cliente);
            return MapToDTO(actualizado);
        }

        public async Task<bool> EliminarClienteAsync(int id)
        {
            return await _repo.EliminarClienteAsync(id);
        }

        private static ClienteDTO MapToDTO(Domain.Entities.Cliente c) => new()
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Telefono = c.Telefono,
            Direccion = c.Direccion,
            Email = c.Email,
            FechaCreacion = c.FechaCreacion
        };
    }
}