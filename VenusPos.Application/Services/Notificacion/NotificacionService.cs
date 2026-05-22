using VenusPos.Application.DTOs.Notificacion;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;
using VenusPos.Domain.Entities;
using VenusPos.Domain.Enums.Notificacion;

namespace VenusPos.Application.Services.Notificacion
{
    public class NotificacionService : INotificacionService
    {
        private readonly INotificacionRepository _notificacionRepo;
        private readonly IReservaRepository _reservaRepo;

        public NotificacionService(
            INotificacionRepository notificacionRepo,
            IReservaRepository reservaRepo)
        {
            _notificacionRepo = notificacionRepo;
            _reservaRepo = reservaRepo;
        }

        public async Task<IEnumerable<NotificacionDTO>> ObtenerTodosAsync()
        {
            var notificaciones = await _notificacionRepo.ObtenerTodosAsync();
            return notificaciones.Select(MapToDTO);
        }

        public async Task<IEnumerable<NotificacionDTO>> ObtenerNoLeidasAsync()
        {
            var notificaciones = await _notificacionRepo.ObtenerNoLeidasAsync();
            return notificaciones.Select(MapToDTO);
        }

        public async Task<NotificacionDTO?> ObtenerPorIdAsync(int id)
        {
            var notificacion = await _notificacionRepo.ObtenerPorIdAsync(id);
            return notificacion is null ? null : MapToDTO(notificacion);
        }

        public async Task<int> ContarNoLeidasAsync()
        {
            return await _notificacionRepo.ContarNoLeidasAsync();
        }

        public async Task<NotificacionDTO> CrearAsync(CrearNotificacionDTO dto)
        {
            var notificacion = new Domain.Entities.Notificacion
            {
                Tipo = Enum.Parse<EnumTipoNotificacion>(dto.Tipo),
                Titulo = dto.Titulo,
                Mensaje = dto.Mensaje,
                Icono = dto.Icono,
                IdReserva = dto.IdReserva,
                Leida = false,
                FechaCreacion = DateTime.UtcNow
            };

            var creada = await _notificacionRepo.CrearAsync(notificacion);
            return MapToDTO(creada);
        }

        public async Task<NotificacionDTO> MarcarLeidaAsync(int id)
        {
            var notificacion = await _notificacionRepo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Notificación con id {id} no encontrada.");

            if (!notificacion.Leida)
            {
                notificacion.Leida = true;
                notificacion.FechaLectura = DateTime.UtcNow;
                await _notificacionRepo.ActualizarAsync(notificacion);
            }

            return MapToDTO(notificacion);
        }

        public async Task<bool> MarcarTodasLeidasAsync()
        {
            return await _notificacionRepo.MarcarTodasLeidasAsync();
        }

        public async Task<bool> EliminarAsync(int id)
        {
            return await _notificacionRepo.EliminarAsync(id);
        }

        public async Task CrearNotificacionReservaAsync(int idReserva, string titulo, string mensaje, string tipo)
        {
            var reserva = await _reservaRepo.ObtenerPorIdAsync(idReserva);
            if (reserva is null) return;

            var iconos = new Dictionary<string, string>
            {
                { "NuevaReserva", "📅" },
                { "ReservaCancelada", "❌" },
                { "ReservaProxima", "⏰" },
                { "PagoRecibido", "💰" },
                { "Sistema", "🔔" }
            };

            var dto = new CrearNotificacionDTO
            {
                Tipo = tipo,
                Titulo = titulo,
                Mensaje = mensaje,
                Icono = iconos.GetValueOrDefault(tipo, "🔔"),
                IdReserva = idReserva
            };

            await CrearAsync(dto);
        }

        private static NotificacionDTO MapToDTO(Domain.Entities.Notificacion notificacion)
        {
            return new NotificacionDTO
            {
                Id = notificacion.Id,
                Tipo = notificacion.Tipo.ToString(),
                Titulo = notificacion.Titulo,
                Mensaje = notificacion.Mensaje,
                Icono = notificacion.Icono,
                IdReserva = notificacion.IdReserva,
                CodigoReserva = notificacion.Reserva?.CodigoReserva,
                Leida = notificacion.Leida,
                FechaCreacion = notificacion.FechaCreacion,
                FechaLectura = notificacion.FechaLectura
            };
        }
    }
}
