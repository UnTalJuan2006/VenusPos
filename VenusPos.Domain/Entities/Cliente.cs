namespace VenusPos.Domain.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public ICollection<Mascota> Mascota { get; set; }
        public ICollection<Reserva> Reserva { get; set; }
        public ICollection<Venta> Ventas { get; set; }
    }
}