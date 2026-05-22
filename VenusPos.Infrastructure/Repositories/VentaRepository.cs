using Microsoft.EntityFrameworkCore;
using VenusPos.Application.DTOs.Ventas;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Domain.Entities;
using VenusPos.Domain.Enums.Reserva;
using VenusPos.Infrastructure.Data;

namespace VenusPos.Infrastructure.Repositories
{
    public class VentaRepository : IVentaRepository
    {
        private readonly VenusPosDbContext _context;

        public VentaRepository(VenusPosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VentaDTO>> ObtenerTodasAsync()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.Reserva)
                .Include(v => v.Caja)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Servicio)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            return ventas.Select(v => MapToDTO(v));
        }

        public async Task<VentaDTO> ObtenerVentaPorIdAsync(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.Reserva)
                .Include(v => v.Caja)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Servicio)
                .FirstOrDefaultAsync(v => v.Id == id);

            return venta != null ? MapToDTO(venta) : null;
        }

        public async Task<IEnumerable<Venta>> ObtenerPorCajaAsync(int idCaja)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Servicio)
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Where(v => v.IdCaja == idCaja)
                .OrderBy(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> ObtenerPendientesAsync()
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Servicio)
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.Reserva)
                .Where(v => v.Estado == "Pendiente")
                .OrderBy(v => v.FechaVenta)
                .ToListAsync();
        }

        public async Task<Venta?> ObtenerPorReservaAsync(int idReserva)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Servicio)
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.Reserva)
                .FirstOrDefaultAsync(v => v.IdReserva == idReserva);
        }

        public async Task<Venta?> ObtenerConDetallesAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Servicio)
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.Reserva)
                .Include(v => v.Caja)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<VentaDTO> RegistrarVentaAsync(RegistrarVentaDTO dto)
        {
            // Obtener la reserva con sus servicios
            var reserva = await _context.Reservas
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .FirstOrDefaultAsync(r => r.Id == dto.IdReserva);

            if (reserva == null)
                throw new KeyNotFoundException("Reserva no encontrada");

            // Intentar obtener caja abierta del empleado o cualquier caja abierta del día
            var cajaAbierta = await _context.Caja
                .FirstOrDefaultAsync(c => c.IdEmpleado == dto.IdEmpleado && c.Estado == "Abierta");

            // Si no hay caja abierta del empleado específico, buscar cualquier caja abierta del día
            if (cajaAbierta == null)
            {
                cajaAbierta = await _context.Caja
                    .Where(c => c.Estado == "Abierta" && c.FechaApertura.Date == DateTime.Today)
                    .OrderByDescending(c => c.FechaApertura)
                    .FirstOrDefaultAsync();
            }

            // Si no hay caja abierta, permitir crear la venta sin caja asignada
            // Las ventas en efectivo quedarán pendientes hasta que se confirmen con caja abierta
            // Las ventas electrónicas (tarjeta, transferencia, PSE) se confirman automáticamente

            // Calcular totales
            decimal subtotal = reserva.PrecioTotal;
            decimal total = subtotal - dto.Descuento;

            // TODAS las ventas empiezan como "Pendiente"
            // No importa el método de pago, se deben confirmar explícitamente
            // Esto permite crear ventas sin caja abierta
            string estadoVenta = "Pendiente";

            // Crear venta
            var venta = new Venta
            {
                IdCaja = cajaAbierta?.Id,
                IdReserva = dto.IdReserva,
                IdCliente = dto.IdCliente,
                IdEmpleado = dto.IdEmpleado,
                Subtotal = subtotal,
                Descuento = dto.Descuento,
                Total = total,
                MetodoPago = dto.MetodoPago,
                Estado = estadoVenta,
                FechaVenta = DateTime.Now,
                Detalles = new List<VentaDetalle>()
            };

            // Crear detalles de venta a partir de los servicios de la reserva
            foreach (var rs in reserva.ReservaServicios)
            {
                venta.Detalles.Add(new VentaDetalle
                {
                    IdServicio = rs.IdServicio,
                    Cantidad = 1,
                    PrecioUnitario = rs.PrecioUnitario,
                    Subtotal = rs.PrecioUnitario
                });
            }

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            // NO actualizar caja ni marcar reserva como completada
            // Eso solo sucede cuando se confirma el pago explícitamente
            // La reserva permanece en estado "Confirmada" hasta que se confirme el pago

            // Retornar venta creada con todos los datos
            return await ObtenerVentaPorIdAsync(venta.Id);
        }

        public async Task<VentaDTO> ConfirmarVentaAsync(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Caja)
                .Include(v => v.Reserva)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                throw new KeyNotFoundException("Venta no encontrada");

            if (venta.Estado == "Confirmada")
                throw new InvalidOperationException("La venta ya está confirmada");

            if (venta.Estado == "Anulada")
                throw new InvalidOperationException("No se puede confirmar una venta anulada");

            // Si la venta no tiene caja asignada, buscar una caja abierta
            if (!venta.IdCaja.HasValue)
            {
                // Buscar caja abierta del empleado
                var cajaAbierta = await _context.Caja
                    .FirstOrDefaultAsync(c => c.IdEmpleado == venta.IdEmpleado && c.Estado == "Abierta");

                // Si no hay caja del empleado, buscar cualquier caja abierta del día
                if (cajaAbierta == null)
                {
                    cajaAbierta = await _context.Caja
                        .Where(c => c.Estado == "Abierta" && c.FechaApertura.Date == DateTime.Today)
                        .OrderByDescending(c => c.FechaApertura)
                        .FirstOrDefaultAsync();
                }

                // Para pagos en efectivo, es obligatorio tener una caja abierta
                // Para pagos electrónicos, se puede confirmar sin caja (el dinero va directo al banco)
                if (cajaAbierta == null && venta.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("No hay una caja abierta. Para confirmar pagos en efectivo debe haber una caja abierta.");
                }

                // Asignar la caja si existe
                if (cajaAbierta != null)
                {
                    venta.IdCaja = cajaAbierta.Id;
                }
            }

            // Cambiar estado a Confirmada
            venta.Estado = "Confirmada";

            // Actualizar totales de la caja solo si hay caja asignada
            if (venta.IdCaja.HasValue)
            {
                await ActualizarTotalesCajaAsync(venta.IdCaja.Value, venta);
            }

            // Marcar la reserva como Completada cuando se confirma el pago
            if (venta.Reserva != null)
            {
                venta.Reserva.Estado = EnumEstado.Completada;
                venta.Reserva.FechaActualizacion = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await ObtenerVentaPorIdAsync(id);
        }

        private async Task ActualizarTotalesCajaAsync(int idCaja, Venta venta)
        {
            var caja = await _context.Caja.FindAsync(idCaja);

            if (caja == null)
                throw new KeyNotFoundException("Caja no encontrada");

            // Sumar al total de ventas
            caja.TotalVentas += venta.Total;

            // Sumar al total según método de pago
            if (venta.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase))
            {
                caja.TotalEfectivo += venta.Total;
            }
            else if (venta.MetodoPago.Equals("Tarjeta", StringComparison.OrdinalIgnoreCase))
            {
                caja.TotalTarjeta += venta.Total;
            }
            else if (venta.MetodoPago.Equals("Transferencia", StringComparison.OrdinalIgnoreCase) ||
                     venta.MetodoPago.Equals("PSE", StringComparison.OrdinalIgnoreCase))
            {
                caja.TotalTransferencia += venta.Total;
            }

            _context.Caja.Update(caja);
        }

        public async Task ActualizarVentaAsync(int id, ActualizarVentaDTO dto)
        {
            var venta = await _context.Ventas.FindAsync(id);

            if (venta == null)
                throw new KeyNotFoundException("Venta no encontrada");

            if (!string.IsNullOrEmpty(dto.Estado))
                venta.Estado = dto.Estado;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ServicioVendidoDTO>> ObtenerServiciosMasVendidosAsync(int top)
        {
            var serviciosVendidos = await _context.VentaDetalles
                .Include(vd => vd.Servicio)
                .Include(vd => vd.Venta)
                .Where(vd => vd.Venta.Estado == "Confirmada") // Solo ventas confirmadas
                .GroupBy(vd => new { vd.IdServicio, vd.Servicio.Nombre })
                .Select(g => new ServicioVendidoDTO
                {
                    IdServicio = g.Key.IdServicio,
                    NombreServicio = g.Key.Nombre ?? "Servicio",
                    CantidadVendida = g.Sum(vd => vd.Cantidad),
                    TotalIngresos = g.Sum(vd => vd.Subtotal)
                })
                .OrderByDescending(s => s.CantidadVendida)
                .Take(top)
                .ToListAsync();

            return serviciosVendidos;
        }

        private VentaDTO MapToDTO(Venta venta)
        {
            return new VentaDTO
            {
                Id = venta.Id,
                IdEmpleado = venta.IdEmpleado,
                NombreEmpleado = venta.Empleado?.Nombre ?? "",
                IdReserva = venta.IdReserva,
                CodigoReserva = venta.Reserva?.CodigoReserva ?? "",
                IdCaja = venta.IdCaja,
                Subtotal = venta.Subtotal,
                Descuento = venta.Descuento,
                Total = venta.Total,
                MetodoPago = venta.MetodoPago,
                Estado = venta.Estado,
                FechaVenta = venta.FechaVenta,
                Detalles = venta.Detalles?.Select(d => new VentaDetalleDTO
                {
                    IdServicio = d.IdServicio,
                    NombreServicio = d.Servicio?.Nombre ?? "",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList() ?? new List<VentaDetalleDTO>()
            };
        }
    }
}
