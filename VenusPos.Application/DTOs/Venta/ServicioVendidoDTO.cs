namespace VenusPos.Application.DTOs.Ventas
{
    public class ServicioVendidoDTO
    {
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalIngresos { get; set; }
    }
}
