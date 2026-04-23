const API = '';
let cajaActual = null;
let empleadoActual = null;
let ventasPendientes = [];
let ventasConfirmadas = [];

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
        // Cargar caja abierta
        const resCaja = await fetch(API + '/api/Caja/abierta', { headers: authHeaders() });

        if (resCaja.ok) {
            cajaActual = await resCaja.json();
            mostrarCajaAbierta();
        } else {
            mostrarCajaCerrada();
        }

        // Cargar ventas siempre (incluso si no hay caja abierta)
        // Esto permite ver reservas pendientes de pago
        await cargarVentas();
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

async function cargarVentas() {
    try {
        // Cargar TODAS las ventas pendientes (incluso sin caja asignada)
        const resPendientes = await fetch(API + '/api/Venta/pendientes', { headers: authHeaders() });

        if (resPendientes.ok) {
            ventasPendientes = await resPendientes.json();
            renderVentasPendientes();
        }

        // Cargar ventas confirmadas solo si hay caja abierta
        if (cajaActual) {
            const resConfirmadas = await fetch(API + '/api/Venta/caja/' + cajaActual.id, { headers: authHeaders() });

            if (resConfirmadas.ok) {
                const ventas = await resConfirmadas.json();
                ventasConfirmadas = ventas.filter(v => v.estado === 'Confirmada');
                renderVentasConfirmadas();
            }
        } else {
            ventasConfirmadas = [];
            renderVentasConfirmadas();
        }
    } catch (err) {
        console.error('Error al cargar ventas:', err);
    }
}

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
            <button class="btn-confirmar" onclick="confirmarVenta(${v.id})">
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

    const datos = {
        montoCierre: parseFloat(document.getElementById('montoCierre').value),
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
