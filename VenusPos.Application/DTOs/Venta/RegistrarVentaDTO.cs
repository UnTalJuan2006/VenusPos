using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Ventas
{
    public class RegistrarVentaDTO
    {
        public int IdReserva { get; set; }
        public int IdCliente { get; set; }
        public int IdEmpleado { get; set; }
        public string MetodoPago { get; set; } = string.Empty; 
        public decimal Descuento { get; set; }
    }
}
