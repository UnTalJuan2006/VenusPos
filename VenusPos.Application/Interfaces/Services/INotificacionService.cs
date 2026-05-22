using VenusPos.Application.DTOs.Notificacion;

namespace VenusPos.Application.Interfaces.Services
{
    public interface INotificacionService
    {
        Task<IEnumerable<NotificacionDTO>> ObtenerTodosAsync();
        Task<IEnumerable<NotificacionDTO>> ObtenerNoLeidasAsync();
        Task<NotificacionDTO?> ObtenerPorIdAsync(int id);
        Task<int> ContarNoLeidasAsync();
        Task<NotificacionDTO> CrearAsync(CrearNotificacionDTO dto);
        Task<NotificacionDTO> MarcarLeidaAsync(int id);
        Task<bool> MarcarTodasLeidasAsync();
        Task<bool> EliminarAsync(int id);

        // Métodos auxiliares para generar notificaciones de eventos
        Task CrearNotificacionReservaAsync(int idReserva, string titulo, string mensaje, string tipo);
    }
}
