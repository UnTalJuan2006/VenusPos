using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface INotificacionRepository
    {
        Task<IEnumerable<Notificacion>> ObtenerTodosAsync();
        Task<IEnumerable<Notificacion>> ObtenerNoLeidasAsync();
        Task<Notificacion?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Notificacion>> ObtenerPorReservaAsync(int idReserva);
        Task<int> ContarNoLeidasAsync();
        Task<Notificacion> CrearAsync(Notificacion notificacion);
        Task<Notificacion> ActualizarAsync(Notificacion notificacion);
        Task<bool> MarcarTodasLeidasAsync();
        Task<bool> EliminarAsync(int id);
    }
}
