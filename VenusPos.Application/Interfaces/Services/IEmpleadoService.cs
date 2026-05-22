using System;
using System.Collections.Generic;
using System.Text;
using VenusPos.Application.DTOs.Empleado;

namespace VenusPos.Application.Interfaces.Services
{
    public interface IEmpleadoService
    {
        Task<IEnumerable<EmpleadoDTO>> ObtenerTodosAsync();
        Task<EmpleadoDTO?> ObtenerPorIdAsync(int id);
        Task<EmpleadoDTO> CrearAsync(CrearEmpleadoDTO dto);
        Task<EmpleadoDTO> ActualizarAsync(int id, ActualizarEmpleadoDTO dto);
        Task<bool> EliminarAsync(int id);
        Task<string> LoginAsync(LoginEmpleadoDTO dto); // retorna JWT token
        Task<EmpleadoDTO> InactivarAsync(int id, InactivarEmpleadoDTO dto);
        Task<EmpleadoDTO> ReactivarAsync(int id);
    }
}