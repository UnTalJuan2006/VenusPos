using VenusPos.Application.DTOs.Ventas;

namespace VenusPos.Application.Interfaces.Services
{
    public interface IVentaService
    {
        Task<IEnumerable<VentaDTO>> ObtenerTodasAsync();
        Task<VentaDTO?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<VentaDTO>> ObtenerPorCajaAsync(int idCaja);
        Task<IEnumerable<VentaDTO>> ObtenerPendientesAsync();
        Task<VentaDTO?> ObtenerPorReservaAsync(int idReserva);
        Task<VentaDTO> RegistrarVentaAsync(RegistrarVentaDTO dto);
        Task<VentaDTO> ConfirmarVentaAsync(int id);
        Task<VentaDTO> ActualizarEstadoAsync(int id, ActualizarVentaDTO dto);
        Task<bool> AnularVentaAsync(int id);
        Task<bool> AnularVentaPorReservaAsync(int idReserva);
        Task<IEnumerable<ServicioVendidoDTO>> ObtenerServiciosMasVendidosAsync(int top = 10);
    }
}
