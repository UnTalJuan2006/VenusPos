using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Domain.Entities
{ 
        public class Venta
        {
            public int Id { get; set; }
            public int IdCaja { get; set; }
            public int IdReserva { get; set; }
            public int IdCliente { get; set; }
            public int IdEmpleado { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Descuento { get; set; }
            public decimal Total { get; set; }
            public string MetodoPago { get; set; }  
            public string Estado { get; set; }      
            public DateTime FechaVenta { get; set; }
            public Caja Caja { get; set; }
            public Reserva Reserva { get; set; }
            public Cliente Cliente { get; set; }
            public Empleado Empleado { get; set; }
            public ICollection<VentaDetalle> Detalles { get; set; }
        }
}
