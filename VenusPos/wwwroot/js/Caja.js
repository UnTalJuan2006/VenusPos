const API = '';
let cajaActual = null;
let empleadoActual = null;
let ventasPendientes = [];
let ventasConfirmadas = [];
let todasCajas = [];
let todasVentas = [];
let empleados = [];

// Paginación
const PER_PAGE = 10;
let pagActualCajas = 1;
let pagActualVentas = 1;
let cajasFiltradas = [];
let ventasFiltradas = [];

// ══════════════════════════════════════════════════════
// INICIALIZACIÓN
// ══════════════════════════════════════════════════════

// Protección de ruta
function check() {
    if (!localStorage.getItem('token') || localStorage.getItem('rol') !== 'admin')
        window.location.replace('/Admin/Login.html');
}
check();
window.addEventListener('pageshow', check);

// Fecha
const DIAS = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
const MESES = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
const hoy = new Date();
document.getElementById('fechaHoy').textContent =
    DIAS[hoy.getDay()] + ', ' + hoy.getDate() + ' de ' + MESES[hoy.getMonth()] + ' de ' + hoy.getFullYear();

// JWT
let uName = 'Admin', uEmail = '', uRole = 'Administrador', uIni = 'AD', uId = null;
try {
    var tok = localStorage.getItem('token');
    if (tok) {
        var p = JSON.parse(atob(tok.split('.')[1]));
        uName = p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || uName;
        uEmail = p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || uEmail;
        uRole = p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || uRole;
        uId = p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || null;
        uIni = uName.trim().split(' ').map(x => x[0]).join('').substring(0, 2).toUpperCase();
    }
} catch (e) { }

document.getElementById('tbAva').textContent = uIni;
document.getElementById('tbName').textContent = uName;
document.getElementById('tbRole').textContent = uRole;
document.getElementById('ddName').textContent = uName;
document.getElementById('ddEmail').textContent = uEmail;

function toggleSidebar() { document.getElementById('sidebar').classList.toggle('collapsed'); }

function toggleDrop() {
    var d = document.getElementById('tbDrop');
    var p = document.getElementById('tbProfile');
    var o = d.classList.toggle('open');
    p.classList.toggle('open', o);
}
document.addEventListener('click', function (e) {
    var w = document.querySelector('.tb-profile-wrap');
    if (w && !w.contains(e.target)) {
        document.getElementById('tbDrop').classList.remove('open');
        document.getElementById('tbProfile').classList.remove('open');
    }
});

var TK = 'venus_theme';
var dark = localStorage.getItem(TK) === 'dark';
function applyTheme(d) {
    document.documentElement.setAttribute('data-theme', d ? 'dark' : 'light');
    document.getElementById('iconSun').style.display = d ? 'none' : 'block';
    document.getElementById('iconMoon').style.display = d ? 'block' : 'none';
    localStorage.setItem(TK, d ? 'dark' : 'light');
}
function toggleTheme() { dark = !dark; applyTheme(dark); }
applyTheme(dark);

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('rol');
    window.location.replace('/Admin/Login.html');
}

function authHeaders() {
    return { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + localStorage.getItem('token') };
}

function toast(msg, tipo) {
    tipo = tipo || 'success';
    var el = document.getElementById('toast');
    el.textContent = msg;
    el.className = 'toast ' + tipo;
    el.style.display = 'block';
    clearTimeout(el._t);
    el._t = setTimeout(() => { el.style.display = 'none'; }, 3200);
}

// ══════════════════════════════════════════════════════
// CARGAR DATOS
// ══════════════════════════════════════════════════════

