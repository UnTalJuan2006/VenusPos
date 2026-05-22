using System;
using System.Collections.Generic;
using System.Text;

using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface IEmpleadoRepository
    {
        Task<IEnumerable<Empleado>> ObtenerTodosAsync();
        Task<Empleado?> ObtenerPorIdAsync(int id);
        Task<Empleado?> ObtenerPorEmailAsync(string email);
        Task<Empleado> CrearAsync(Empleado empleado);
        Task<Empleado> ActualizarAsync(Empleado empleado);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteEmailAsync(string email);
        Task<Empleado> InactivarEmpleadoAsync(Empleado empleado);
    }
}