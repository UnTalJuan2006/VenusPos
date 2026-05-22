namespace VenusPos.Application.DTOs.Notificacion
{
    public class NotificacionDTO
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public int? IdReserva { get; set; }
        public string? CodigoReserva { get; set; }
        public bool Leida { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLectura { get; set; }
    }
}
