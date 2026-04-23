namespace VenusPos.Application.DTOs.ConfiguracionPrecio
{
    public class ConfiguracionPrecioDTO
    {
        public int Id { get; set; }
        public string Clave { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
