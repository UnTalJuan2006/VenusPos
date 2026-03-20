namespace VenusPos.Domain.Entities
{
    public class Historial
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; }
        public int IdMascota { get; set; }
        public string Recomendaciones { get; set; } = string.Empty;
        public DateTime FechaAtencion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public Empleado Empleado { get; set; } = null!;
        public Mascota Mascota { get; set; } = null!;
    }
}