async function cargarDatos() {
    try {
        // Cargar todos los datos en paralelo para mejorar el rendimiento
        const [empleadosRes, cajaRes, ventasPendRes, historialRes, todasVentasRes] = await Promise.all([
            fetch(API + '/api/Empleado', { headers: authHeaders() }).catch(() => null),
            fetch(API + '/api/Caja/abierta', { headers: authHeaders() }).catch(() => null),
            fetch(API + '/api/Venta/pendientes', { headers: authHeaders() }).catch(() => null),
            fetch(API + '/api/Caja/historial', { headers: authHeaders() }).catch(() => null),
            fetch(API + '/api/Venta', { headers: authHeaders() }).catch(() => null)
        ]);

        // Procesar empleados
        if (empleadosRes && empleadosRes.ok) {
            empleados = await empleadosRes.json();
            const optionsHTML = empleados.map(e =>
                `<option value="${e.id}">${e.nombre}</option>`
            ).join('');
            const selectCaja = document.getElementById('filtroCajaEmpleado');
            if (selectCaja) {
                selectCaja.innerHTML = '<option value="">Todos los empleados</option>' + optionsHTML;
            }
        }

        // Procesar caja abierta
        if (cajaRes && cajaRes.ok) {
            cajaActual = await cajaRes.json();
            mostrarCajaAbierta();

            // Cargar ventas confirmadas de la caja actual
            const ventasConfRes = await fetch(API + '/api/Venta/caja/' + cajaActual.id, { headers: authHeaders() });
            if (ventasConfRes.ok) {
                const ventas = await ventasConfRes.json();
                ventasConfirmadas = ventas.filter(v => v.estado === 'Confirmada');
                renderVentasConfirmadas();
            }
        } else {
            mostrarCajaCerrada();
            ventasConfirmadas = [];
            renderVentasConfirmadas();
        }

        // Procesar ventas pendientes
        if (ventasPendRes && ventasPendRes.ok) {
            ventasPendientes = await ventasPendRes.json();
            renderVentasPendientes();
        }

        // Procesar historial de cajas
        if (historialRes && historialRes.ok) {
            todasCajas = await historialRes.json();
            cajasFiltradas = todasCajas.slice();
            pagActualCajas = 1;
            renderHistorialCajas(cajasFiltradas);
            renderPaginacionCajas();
            actualizarKPIsCajas();
        }

        // Procesar todas las ventas
        if (todasVentasRes && todasVentasRes.ok) {
            todasVentas = await todasVentasRes.json();
            ventasFiltradas = todasVentas.slice();
            pagActualVentas = 1;
            renderTodasVentas(ventasFiltradas);
            renderPaginacionVentas();
            actualizarKPIsVentas();
        }

    } catch (err) {
        console.error('Error al cargar datos:', err);
        toast('Error al cargar los datos de caja', 'error');
    }
}

function mostrarCajaAbierta() {
    // Actualizar estado
    const pill = document.getElementById('pillEstado');
    pill.classList.add('abierta');
    pill.classList.remove('cerrada');
    document.getElementById('estadoCaja').textContent = 'Caja Abierta';

    // Actualizar empleado
    document.getElementById('empleadoCaja').textContent = 'Caja de ' + (cajaActual.nombreEmpleado || 'Empleado');

    // Mostrar/Ocultar botones
    document.getElementById('btnAbrirCaja').style.display = 'none';
    document.getElementById('btnCerrarCaja').style.display = 'inline-flex';

    // Habilitar botón Registrar Venta
    const btnRegistrarVenta = document.querySelector('button[onclick="abrirModalRegistrarVenta()"]');
    if (btnRegistrarVenta) {
        btnRegistrarVenta.disabled = false;
        btnRegistrarVenta.style.opacity = '1';
        btnRegistrarVenta.style.cursor = 'pointer';
    }

    // Actualizar KPIs
    actualizarKPIs();
}

function mostrarCajaCerrada() {
    // Actualizar estado
    const pill = document.getElementById('pillEstado');
    pill.classList.remove('abierta');
    pill.classList.add('cerrada');
    document.getElementById('estadoCaja').textContent = 'Caja Cerrada';

    // Actualizar empleado
    document.getElementById('empleadoCaja').textContent = 'No hay caja abierta';

    // Mostrar/Ocultar botones
    document.getElementById('btnAbrirCaja').style.display = 'inline-flex';
    document.getElementById('btnCerrarCaja').style.display = 'none';

    // Deshabilitar botón Registrar Venta
    const btnRegistrarVenta = document.querySelector('button[onclick="abrirModalRegistrarVenta()"]');
    if (btnRegistrarVenta) {
        btnRegistrarVenta.disabled = true;
        btnRegistrarVenta.style.opacity = '0.5';
        btnRegistrarVenta.style.cursor = 'not-allowed';
        btnRegistrarVenta.title = 'Debe abrir una caja primero';
    }

    // Limpiar KPIs
    document.getElementById('totalVentas').textContent = '$0';
    document.getElementById('totalEfectivo').textContent = '$0';
    document.getElementById('totalTarjeta').textContent = '$0';
    document.getElementById('totalTransferencia').textContent = '$0';

    // NO ocultar ventas pendientes - pueden existir reservas sin caja asignada
    // La tabla de confirmadas sí se limpia porque requiere caja abierta
    document.getElementById('ventasTableBody').innerHTML = '<tr><td colspan="6" style="text-align:center;padding:40px;color:var(--muted)">No hay ventas confirmadas</td></tr>';
}

