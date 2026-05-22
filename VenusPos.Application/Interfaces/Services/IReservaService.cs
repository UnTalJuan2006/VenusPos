using VenusPos.Application.DTOs.Reserva;
using VenusPos.Domain.Enums.Mascota;

namespace VenusPos.Application.Interfaces.Services
{
    public interface IReservaService
    {
        // CRUD
        Task<IEnumerable<ReservaDTO>> ObtenerTodosAsync();
        Task<ReservaDTO?> ObtenerPorIdAsync(int id);
        Task<ReservaDTO?> ObtenerPorCodigoAsync(string codigo);
        Task<IEnumerable<ReservaDTO>> ObtenerPorClienteAsync(int idCliente);
        Task<IEnumerable<ReservaDTO>> ObtenerPorMascotaAsync(int idMascota);
        Task<ReservaDTO> CrearReservaAsync(CrearReservaDTO dto);
        Task<ReservaDTO> ActualizarEstadoAsync(int id, ActualizarEstadoReservaDTO dto);
        Task<ReservaDTO> ActualizarReservaAsync(int id, ActualizarReservaDTO dto);
        Task<bool> CancelarReservaAsync(int id);

        // Disponibilidad
        Task<List<HorarioDisponibleDTO>> ObtenerHorariosDisponiblesAsync(
            DateTime fecha, int duracionMinutos, int? idEmpleado = null, EnumTamaño? tamañoMascota = null);

        // Cálculos
        Task<CalculoPrecioDTO> CalcularPrecioReservaAsync(int idMascota, List<int> idsServicios);

        // Confirmación
        Task<ReservaDTO> ConfirmarReservaAsync(int id);

        // Consultas especiales
        Task<IEnumerable<ReservaDTO>> ObtenerReservasSinVentaAsync();
    }
}
