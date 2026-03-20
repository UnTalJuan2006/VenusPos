using System;
using System.Collections.Generic;
using System.Text;
using VenusPos.Application.DTOs.Servicio;

namespace VenusPos.Application.Interfaces.Services
{
    public interface IServicioService
    {
        Task<IEnumerable<ServicioDTO>> ObtenerTodosAsync();
        Task<ServicioDTO?> ObtenerPorIdAsync(int id);   
        Task<ServicioDTO> CrearAsync(CrearServicioDTO dto);
        Task<ServicioDTO> ActualizarAsync(int id, ActualizarServicioDTO dto);
        Task<bool> EliminarAsync(int id);
        

    }
}