function actualizarKPIs() {
    if (!cajaActual) return;

    document.getElementById('totalVentas').textContent = '$' + (cajaActual.totalVentas || 0).toLocaleString('es-CO');
    document.getElementById('totalEfectivo').textContent = '$' + (cajaActual.totalEfectivo || 0).toLocaleString('es-CO');
    document.getElementById('totalTarjeta').textContent = '$' + (cajaActual.totalTarjeta || 0).toLocaleString('es-CO');
    document.getElementById('totalTransferencia').textContent = '$' + (cajaActual.totalTransferencia || 0).toLocaleString('es-CO');
}

// ══════════════════════════════════════════════════════
// VENTAS
// ══════════════════════════════════════════════════════

// Esta función ya no es necesaria, se integró en cargarDatos() para mejor rendimiento

function renderVentasPendientes() {
    const section = document.getElementById('ventasPendientesSection');
    const lista = document.getElementById('ventasPendientesList');
    const count = document.getElementById('countPendientes');

    count.textContent = ventasPendientes.length;

    if (ventasPendientes.length === 0) {
        section.style.display = 'none';
        return;
    }

    section.style.display = 'block';

    lista.innerHTML = ventasPendientes.map(v => {
        const servicios = v.detalles && v.detalles.length > 0
            ? v.detalles.map(d => d.nombreServicio).join(', ')
            : 'Sin servicios';

        // Deshabilitar botón si no hay caja abierta
        const btnDisabled = !cajaActual ? 'disabled' : '';
        const btnStyle = !cajaActual ? 'opacity:0.5; cursor:not-allowed;' : '';
        const btnTitle = !cajaActual ? 'title="Debe abrir una caja primero"' : '';

        return `<div class="venta-item">
            <div class="venta-info">
                <div class="venta-icon">
                    <svg width="20" height="20" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                </div>
                <div class="venta-details">
                    <h4>Reserva ${v.codigoReserva || '#' + v.idReserva}</h4>
                    <p>${servicios} • ${formatFecha(v.fechaVenta)}</p>
                </div>
            </div>
            <div class="venta-monto">$${v.total.toLocaleString('es-CO')}</div>
            <button class="btn-confirmar" onclick="confirmarVenta(${v.id})" ${btnDisabled} style="${btnStyle}" ${btnTitle}>
                <svg width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                    <path d="M5 13l4 4L19 7" />
                </svg>
                Confirmar Pago
            </button>
        </div>`;
    }).join('');
}

function renderVentasConfirmadas() {
    const tbody = document.getElementById('ventasTableBody');
    const count = document.getElementById('countVentas');

    count.textContent = ventasConfirmadas.length;

    if (ventasConfirmadas.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:40px;color:var(--muted)">No hay ventas confirmadas</td></tr>';
        return;
    }

    tbody.innerHTML = ventasConfirmadas.map(v => {
        const servicios = v.detalles && v.detalles.length > 0
            ? v.detalles.map(d => d.nombreServicio).join(', ')
            : 'Sin servicios';

        const metodoPagoClass = v.metodoPago.toLowerCase();

        return `<tr>
            <td><span class="badge-secondary">${v.codigoReserva || '#' + v.idReserva}</span></td>
            <td>${formatHora(v.fechaVenta)}</td>
            <td>${servicios}</td>
            <td><span class="metodo-pago ${metodoPagoClass}">${v.metodoPago}</span></td>
            <td><strong>$${v.total.toLocaleString('es-CO')}</strong></td>
            <td><span class="estado-venta confirmada">Confirmada</span></td>
        </tr>`;
    }).join('');
}

async function confirmarVenta(id) {
    // Validar que haya caja abierta
    if (!cajaActual) {
        toast('No se puede confirmar la venta sin una caja abierta', 'error');
        return;
    }

    if (!confirm('¿Confirmar el pago en efectivo de esta venta?')) return;

    try {
        const res = await fetch(API + '/api/Venta/' + id + '/confirmar', {
            method: 'POST',
            headers: authHeaders()
        });

        if (res.ok) {
            toast('Venta confirmada exitosamente', 'success');
            await cargarDatos(); // Recargar todo para actualizar caja y ventas
        } else {
            const error = await res.json();
            toast(error.message || 'Error al confirmar la venta', 'error');
        }
    } catch (err) {
        console.error('Error al confirmar venta:', err);
        toast('Error al confirmar la venta', 'error');
    }
}

