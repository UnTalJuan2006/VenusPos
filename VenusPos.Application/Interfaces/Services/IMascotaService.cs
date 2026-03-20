using System;
using System.Collections.Generic;
using System.Text;
using VenusPos.Application.DTOs.Mascota;
namespace VenusPos.Application.Interfaces.Services
{
    public interface IMascotaService
    {
        Task<IEnumerable<MascotaDTO>> ObtenerTodosAsync();
        Task<IEnumerable<MascotaDTO>> ObtenerPorClienteAsync(int idCliente);
        Task<MascotaDTO?> ObtenerPorIdAsync(int id);
        Task<MascotaDTO> CrearAsync(CrearMascotaDTO dto);
        Task<MascotaDTO> ActualizarAsync(int id, ActualizarMascotaDTO dto);
        Task<bool> EliminarAsync(int id);
       
    }
}
