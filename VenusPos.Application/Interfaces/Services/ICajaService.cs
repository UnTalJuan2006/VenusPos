using VenusPos.Application.DTOs.Caja;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Application.Interfaces.Services
{
    
    public interface ICajaService
    {
        Task<IEnumerable<CajaDTO>> ObtenerTodosAsync();
        Task<IEnumerable<CajaDTO>> ObtenerHistorialAsync();
        Task<CajaDTO?> ObtenerCajaAbiertaAsync();
        Task<CajaDTO?> ObtenerCajaAbiertaPorEmpleadoAsync(int idEmpleado);
        Task<CajaDTO?> ObtenerPorIdAsync(int id);
        Task<CajaDTO?> ObtenerPorEmpleadoAsync(int idEmpleado);
        Task<CajaDTO?> ObtenerPorFechaAsync(DateTime fecha);
        Task<CajaDTO> AbrirCajaAsync(AbrirCajaDTO dto);
        Task<CajaDTO> CerrarCajaAsync(int id, CerrarCajaDTO dto);
        Task RecalcularTotalesAsync(int idCaja);
    }
}
