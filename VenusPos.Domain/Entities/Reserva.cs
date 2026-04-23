using VenusPos.Domain.Enums.Reserva;

namespace VenusPos.Domain.Entities;

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
    public string? CodigoReserva { get; set; }
    public decimal PrecioTotal { get; set; }
    public int DuracionMinutos { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public ICollection<ReservaMascota> ReservaMascotas { get; set; } = new List<ReservaMascota>();
    public ICollection<ReservaServicio> ReservaServicios { get; set; } = new List<ReservaServicio>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<Historial> Historiales { get; set; } = new List<Historial>();
}