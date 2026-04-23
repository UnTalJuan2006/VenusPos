using System;
using System.Collections.Generic;
using System.Text;
using VenusPos.Application.DTOs.Ventas;
using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Repositories
{
    public interface IVentaRepository
    {
        Task<IEnumerable<VentaDTO>> ObtenerTodasAsync();
        Task<VentaDTO> ObtenerVentaPorIdAsync(int id);
        Task<IEnumerable<Venta>> ObtenerPorCajaAsync(int idCaja);
        Task<IEnumerable<Venta>> ObtenerPendientesAsync();
        Task<Venta?> ObtenerPorReservaAsync(int idReserva);
        Task<Venta?> ObtenerConDetallesAsync(int id);
        Task<VentaDTO> RegistrarVentaAsync(RegistrarVentaDTO venta);
        Task<VentaDTO> ConfirmarVentaAsync(int id);
        Task ActualizarVentaAsync(int id, ActualizarVentaDTO venta);
    }
}
