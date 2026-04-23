namespace VenusPos.Application.DTOs.Reserva
{
    public class CalculoPrecioDTO
    {
        public int IdMascota { get; set; }
        public List<int> IdsServicios { get; set; } = new();
        public decimal PrecioBase { get; set; }
        public decimal MultiplicadorTamaño { get; set; }
        public decimal MultiplicadorPelaje { get; set; }
        public decimal PrecioFinal { get; set; }
        public int DuracionMinutos { get; set; }
        public string DetalleCalculo { get; set; } = string.Empty;
    }
}
