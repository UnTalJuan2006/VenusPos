using VenusPos.Application.DTOs.Caja;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;
using VenusPos.Domain.Entities;

namespace VenusPos.Application.Services.Caja
{
    public class CajaService : ICajaService
    {
        private readonly ICajaRepository _cajaRepository;

        public CajaService(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<IEnumerable<CajaDTO>> ObtenerTodosAsync()
        {
            var cajas = await _cajaRepository.ObtenerTodosAsync();
            return cajas.Select(c => MapearACajaDTO(c));
        }

        public async Task<IEnumerable<CajaDTO>> ObtenerHistorialAsync()
        {
            var cajas = await _cajaRepository.ObtenerHistorialAsync();
            return cajas.Select(c => MapearACajaDTO(c));
        }

        public async Task<CajaDTO?> ObtenerCajaAbiertaAsync()
        {
            var caja = await _cajaRepository.ObtenerCajaAbiertaAsync();
            return caja != null ? MapearACajaDTO(caja) : null;
        }

        public async Task<CajaDTO?> ObtenerCajaAbiertaPorEmpleadoAsync(int idEmpleado)
        {
            var caja = await _cajaRepository.ObtenerCajaAbiertaPorEmpleadoAsync(idEmpleado);
            return caja != null ? MapearACajaDTO(caja) : null;
        }

        public async Task<CajaDTO?> ObtenerPorIdAsync(int id)
        {
            var caja = await _cajaRepository.ObtenerPorIdAsync(id);
            return caja != null ? MapearACajaDTO(caja) : null;
        }

        public async Task<CajaDTO?> ObtenerPorEmpleadoAsync(int idEmpleado)
        {
            var caja = await _cajaRepository.ObtenerPorEmpleadoAsync(idEmpleado);
            return caja != null ? MapearACajaDTO(caja) : null;
        }

        public async Task<CajaDTO?> ObtenerPorFechaAsync(DateTime fecha)
        {
            var caja = await _cajaRepository.ObtenerPorFechaAsync(fecha);
            return caja != null ? MapearACajaDTO(caja) : null;
        }

        public async Task<CajaDTO> AbrirCajaAsync(AbrirCajaDTO dto)
        {
            var cajaExistente = await _cajaRepository.ObtenerCajaAbiertaPorEmpleadoAsync(dto.IdEmpleado);
            if (cajaExistente != null)
            {
                throw new InvalidOperationException("El empleado ya tiene una caja abierta.");
            }

            var caja = new Domain.Entities.Caja
            {
                IdEmpleado = dto.IdEmpleado,
                MontoApertura = dto.MontoApertura,
                MontoCierre = 0,
                TotalVentas = 0,
                TotalEfectivo = 0,
                TotalTarjeta = 0,
                TotalTransferencia = 0,
                Faltante = 0,
                Sobrante = 0,
                Estado = "Abierta",
                Observaciones = dto.Observaciones ?? string.Empty,
                FechaApertura = DateTime.Now,
                FechaCierre = null
            };

            var cajaCreada = await _cajaRepository.CrearAsync(caja);
            return MapearACajaDTO(cajaCreada);
        }

        public async Task<CajaDTO> CerrarCajaAsync(int id, CerrarCajaDTO dto)
        {
            var caja = await _cajaRepository.ObtenerConVentasYMovimientosAsync(id);
            if (caja == null)
            {
                throw new KeyNotFoundException("Caja no encontrada.");
            }

            if (caja.Estado == "Cerrada")
            {
                throw new InvalidOperationException("La caja ya está cerrada.");
            }

            // Inicializar colecciones si son null
            caja.Ventas ??= new List<Domain.Entities.Venta>();
            caja.MovimientosCaja ??= new List<Domain.Entities.MovimientoCaja>();

            // Recalcular totales manualmente sin guardar
            var ventasValidas = caja.Ventas.Where(v =>
                v.Estado == "Completada" || v.Estado == "Confirmada"
            ).ToList();

            caja.TotalVentas = ventasValidas.Sum(v => v.Total);
            caja.TotalEfectivo = ventasValidas
                .Where(v => v.MetodoPago != null && v.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase))
                .Sum(v => v.Total);
            caja.TotalTarjeta = ventasValidas
                .Where(v => v.MetodoPago != null && v.MetodoPago.Equals("Tarjeta", StringComparison.OrdinalIgnoreCase))
                .Sum(v => v.Total);
            caja.TotalTransferencia = ventasValidas
                .Where(v => v.MetodoPago != null && (
                    v.MetodoPago.Equals("Transferencia", StringComparison.OrdinalIgnoreCase) ||
                    v.MetodoPago.Equals("PSE", StringComparison.OrdinalIgnoreCase)
                ))
                .Sum(v => v.Total);

            // Agregar movimientos de caja
            var ingresos = caja.MovimientosCaja
                .Where(m => m.Tipo == "Ingreso")
                .Sum(m => m.Monto);
            var egresos = caja.MovimientosCaja
                .Where(m => m.Tipo == "Egreso")
                .Sum(m => m.Monto);
            caja.TotalEfectivo += ingresos - egresos;

            // Datos de cierre
            caja.MontoCierre = dto.MontoCierre;
            caja.Estado = "Cerrada";
            caja.FechaCierre = DateTime.Now;
            caja.Observaciones = dto.Observaciones ?? caja.Observaciones;

            // Calcular diferencias
            decimal totalEsperado = caja.MontoApertura + caja.TotalEfectivo;
            decimal diferencia = caja.MontoCierre - totalEsperado;

            if (diferencia > 0)
            {
                caja.Sobrante = diferencia;
                caja.Faltante = 0;
            }
            else if (diferencia < 0)
            {
                caja.Faltante = Math.Abs(diferencia);
                caja.Sobrante = 0;
            }
            else
            {
                caja.Faltante = 0;
                caja.Sobrante = 0;
            }

            // Guardar todos los cambios de una vez
            var cajaActualizada = await _cajaRepository.ActualizarAsync(caja);
            return MapearACajaDTO(cajaActualizada);
        }

