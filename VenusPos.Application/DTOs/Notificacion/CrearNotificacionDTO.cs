namespace VenusPos.Application.DTOs.Notificacion
{
    public class CrearNotificacionDTO
    {
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Icono { get; set; } = "🔔";
        public int? IdReserva { get; set; }
    }
}
