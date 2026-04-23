using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Ventas
{
    public class VentaDTO
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public int IdReserva { get; set; }
        public string CodigoReserva { get; set; } = string.Empty;
        public int? IdCaja { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public List<VentaDetalleDTO> Detalles { get; set; } = new();
    }
}
