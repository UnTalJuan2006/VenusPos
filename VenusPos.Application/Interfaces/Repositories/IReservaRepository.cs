using VenusPos.Domain.Entities;
using VenusPos.Domain.Enums.Mascota;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface IReservaRepository
    {
        Task<IEnumerable<Reserva>> ObtenerTodosAsync();
        Task<Reserva?> ObtenerPorIdAsync(int id);
        Task<Reserva?> ObtenerPorCodigoAsync(string codigo);
        Task<IEnumerable<Reserva>> ObtenerPorClienteAsync(int idCliente);
        Task<IEnumerable<Reserva>> ObtenerPorEmpleadoAsync(int idEmpleado);
        Task<IEnumerable<Reserva>> ObtenerPorEmpleadoYFechaAsync(int idEmpleado, DateTime fecha);
        Task<IEnumerable<Reserva>> ObtenerPorFechaAsync(DateTime fecha);
        Task<IEnumerable<Reserva>> ObtenerPorFechaYTamañoAsync(DateTime fecha, EnumTamaño tamaño);
        Task<int> ObtenerSiguienteSecuencialAsync(DateTime fecha);
        Task<IEnumerable<Reserva>> ObtenerPorMascotaAsync(int idMascota);
        Task<Reserva> CrearAsync(Reserva reserva);
        Task<Reserva> ActualizarAsync(Reserva reserva);
        Task<bool> EliminarAsync(int id);
        Task<IEnumerable<Reserva>> ObtenerReservasSinVentaAsync();
    }
}
