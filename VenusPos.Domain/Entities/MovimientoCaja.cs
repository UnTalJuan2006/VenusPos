using System;
using System.Collections.Generic;
using System.Text;
using VenusPos.Domain.Enums.Mascota;

namespace VenusPos.Domain.Entities
{
    public class MovimientoCaja
    {
        public int Id { get; set; }
        public int IdCaja { get; set; }
        public Caja Caja { get; set; }
        public int IdEmpleado { get; set; }
        public Empleado Empleado { get; set; }
        public string Tipo { get; set; }
        public decimal Monto { get; set; }
        public string Concepto { get; set; }
        public string Descripcion   { get; set; }
        public DateTime FechaMovimiento { get; set; }

    }
}
