using VenusPos.Domain.Enums.Reserva;

namespace VenusPos.Domain.Entities
{
    public class Empleado
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string? Imagen { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime InactivoDesde { get; set; }
        public DateTime InactivoHasta { get; set; }
        public ICollection<EmpleadoServicio> EmpleadoServicios { get; set; }
        public ICollection<Reserva> Reserva { get; set; }
        public ICollection<Caja> Caja { get; set; }
        public ICollection<Historial> Historial { get; set; }
        public ICollection<Venta> Ventas { get; set; }
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; }  
    }
}