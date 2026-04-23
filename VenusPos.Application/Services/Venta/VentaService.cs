using VenusPos.Application.DTOs.Ventas;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;

namespace VenusPos.Application.Services.Venta
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventaRepository;

        public VentaService(IVentaRepository ventaRepository)
        {
            _ventaRepository = ventaRepository;
        }

        public async Task<IEnumerable<VentaDTO>> ObtenerTodasAsync()
        {
            return await _ventaRepository.ObtenerTodasAsync();
        }

        public async Task<VentaDTO?> ObtenerPorIdAsync(int id)
        {
            return await _ventaRepository.ObtenerVentaPorIdAsync(id);
        }

        public async Task<IEnumerable<VentaDTO>> ObtenerPorCajaAsync(int idCaja)
        {
            var ventas = await _ventaRepository.ObtenerPorCajaAsync(idCaja);
            return ventas.Select(v => MapToDTO(v));
        }

        public async Task<IEnumerable<VentaDTO>> ObtenerPendientesAsync()
        {
            var ventas = await _ventaRepository.ObtenerPendientesAsync();
            return ventas.Select(v => MapToDTO(v));
        }

        public async Task<VentaDTO?> ObtenerPorReservaAsync(int idReserva)
        {
            var venta = await _ventaRepository.ObtenerPorReservaAsync(idReserva);
            return venta != null ? MapToDTO(venta) : null;
        }

        public async Task<VentaDTO> RegistrarVentaAsync(RegistrarVentaDTO dto)
        {
            // Validaciones
            if (dto.IdReserva <= 0)
                throw new ArgumentException("IdReserva es requerido");

            if (dto.IdCliente <= 0)
                throw new ArgumentException("IdCliente es requerido");

            if (dto.IdEmpleado <= 0)
                throw new ArgumentException("IdEmpleado es requerido");

            if (string.IsNullOrEmpty(dto.MetodoPago))
                throw new ArgumentException("MetodoPago es requerido");

            // Validar método de pago
            var metodosValidos = new[] { "Efectivo", "Tarjeta", "Transferencia", "PSE" };
            if (!metodosValidos.Contains(dto.MetodoPago, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Método de pago no válido. Valores permitidos: {string.Join(", ", metodosValidos)}");

            if (dto.Descuento < 0)
                throw new ArgumentException("El descuento no puede ser negativo");

            // Verificar que no exista ya una venta para esta reserva
            var ventaExistente = await _ventaRepository.ObtenerPorReservaAsync(dto.IdReserva);
            if (ventaExistente != null)
                throw new InvalidOperationException("Ya existe una venta para esta reserva");

            return await _ventaRepository.RegistrarVentaAsync(dto);
        }

        public async Task<VentaDTO> ConfirmarVentaAsync(int id)
        {
            return await _ventaRepository.ConfirmarVentaAsync(id);
        }

        public async Task<VentaDTO> ActualizarEstadoAsync(int id, ActualizarVentaDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Estado))
                throw new ArgumentException("El estado es requerido");

            var estadosValidos = new[] { "Pendiente", "Completada", "Anulada" };
            if (!estadosValidos.Contains(dto.Estado))
                throw new ArgumentException($"Estado no válido. Valores permitidos: {string.Join(", ", estadosValidos)}");

            await _ventaRepository.ActualizarVentaAsync(id, dto);

            return await _ventaRepository.ObtenerVentaPorIdAsync(id);
        }

        public async Task<bool> AnularVentaAsync(int id)
        {
            var venta = await _ventaRepository.ObtenerConDetallesAsync(id);

            if (venta == null)
                throw new KeyNotFoundException("Venta no encontrada");

            if (venta.Estado == "Anulada")
                throw new InvalidOperationException("La venta ya está anulada");

            await _ventaRepository.ActualizarVentaAsync(id, new ActualizarVentaDTO { Estado = "Anulada" });

            return true;
        }

        private VentaDTO MapToDTO(Domain.Entities.Venta venta)
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