// ══════════════════════════════════════════════════════
// ABRIR CAJA
// ══════════════════════════════════════════════════════

function abrirModalAbrirCaja() {
    document.getElementById('formAbrirCaja').reset();
    document.getElementById('modalAbrirCaja').style.display = 'flex';
}

async function abrirCaja(event) {
    event.preventDefault();

    if (!uId) {
        toast('No se pudo identificar al empleado', 'error');
        return;
    }

    const datos = {
        idEmpleado: parseInt(uId),
        montoApertura: parseFloat(document.getElementById('montoApertura').value),
        observaciones: document.getElementById('observacionesApertura').value
    };

    try {
        const res = await fetch(API + '/api/Caja/abrir', {
            method: 'POST',
            headers: authHeaders(),
            body: JSON.stringify(datos)
        });

        if (res.ok) {
            toast('Caja abierta exitosamente', 'success');
            cerrarModal(null, 'modalAbrirCaja');
            await cargarDatos();
        } else {
            const error = await res.json();
            toast(error.message || 'Error al abrir la caja', 'error');
        }
    } catch (err) {
        console.error('Error al abrir caja:', err);
        toast('Error al abrir la caja', 'error');
    }
}

// ══════════════════════════════════════════════════════
// CERRAR CAJA
// ══════════════════════════════════════════════════════

function abrirModalCerrarCaja() {
    if (!cajaActual) return;

    // Limpiar formulario
    document.getElementById('formCerrarCaja').reset();
    document.getElementById('diferencia').style.display = 'none';

    // Mostrar resumen
    document.getElementById('resumenApertura').textContent = '$' + (cajaActual.montoApertura || 0).toLocaleString('es-CO');
    document.getElementById('resumenEfectivo').textContent = '$' + (cajaActual.totalEfectivo || 0).toLocaleString('es-CO');

    const montoEsperado = (cajaActual.montoApertura || 0) + (cajaActual.totalEfectivo || 0);
    document.getElementById('resumenEsperado').textContent = '$' + montoEsperado.toLocaleString('es-CO');

    document.getElementById('modalCerrarCaja').style.display = 'flex';
}

function calcularDiferencia() {
    const montoCierre = parseFloat(document.getElementById('montoCierre').value) || 0;
    const montoEsperado = (cajaActual.montoApertura || 0) + (cajaActual.totalEfectivo || 0);
    const diferencia = montoCierre - montoEsperado;

    const divDiferencia = document.getElementById('diferencia');
    divDiferencia.style.display = 'block';

    // Limpiar clases anteriores
    divDiferencia.classList.remove('faltante', 'sobrante', 'exacto');

    if (diferencia > 0) {
        divDiferencia.classList.add('sobrante');
        document.getElementById('diferenciaLabel').textContent = 'Sobrante';
        document.getElementById('diferenciaValor').textContent = '+$' + diferencia.toLocaleString('es-CO');
    } else if (diferencia < 0) {
        divDiferencia.classList.add('faltante');
        document.getElementById('diferenciaLabel').textContent = 'Faltante';
        document.getElementById('diferenciaValor').textContent = '-$' + Math.abs(diferencia).toLocaleString('es-CO');
    } else {
        divDiferencia.classList.add('exacto');
        document.getElementById('diferenciaLabel').textContent = 'Exacto';
        document.getElementById('diferenciaValor').textContent = '$0';
    }
}

async function cerrarCaja(event) {
    event.preventDefault();

    if (!cajaActual) return;

    const montoCierre = parseFloat(document.getElementById('montoCierre').value);
    const montoEsperado = (cajaActual.montoApertura || 0) + (cajaActual.totalEfectivo || 0);
    const diferencia = montoCierre - montoEsperado;

    const datos = {
        montoCierre: montoCierre,
        diferencia: diferencia,
        observaciones: document.getElementById('observacionesCierre').value
    };

    try {
        const res = await fetch(API + '/api/Caja/' + cajaActual.id + '/cerrar', {
            method: 'PUT',
            headers: authHeaders(),
            body: JSON.stringify(datos)
        });

        if (res.ok) {
            toast('Caja cerrada exitosamente', 'success');
            cerrarModal(null, 'modalCerrarCaja');
            cajaActual = null;
            mostrarCajaCerrada();
        } else {
            const error = await res.json();
            toast(error.message || 'Error al cerrar la caja', 'error');
        }
    } catch (err) {
        console.error('Error al cerrar caja:', err);
        toast('Error al cerrar la caja', 'error');
    }
}

