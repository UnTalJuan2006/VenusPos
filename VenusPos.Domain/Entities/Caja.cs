using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Domain.Entities
{
    public class Caja
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; }
        public decimal MontoApertura { get; set; }
        public decimal MontoCierre { get; set; }
        public decimal TotalVentas { get; set; }        
        public decimal TotalEfectivo { get; set; }      
        public decimal TotalTarjeta { get; set; }        
        public decimal TotalTransferencia { get; set; }  
        public decimal Faltante { get; set; }
        public decimal Sobrante { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public Empleado Empleado { get; set; }
        public ICollection<Venta> Ventas { get; set; }
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; }
    }

}

