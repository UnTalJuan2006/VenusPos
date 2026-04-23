using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Caja
{
    public class CajaDTO
    {
        public int Id { get; set; }
        public string? NombreEmpleado { get; set; }
        public decimal MontoApertura { get; set; }
        public decimal MontoCierre { get; set; }
        public DateTime? FechaApertura{ get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal Faltante { get; set; }
        public decimal Sobrante { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalVentas { get; set; }
        
       
    }
}