        public async Task RecalcularTotalesAsync(int idCaja)
        {
            var caja = await _cajaRepository.ObtenerConVentasYMovimientosAsync(idCaja);
            if (caja == null)
            {
                throw new KeyNotFoundException("Caja no encontrada.");
            }

            // Solo contar ventas Completadas o Confirmadas, NO las Pendientes ni Anuladas
            var ventasValidas = caja.Ventas?.Where(v =>
                v.Estado == "Completada" || v.Estado == "Confirmada"
            ) ?? Enumerable.Empty<Domain.Entities.Venta>();

            caja.TotalVentas = ventasValidas.Sum(v => v.Total);
            caja.TotalEfectivo = ventasValidas.Where(v => v.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase)).Sum(v => v.Total);
            caja.TotalTarjeta = ventasValidas.Where(v => v.MetodoPago.Equals("Tarjeta", StringComparison.OrdinalIgnoreCase)).Sum(v => v.Total);
            caja.TotalTransferencia = ventasValidas.Where(v =>
                v.MetodoPago.Equals("Transferencia", StringComparison.OrdinalIgnoreCase) ||
                v.MetodoPago.Equals("PSE", StringComparison.OrdinalIgnoreCase)
            ).Sum(v => v.Total);

            // Agregar movimientos de caja (ingresos y egresos)
            if (caja.MovimientosCaja != null)
            {
                var ingresos = caja.MovimientosCaja.Where(m => m.Tipo == "Ingreso").Sum(m => m.Monto);
                var egresos = caja.MovimientosCaja.Where(m => m.Tipo == "Egreso").Sum(m => m.Monto);
                caja.TotalEfectivo += ingresos - egresos;
            }

            await _cajaRepository.ActualizarAsync(caja);
        }

        private CajaDTO MapearACajaDTO(Domain.Entities.Caja caja)
        {
            return new CajaDTO
            {
                Id = caja.Id,
                NombreEmpleado = caja.Empleado?.Nombre,
                MontoApertura = caja.MontoApertura,
                MontoCierre = caja.MontoCierre,
                FechaApertura = caja.FechaApertura,
                FechaCierre = caja.FechaCierre,
                Faltante = caja.Faltante,
                Sobrante = caja.Sobrante,
                Observaciones = caja.Observaciones,
                Estado = caja.Estado,
                TotalTarjeta = caja.TotalTarjeta,
                TotalEfectivo = caja.TotalEfectivo,
                TotalTransferencia = caja.TotalTransferencia,
                TotalVentas = caja.TotalVentas
            };
        }
    }
}