// ══════════════════════════════════════════════════════
// HISTORIAL DE CAJAS
// ══════════════════════════════════════════════════════

// Esta función ya no es necesaria, se integró en cargarDatos() para mejor rendimiento

function actualizarKPIsCajas() {
    // Total de cajas registradas
    document.getElementById('statTotalCajas').textContent = todasCajas.length;
}

function renderHistorialCajas(cajas) {
    cajasFiltradas = cajas;
    const tbody = document.getElementById('cajasTableBody');
    const count = document.getElementById('countCajas');

    count.textContent = cajas.length;

    if (cajas.length === 0) {
        tbody.innerHTML = '<tr><td colspan="13" style="text-align:center;padding:40px;color:var(--muted)">No hay cajas registradas</td></tr>';
        return;
    }

    // Paginación
    const paginaActualCajas = cajasFiltradas.slice((pagActualCajas - 1) * PER_PAGE, pagActualCajas * PER_PAGE);

    tbody.innerHTML = paginaActualCajas.map(c => {
        const estadoClass = c.estado === 'Abierta' ? 'confirmada' : 'completada';
        const fechaApertura = formatFecha(c.fechaApertura) + ' ' + formatHora(c.fechaApertura);
        const fechaCierre = c.fechaCierre ? formatFecha(c.fechaCierre) + ' ' + formatHora(c.fechaCierre) : '-';
        const montoCierre = c.montoCierre ? '$' + c.montoCierre.toLocaleString('es-CO') : '-';

        const faltante = c.faltante && c.faltante > 0 ? `<span style="color:#EF4444">$${c.faltante.toLocaleString('es-CO')}</span>` : '-';
        const sobrante = c.sobrante && c.sobrante > 0 ? `<span style="color:#10B981">$${c.sobrante.toLocaleString('es-CO')}</span>` : '-';
        const observaciones = c.observaciones ? `<span style="font-size:12px;color:var(--muted)">${c.observaciones}</span>` : '-';

        return `<tr>
            <td>${c.id}</td>
            <td>${c.nombreEmpleado || 'Sin empleado'}</td>
            <td>${fechaApertura}</td>
            <td>${fechaCierre}</td>
            <td>$${(c.montoApertura || 0).toLocaleString('es-CO')}</td>
            <td>$${(c.totalEfectivo || 0).toLocaleString('es-CO')}</td>
            <td>$${(c.totalTarjeta || 0).toLocaleString('es-CO')}</td>
            <td>$${(c.totalTransferencia || 0).toLocaleString('es-CO')}</td>
            <td>${montoCierre}</td>
            <td>${faltante}</td>
            <td>${sobrante}</td>
            <td>${observaciones}</td>
            <td><span class="estado-venta ${estadoClass}">${c.estado}</span></td>
        </tr>`;
    }).join('');

    renderPaginacionCajas();
}

function totalPagsCajas() {
    return Math.max(1, Math.ceil(cajasFiltradas.length / PER_PAGE));
}

function renderPaginacionCajas() {
    const total = totalPagsCajas();
    const pag = document.getElementById('paginationCajas');
    const info = document.getElementById('pagInfoCajas');
    const btns = document.getElementById('pagBtnsCajas');

    if (cajasFiltradas.length <= PER_PAGE) {
        pag.style.display = 'none';
        return;
    }

    pag.style.display = 'flex';
    const desde = (pagActualCajas - 1) * PER_PAGE + 1;
    const hasta = Math.min(pagActualCajas * PER_PAGE, cajasFiltradas.length);
    info.textContent = 'Mostrando ' + desde + ' - ' + hasta + ' de ' + cajasFiltradas.length + ' cajas';

    let html = '<button class="pag-btn" onclick="irPagCajas(' + (pagActualCajas - 1) + ')" ' + (pagActualCajas === 1 ? 'disabled' : '') + '>&lsaquo;</button>';

    for (let i = 1; i <= total; i++) {
        if (total > 7 && i > 2 && i < total - 1 && Math.abs(i - pagActualCajas) > 1) {
            if (i === 3 || i === total - 2) html += '<button class="pag-btn" disabled>&hellip;</button>';
            continue;
        }
        html += '<button class="pag-btn ' + (i === pagActualCajas ? 'active' : '') + '" onclick="irPagCajas(' + i + ')">' + i + '</button>';
    }

    html += '<button class="pag-btn" onclick="irPagCajas(' + (pagActualCajas + 1) + ')" ' + (pagActualCajas === total ? 'disabled' : '') + '>&rsaquo;</button>';
    btns.innerHTML = html;
}

