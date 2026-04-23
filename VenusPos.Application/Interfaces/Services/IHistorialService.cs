using System;
using System.Collections.Generic;
using System.Text;
using VenusPos.Application.DTOs.Historial;


namespace VenusPos.Application.Interfaces.Services
{
    public interface IHistorialService
    {
        Task<IEnumerable<HistorialDTO>> ObtenerTodosAsync();
        Task<HistorialDTO?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<HistorialDTO>> ObtenerPorMascotaAsync(int idMascota);
        Task<IEnumerable<HistorialDTO>> ObtenerPorEmpleadoAsync(int idEmpleado);
        Task<IEnumerable<HistorialDTO>> ObtenerPorReservaAsync(int idReserva);
        Task<HistorialDTO> CrearAsync(CrearHistorialDTO dto);
        Task<HistorialDTO> ActualizarAsync(int id, ActualizarHistorialDTO dto);
    }
}
