using VenusPos.Domain.Entities;
using VenusPos.Domain.Enums.Reserva;

public class Reserva
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public int IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public DateTime FechaReserva { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public EnumEstado Estado { get; set; }
    public string Detalles { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public ICollection<ReservaMascota> ReservaMascotas { get; set; }
    public ICollection<Venta> Ventas { get; set; }  // ← cambiado
}