function irPagCajas(n) {
    pagActualCajas = Math.max(1, Math.min(n, totalPagsCajas()));
    renderHistorialCajas(cajasFiltradas);
}

// ══════════════════════════════════════════════════════
// TODAS LAS VENTAS
// ══════════════════════════════════════════════════════

// Esta función ya no es necesaria, se integró en cargarDatos() para mejor rendimiento

function actualizarKPIsVentas() {
    // Total de ventas registradas
    document.getElementById('statTotalVentas').textContent = todasVentas.length;
}

function renderTodasVentas(ventas) {
    ventasFiltradas = ventas;
    const tbody = document.getElementById('todasVentasTableBody');
    const count = document.getElementById('countTodasVentas');

    count.textContent = ventas.length;

    if (ventas.length === 0) {
        tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;padding:40px;color:var(--muted)">No hay ventas registradas</td></tr>';
        return;
    }

    // Paginación
    const paginaActualVentas = ventasFiltradas.slice((pagActualVentas - 1) * PER_PAGE, pagActualVentas * PER_PAGE);

    tbody.innerHTML = paginaActualVentas.map(v => {
        const servicios = v.detalles && v.detalles.length > 0
            ? v.detalles.map(d => d.nombreServicio).join(', ')
            : 'Sin servicios';

        const metodoPagoClass = v.metodoPago.toLowerCase();
        const estadoClass = v.estado === 'Confirmada' ? 'confirmada' : v.estado === 'Pendiente' ? 'pendiente' : 'cancelada';
        const fechaVenta = formatFecha(v.fechaVenta) + ' ' + formatHora(v.fechaVenta);

        return `<tr>
            <td>${v.id}</td>
            <td><span class="badge-secondary">${v.codigoReserva || '#' + v.idReserva}</span></td>
            <td>${fechaVenta}</td>
            <td>${v.nombreEmpleado || 'Sin empleado'}</td>
            <td>${servicios}</td>
            <td><span class="metodo-pago ${metodoPagoClass}">${v.metodoPago}</span></td>
            <td><strong>$${v.total.toLocaleString('es-CO')}</strong></td>
            <td><span class="estado-venta ${estadoClass}">${v.estado}</span></td>
        </tr>`;
    }).join('');

    renderPaginacionVentas();
}

function totalPagsVentas() {
    return Math.max(1, Math.ceil(ventasFiltradas.length / PER_PAGE));
}

function renderPaginacionVentas() {
    const total = totalPagsVentas();
    const pag = document.getElementById('paginationVentas');
    const info = document.getElementById('pagInfoVentas');
    const btns = document.getElementById('pagBtnsVentas');

    if (ventasFiltradas.length <= PER_PAGE) {
        pag.style.display = 'none';
        return;
    }

    pag.style.display = 'flex';
    const desde = (pagActualVentas - 1) * PER_PAGE + 1;
    const hasta = Math.min(pagActualVentas * PER_PAGE, ventasFiltradas.length);
    info.textContent = 'Mostrando ' + desde + ' - ' + hasta + ' de ' + ventasFiltradas.length + ' ventas';

    let html = '<button class="pag-btn" onclick="irPagVentas(' + (pagActualVentas - 1) + ')" ' + (pagActualVentas === 1 ? 'disabled' : '') + '>&lsaquo;</button>';

    for (let i = 1; i <= total; i++) {
        if (total > 7 && i > 2 && i < total - 1 && Math.abs(i - pagActualVentas) > 1) {
            if (i === 3 || i === total - 2) html += '<button class="pag-btn" disabled>&hellip;</button>';
            continue;
        }
        html += '<button class="pag-btn ' + (i === pagActualVentas ? 'active' : '') + '" onclick="irPagVentas(' + i + ')">' + i + '</button>';
    }

    html += '<button class="pag-btn" onclick="irPagVentas(' + (pagActualVentas + 1) + ')" ' + (pagActualVentas === total ? 'disabled' : '') + '>&rsaquo;</button>';
    btns.innerHTML = html;
}

