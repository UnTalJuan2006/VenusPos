using System;
using System.Collections.Generic;
using System.Text;
using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface IServicioRepository
    {
        Task<IEnumerable<Servicio>> ObtenerTodosAsync();
        Task<Servicio?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Servicio>> ObtenerPorIdsAsync(List<int> ids);
        Task<Servicio> CrearAsync(Servicio servicio);
        Task<Servicio> ActualizarAsync(Servicio servicio);
        Task<bool> EliminarAsync(int id);
    }
}
