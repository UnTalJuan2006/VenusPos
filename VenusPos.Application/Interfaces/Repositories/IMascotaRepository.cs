using System;
using System.Collections.Generic;
using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface IMascotaRepository
    {
        Task<IEnumerable<Mascota>> ObtenerTodosAsync();
        Task<Mascota?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Mascota>> ObtenerPorClienteAsync(int idCliente);
        Task<Mascota> CrearMascotaAsync(Mascota mascota);
        Task<Mascota> ActualizarMascotaAsync(Mascota mascota);
        Task<bool> EliminarMascotaAsync(int id);
    }
}