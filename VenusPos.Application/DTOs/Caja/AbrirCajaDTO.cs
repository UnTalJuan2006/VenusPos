using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Caja
{
    public class AbrirCajaDTO
    {
        public int IdEmpleado { get; set; }
        public decimal MontoApertura { get; set; }
        public string? Observaciones { get; set; }

    }
}
