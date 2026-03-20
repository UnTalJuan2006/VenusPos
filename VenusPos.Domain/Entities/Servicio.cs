namespace VenusPos.Domain.Entities
{
    public class Servicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public ICollection<MascotaServicio> MascotaServicios { get; set; }
        public ICollection<EmpleadoServicio> EmpleadoServicios { get; set; }
        public ICollection<VentaDetalle> VentaDetalles { get; set; } 



    }
}