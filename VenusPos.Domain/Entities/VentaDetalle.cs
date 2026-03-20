using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Domain.Entities
{
    public class VentaDetalle
    {
        public int Id { get; set; }
        public int IdVenta { get; set; }
        public int IdServicio { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public Venta Venta { get; set; }
        public Servicio Servicio { get; set; }
    }

}
