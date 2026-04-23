namespace VenusPos.Application.DTOs.Reserva
{
    public class EmpleadoDisponibleDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Imagen { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public decimal Calificacion { get; set; } = 5.0m;
    }
}
