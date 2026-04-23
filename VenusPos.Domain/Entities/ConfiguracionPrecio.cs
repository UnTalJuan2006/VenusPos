namespace VenusPos.Domain.Entities
{
    public class ConfiguracionPrecio
    {
        public int Id { get; set; }
        public string Clave { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
