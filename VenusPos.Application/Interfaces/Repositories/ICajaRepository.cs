using System;
using System.Collections.Generic;
using System.Text;

using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface ICajaRepository
    {
        Task<IEnumerable<Caja>> ObtenerTodosAsync();
        Task<IEnumerable<Caja>> ObtenerHistorialAsync();
        Task<Caja?> ObtenerCajaAbiertaAsync();
        Task<Caja?> ObtenerCajaAbiertaPorEmpleadoAsync(int idEmpleado);
        Task<Caja?> ObtenerPorIdAsync(int id);
        Task<Caja?> ObtenerPorEmpleadoAsync(int idEmpleado);
        Task<Caja?> ObtenerPorFechaAsync(DateTime fecha);
        Task<Caja?> ObtenerConVentasYMovimientosAsync(int id);
        Task<Caja> CrearAsync(Caja caja);
        Task<Caja> ActualizarAsync(Caja caja);
    }

}
