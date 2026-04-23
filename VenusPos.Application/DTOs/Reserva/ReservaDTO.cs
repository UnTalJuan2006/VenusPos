namespace VenusPos.Application.DTOs.Reserva
{
    public class ReservaDTO
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public int IdMascota { get; set; }
        public string NombreMascota { get; set; } = string.Empty;
        public string RazaMascota { get; set; } = string.Empty;
        public string TamañoMascota { get; set; } = string.Empty;
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public string ImagenEmpleado { get; set; } = string.Empty;
        public DateTime FechaReserva { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int DuracionMinutos { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? CodigoReserva { get; set; }
        public decimal PrecioTotal { get; set; }
        public List<ServicioReservaDTO> Servicios { get; set; } = new();
        public string Detalles { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