function irPagVentas(n) {
    pagActualVentas = Math.max(1, Math.min(n, totalPagsVentas()));
    renderTodasVentas(ventasFiltradas);
}

// ══════════════════════════════════════════════════════
// EMPLEADOS
// ══════════════════════════════════════════════════════

// Esta función ya no es necesaria, se integró en cargarDatos() para mejor rendimiento

// ══════════════════════════════════════════════════════
// FILTROS - CAJAS
// ══════════════════════════════════════════════════════

function aplicarFiltrosCajas() {
    const idEmpleado = document.getElementById('filtroCajaEmpleado').value;

    let cajasFiltradas_temp = [...todasCajas];

    // Filtrar por empleado
    if (idEmpleado) {
        cajasFiltradas_temp = cajasFiltradas_temp.filter(c => c.idEmpleado === parseInt(idEmpleado));
    }

    pagActualCajas = 1;
    renderHistorialCajas(cajasFiltradas_temp);
    toast(`${cajasFiltradas_temp.length} caja(s) encontrada(s)`, 'success');
}

function limpiarFiltrosCajas() {
    document.getElementById('filtroCajaEmpleado').value = '';

    pagActualCajas = 1;
    renderHistorialCajas(todasCajas);
    toast('Filtros limpiados', 'success');
}

// ══════════════════════════════════════════════════════
// FILTROS - VENTAS
// ══════════════════════════════════════════════════════

function aplicarFiltrosVentas() {
    const fechaDesde = document.getElementById('filtroVentaFechaDesde').value;
    const fechaHasta = document.getElementById('filtroVentaFechaHasta').value;
    const metodoPago = document.getElementById('filtroVentaMetodoPago').value;

    let ventasFiltradas_temp = [...todasVentas];

    // Filtrar por fecha desde
    if (fechaDesde) {
        const desde = new Date(fechaDesde + 'T00:00:00');
        ventasFiltradas_temp = ventasFiltradas_temp.filter(v => {
            const fechaVenta = new Date(v.fechaVenta);
            return fechaVenta >= desde;
        });
    }

    // Filtrar por fecha hasta
    if (fechaHasta) {
        const hasta = new Date(fechaHasta + 'T23:59:59');
        ventasFiltradas_temp = ventasFiltradas_temp.filter(v => {
            const fechaVenta = new Date(v.fechaVenta);
            return fechaVenta <= hasta;
        });
    }

    // Filtrar por método de pago
    if (metodoPago) {
        ventasFiltradas_temp = ventasFiltradas_temp.filter(v => v.metodoPago === metodoPago);
    }

    pagActualVentas = 1;
    renderTodasVentas(ventasFiltradas_temp);

    if (fechaDesde || fechaHasta || metodoPago) {
        toast(`${ventasFiltradas_temp.length} venta(s) encontrada(s)`, 'success');
    }
}

function limpiarFiltrosVentas() {
    document.getElementById('filtroVentaFechaDesde').value = '';
    document.getElementById('filtroVentaFechaHasta').value = '';
    document.getElementById('filtroVentaMetodoPago').value = '';

    pagActualVentas = 1;
    renderTodasVentas(todasVentas);
    toast('Filtros limpiados', 'success');
}

// ══════════════════════════════════════════════════════
// REGISTRAR VENTA DESDE RESERVA
// ══════════════════════════════════════════════════════

let reservasSinVenta = [];
let reservaSeleccionada = null;

async function abrirModalRegistrarVenta() {
    // Validar que haya caja abierta
    if (!cajaActual) {
        toast('Debe abrir una caja antes de registrar ventas', 'error');
        return;
    }

    document.getElementById('modalRegistrarVenta').style.display = 'flex';
    await cargarReservasSinVenta();
}

