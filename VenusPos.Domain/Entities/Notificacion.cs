using VenusPos.Domain.Enums.Notificacion;

namespace VenusPos.Domain.Entities;

public class Notificacion
{
    public int Id { get; set; }
    public EnumTipoNotificacion Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Icono { get; set; } = "🔔";
    public int? IdReserva { get; set; }
    public Reserva? Reserva { get; set; }
    public bool Leida { get; set; } = false;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLectura { get; set; }
}
