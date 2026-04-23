using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Ventas
{
    public class VentaDetalleDTO
    {
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