async function cargarReservasSinVenta() {
    try {
        const select = document.getElementById('selectReservaVenta');
        select.innerHTML = '<option value="">Cargando reservas...</option>';

        // Usar el nuevo endpoint optimizado que devuelve solo reservas sin venta
        const res = await fetch(API + '/api/Reserva/sin-venta', { headers: authHeaders() });
        if (!res.ok) throw new Error('Error al cargar reservas');

        reservasSinVenta = await res.json();

        // Actualizar select
        if (reservasSinVenta.length === 0) {
            select.innerHTML = '<option value="">No hay reservas disponibles para registrar venta</option>';
            return;
        }

        select.innerHTML = '<option value="">Seleccionar reserva...</option>';
        reservasSinVenta.forEach(r => {
            const fecha = new Date(r.fechaReserva).toLocaleDateString('es-CO');
            const option = document.createElement('option');
            option.value = r.id;
            option.textContent = `${r.codigoReserva || 'RES-' + r.id} - ${r.nombreCliente} (${r.nombreMascota}) - ${fecha}`;
            select.appendChild(option);
        });

    } catch (err) {
        console.error('Error al cargar reservas:', err);
        toast('Error al cargar reservas sin venta', 'error');
    }
}

function cargarDetallesReserva() {
    const idReserva = parseInt(document.getElementById('selectReservaVenta').value);
    const detallesDiv = document.getElementById('detallesReservaVenta');

    if (!idReserva) {
        detallesDiv.style.display = 'none';
        reservaSeleccionada = null;
        return;
    }

    reservaSeleccionada = reservasSinVenta.find(r => r.id === idReserva);

    if (!reservaSeleccionada) {
        detallesDiv.style.display = 'none';
        return;
    }

    // Mostrar detalles
    document.getElementById('detalleCliente').textContent = reservaSeleccionada.nombreCliente;
    document.getElementById('detalleMascota').textContent =
        `${reservaSeleccionada.nombreMascota} (${reservaSeleccionada.razaMascota || 'N/A'})`;

    const fecha = new Date(reservaSeleccionada.fechaReserva);
    document.getElementById('detalleFecha').textContent =
        fecha.toLocaleDateString('es-CO') + ' ' + reservaSeleccionada.horaInicio;

    const servicios = reservaSeleccionada.servicios?.map(s => s.nombreServicio).join(', ') || 'N/A';
    document.getElementById('detalleServicios').textContent = servicios;

    document.getElementById('detalleTotal').textContent =
        '$' + (reservaSeleccionada.precioTotal || 0).toLocaleString('es-CO');

    detallesDiv.style.display = 'block';
}

async function registrarVentaDesdeReserva(event) {
    event.preventDefault();

    if (!reservaSeleccionada) {
        toast('Debe seleccionar una reserva', 'error');
        return;
    }

    const metodoPago = document.getElementById('metodoPagoVenta').value;
    const descuento = parseFloat(document.getElementById('descuentoVenta').value) || 0;

    const btn = event.target.querySelector('button[type="submit"]');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner"></span>Registrando...';

    try {
        const payload = {
            idReserva: reservaSeleccionada.id,
            idCliente: reservaSeleccionada.idCliente,
            idEmpleado: reservaSeleccionada.idEmpleado,
            metodoPago: metodoPago,
            descuento: descuento
        };

        const res = await fetch(API + '/api/Venta', {
            method: 'POST',
            headers: authHeaders(),
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const error = await res.json().catch(() => ({ message: 'Error al registrar venta' }));
            throw new Error(error.message || 'Error al registrar venta');
        }

        toast('✓ Venta registrada correctamente como Pendiente', 'success');
        cerrarModal(null, 'modalRegistrarVenta');

        // Limpiar formulario
        document.getElementById('formRegistrarVenta').reset();
        document.getElementById('detallesReservaVenta').style.display = 'none';
        reservaSeleccionada = null;

        // Recargar datos
        await cargarDatos();

    } catch (err) {
        console.error('Error:', err);
        toast(err.message, 'error');
    } finally {
        btn.disabled = false;
        btn.innerHTML = 'Registrar Venta';
    }
}

// ══════════════════════════════════════════════════════
// UTILIDADES
// ══════════════════════════════════════════════════════

function cerrarModal(event, modalId) {
    if (event && event.target.classList.contains('modal')) return;
    document.getElementById(modalId).style.display = 'none';
}

function formatFecha(fecha) {
    if (!fecha) return '-';
    const f = new Date(fecha);
    return f.toLocaleDateString('es-CO', { year: 'numeric', month: 'short', day: 'numeric' });
}

function formatHora(fecha) {
    if (!fecha) return '-';
    const f = new Date(fecha);
    return f.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });
}

// ══════════════════════════════════════════════════════
// INIT
// ══════════════════════════════════════════════════════

cargarDatos();
