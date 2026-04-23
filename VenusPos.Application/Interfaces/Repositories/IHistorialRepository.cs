using System;
using System.Collections.Generic;
using System.Text;
using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface IHistorialRepository
    {
        Task<IEnumerable<Historial>> ObtenerTodosAsync();
        Task<Historial?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Historial>> ObtenerPorClienteAsync(int idCliente);
        Task<IEnumerable<Historial>> ObtenerPorMascotaAsync(int idMascota);
        Task<IEnumerable<Historial>> ObtenerPorEmpleadoAsync(int idEmpleado);
        Task<IEnumerable<Historial>> ObtenerPorReservaAsync(int idReserva);
        Task<Historial> CrearAsync(Historial historial);
        Task<Historial> ActualizarAsync(Historial historial);
    }
}
