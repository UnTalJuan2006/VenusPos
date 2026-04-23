// ═══════════════════════════════════════════════════════════════════════════
// VENUSPOS - RESERVAS ADMIN JAVASCRIPT
// CRUD Básico de Reservas
// ═══════════════════════════════════════════════════════════════════════════

// ───────────────────────────────────────────────────────────────────────────
// VARIABLES GLOBALES
// ───────────────────────────────────────────────────────────────────────────

let reservas = [];
let reservasFiltradas = [];

const API_BASE = '/api';

// ───────────────────────────────────────────────────────────────────────────
// INICIALIZACIÓN
// ───────────────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function() {
    inicializar();
});

async function inicializar() {
    console.log('=== Inicializando página de reservas ===');
    mostrarFechaHoy();
    await cargarReservas();
    inicializarSidebar();
    inicializarTheme();
    console.log('=== Inicialización completa ===');
}

function mostrarFechaHoy() {
    const opciones = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
    const fechaStr = new Date().toLocaleDateString('es-ES', opciones);
    const fechaHoyEl = document.getElementById('fechaHoy');
    if (fechaHoyEl) {
        fechaHoyEl.textContent = fechaStr.charAt(0).toUpperCase() + fechaStr.slice(1);
    }
}

// ───────────────────────────────────────────────────────────────────────────
// CARGA DE DATOS
// ───────────────────────────────────────────────────────────────────────────

async function cargarReservas() {
    mostrarLoading(true);
    try {
        const response = await fetch(`${API_BASE}/Reserva`);

        // Si hay error de autenticación o el endpoint falla, usar datos de ejemplo
        if (!response.ok) {
            console.warn('API no disponible, usando datos de ejemplo');
            cargarDatosEjemplo();
            return;
        }

        reservas = await response.json();

        // Si no hay reservas, cargar datos de ejemplo
        if (!reservas || reservas.length === 0) {
            console.log('No hay reservas en la BD, cargando datos de ejemplo');
            cargarDatosEjemplo();
            return;
        }

        reservasFiltradas = [...reservas];
        renderizarTabla();
        actualizarKPIs();
    } catch (error) {
        console.error('Error:', error);
        console.log('Cargando datos de ejemplo debido a error');
        cargarDatosEjemplo();
    } finally {
        mostrarLoading(false);
    }
}

function cargarDatosEjemplo() {
    console.log('Cargando datos de ejemplo...');
    const hoy = new Date();
    const mañana = new Date(hoy);
    mañana.setDate(mañana.getDate() + 1);

    reservas = [
        {
            id: 1,
            codigoReserva: 'RES-20260329-001',
            fechaReserva: hoy.toISOString(),
            horaInicio: '09:00:00',
            horaFin: '11:00:00',
            duracionMinutos: 120,
            nombreCliente: 'Juan Pérez',
            emailCliente: 'juan@email.com',
            nombreMascota: 'Max',
            razaMascota: 'Golden Retriever',
            tamañoMascota: 'Grande',
            nombreEmpleado: 'Carlos Rodríguez',
            servicios: [{ nombre: 'Baño Completo' }],
            estado: 'Confirmada',
            precioTotal: 97500
        },
        {
            id: 2,
            codigoReserva: 'RES-20260329-002',
            fechaReserva: hoy.toISOString(),
            horaInicio: '13:00:00',
            horaFin: '14:30:00',
            duracionMinutos: 90,
            nombreCliente: 'María González',
            emailCliente: 'maria@email.com',
            nombreMascota: 'Luna',
            razaMascota: 'Poodle',
            tamañoMascota: 'Pequeño',
            nombreEmpleado: 'Sofía Martínez',
            servicios: [{ nombre: 'Corte de Pelo' }],
            estado: 'Pendiente',
            precioTotal: 45000
        },
        {
            id: 3,
            codigoReserva: 'RES-20260329-003',
            fechaReserva: hoy.toISOString(),
            horaInicio: '10:00:00',
            horaFin: '12:00:00',
            duracionMinutos: 120,
            nombreCliente: 'Pedro Ramírez',
            emailCliente: 'pedro@email.com',
            nombreMascota: 'Rocky',
            razaMascota: 'Bulldog',
            tamañoMascota: 'Mediano',
            nombreEmpleado: 'María González',
            servicios: [{ nombre: 'Spa Completo' }],
            estado: 'Completada',
            precioTotal: 85000
        },
        {
            id: 4,
            codigoReserva: 'RES-20260329-004',
            fechaReserva: mañana.toISOString(),
            horaInicio: '14:00:00',
            horaFin: '15:30:00',
            duracionMinutos: 90,
            nombreCliente: 'Ana Martínez',
            emailCliente: 'ana@email.com',
            nombreMascota: 'Kira',
            razaMascota: 'Schnauzer',
            tamañoMascota: 'Pequeño',
            nombreEmpleado: 'Carlos Rodríguez',
            servicios: [{ nombre: 'Baño Completo' }, { nombre: 'Corte de Uñas' }],
            estado: 'Confirmada',
            precioTotal: 62500
        },
        {
            id: 5,
            codigoReserva: null,
            fechaReserva: mañana.toISOString(),
            horaInicio: '16:00:00',
            horaFin: '17:00:00',
            duracionMinutos: 60,
            nombreCliente: 'Luis Torres',
            emailCliente: 'luis@email.com',
            nombreMascota: 'Toby',
            razaMascota: 'Beagle',
            tamañoMascota: 'Mediano',
            nombreEmpleado: 'Sofía Martínez',
            servicios: [{ nombre: 'Limpieza Dental' }],
            estado: 'Pendiente',
            precioTotal: 35000
        },
        {
            id: 6,
            codigoReserva: 'RES-20260328-015',
            fechaReserva: new Date(hoy.getTime() - 86400000).toISOString(),
            horaInicio: '11:00:00',
            horaFin: '12:30:00',
            duracionMinutos: 90,
            nombreCliente: 'Carmen Díaz',
            emailCliente: 'carmen@email.com',
            nombreMascota: 'Bella',
            razaMascota: 'Chihuahua',
            tamañoMascota: 'Pequeño',
            nombreEmpleado: 'María González',
            servicios: [{ nombre: 'Baño Completo' }],
            estado: 'Completada',
            precioTotal: 57500
        }
    ];

    reservasFiltradas = [...reservas];
    console.log('Datos de ejemplo cargados:', reservas.length, 'reservas');
    renderizarTabla();
    actualizarKPIs();

    mostrarToast('Mostrando datos de ejemplo', 'info');
}

// ───────────────────────────────────────────────────────────────────────────
// RENDERIZADO DE TABLA
// ───────────────────────────────────────────────────────────────────────────

function renderizarTabla() {
    console.log('Renderizando tabla con', reservasFiltradas.length, 'reservas');
    const tbody = document.getElementById('tablaBody');

    if (!tbody) {
        console.error('ERROR: No se encontró el elemento tablaBody');
        return;
    }

    tbody.innerHTML = '';

    if (reservasFiltradas.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="10">
                    <div class="empty-state">
                        <svg width="64" height="64" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                            <rect x="3" y="4" width="18" height="18" rx="2" />
                            <path d="M16 2v4M8 2v4M3 10h18" />
                        </svg>
                        <h3>No hay reservas</h3>
                        <p>No se encontraron reservas con los filtros aplicados</p>
                    </div>
                </td>
            </tr>
        `;
        return;
    }

    reservasFiltradas.forEach(reserva => {
        const fila = crearFilaReserva(reserva);
        tbody.appendChild(fila);
    });
}

function crearFilaReserva(reserva) {
    const tr = document.createElement('tr');

    const fecha = formatearFecha(reserva.fechaReserva);
    const horaInicio = formatearHora(reserva.horaInicio);
    const horaFin = formatearHora(reserva.horaFin);
    const precio = formatearMoneda(reserva.precioTotal);
    const estadoClass = reserva.estado.toLowerCase();

    tr.innerHTML = `
        <td class="codigo-cell">${reserva.codigoReserva || 'N/A'}</td>
        <td class="fecha-cell">${fecha}</td>
        <td class="hora-cell">${horaInicio} - ${horaFin}</td>
        <td class="cliente-cell">${reserva.nombreCliente || 'N/A'}</td>
        <td class="mascota-cell">${reserva.nombreMascota || 'N/A'}</td>
        <td class="empleado-cell">${reserva.nombreEmpleado || 'N/A'}</td>
        <td class="servicios-cell">${renderizarServicios(reserva)}</td>
        <td>
            <span class="estado-badge ${estadoClass}">
                <span class="estado-dot"></span>
                ${reserva.estado}
            </span>
        </td>
        <td class="precio-cell">${precio}</td>
        <td>
            <div class="actions-cell">
                <button class="btn-icon" onclick="verDetalle(${reserva.id})" title="Ver detalle">
                    <svg width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                        <path d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                </button>
                <button class="btn-icon edit" onclick="editarReserva(${reserva.id})" title="Editar">
                    <svg width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                </button>
                <button class="btn-icon delete" onclick="confirmarEliminar(${reserva.id})" title="Eliminar">
                    <svg width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                </button>
            </div>
        </td>
    `;

    return tr;
}

function renderizarServicios(reserva) {
    // Si la reserva tiene servicios en un array
    if (reserva.servicios && Array.isArray(reserva.servicios) && reserva.servicios.length > 0) {
        return reserva.servicios.map(s =>
            `<span class="servicio-tag">${s.nombre || s}</span>`
        ).join('');
    }
    return '<span class="servicio-tag">Sin servicios</span>';
}

// ───────────────────────────────────────────────────────────────────────────
// FILTRADO
// ───────────────────────────────────────────────────────────────────────────

function filtrarReservas() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const estadoFiltro = document.getElementById('filtroEstado').value;
    const fechaFiltro = document.getElementById('filtroFecha').value;

    reservasFiltradas = reservas.filter(reserva => {
        // Filtro de búsqueda
        const cumpleBusqueda = !searchTerm ||
            (reserva.codigoReserva && reserva.codigoReserva.toLowerCase().includes(searchTerm)) ||
            (reserva.nombreCliente && reserva.nombreCliente.toLowerCase().includes(searchTerm)) ||
            (reserva.nombreMascota && reserva.nombreMascota.toLowerCase().includes(searchTerm)) ||
            (reserva.emailCliente && reserva.emailCliente.toLowerCase().includes(searchTerm));

        // Filtro de estado
        const cumpleEstado = !estadoFiltro || reserva.estado === estadoFiltro;

        // Filtro de fecha
        let cumpleFecha = true;
        if (fechaFiltro) {
            const fechaReserva = new Date(reserva.fechaReserva).toISOString().split('T')[0];
            cumpleFecha = fechaReserva === fechaFiltro;
        }

        return cumpleBusqueda && cumpleEstado && cumpleFecha;
    });

    renderizarTabla();
    actualizarKPIs();
}

// ───────────────────────────────────────────────────────────────────────────
// KPIs
// ───────────────────────────────────────────────────────────────────────────

function actualizarKPIs() {
    const datos = reservasFiltradas;

    document.getElementById('statTotal').textContent = datos.length;
    document.getElementById('statConfirmadas').textContent =
        datos.filter(r => r.estado === 'Confirmada').length;
    document.getElementById('statPendientes').textContent =
        datos.filter(r => r.estado === 'Pendiente').length;
    document.getElementById('statCompletadas').textContent =
        datos.filter(r => r.estado === 'Completada').length;

    // Actualizar badge en sidebar
    const badge = document.getElementById('badgeReservas');
    if (badge) {
        badge.textContent = reservas.length;
    }
}

// ───────────────────────────────────────────────────────────────────────────
// OPERACIONES CRUD
// ───────────────────────────────────────────────────────────────────────────

function abrirModalCrear() {
    // Redirigir a la página de creación de reservas del cliente
    window.location.href = '/AgendaCita.html';
}

function verDetalle(id) {
    const reserva = reservas.find(r => r.id === id);
    if (!reserva) return;

    alert(`Detalle de Reserva #${id}\n\n` +
          `Código: ${reserva.codigoReserva || 'N/A'}\n` +
          `Cliente: ${reserva.nombreCliente}\n` +
          `Mascota: ${reserva.nombreMascota}\n` +
          `Empleado: ${reserva.nombreEmpleado}\n` +
          `Fecha: ${formatearFecha(reserva.fechaReserva)}\n` +
          `Hora: ${formatearHora(reserva.horaInicio)} - ${formatearHora(reserva.horaFin)}\n` +
          `Estado: ${reserva.estado}\n` +
          `Precio: ${formatearMoneda(reserva.precioTotal)}`);

    // TODO: Implementar modal de detalle
}

function editarReserva(id) {
    alert(`Editar reserva #${id}`);
    // TODO: Implementar modal de edición
}

async function confirmarEliminar(id) {
    if (!confirm('¿Estás seguro de que deseas eliminar esta reserva?')) {
        return;
    }

    await eliminarReserva(id);
}

async function eliminarReserva(id) {
    mostrarLoading(true);
    try {
        const response = await fetch(`${API_BASE}/Reserva/${id}`, {
            method: 'DELETE'
        });

        if (!response.ok) throw new Error('Error al eliminar la reserva');

        mostrarToast('Reserva eliminada exitosamente', 'success');
        await cargarReservas();
    } catch (error) {
        console.error('Error:', error);
        mostrarToast('Error al eliminar la reserva', 'error');
    } finally {
        mostrarLoading(false);
    }
}

// ───────────────────────────────────────────────────────────────────────────
// UTILIDADES DE FORMATO
// ───────────────────────────────────────────────────────────────────────────

function formatearFecha(fechaStr) {
    const fecha = new Date(fechaStr);
    const dia = fecha.getDate().toString().padStart(2, '0');
    const mes = (fecha.getMonth() + 1).toString().padStart(2, '0');
    const anio = fecha.getFullYear();
    return `${dia}/${mes}/${anio}`;
}

function formatearHora(horaStr) {
    if (!horaStr) return 'N/A';
    const [hora, minuto] = horaStr.split(':');
    const h = parseInt(hora);
    const ampm = h >= 12 ? 'PM' : 'AM';
    const h12 = h % 12 || 12;
    return `${h12}:${minuto} ${ampm}`;
}

function formatearMoneda(valor) {
    return new Intl.NumberFormat('es-CO', {
        style: 'currency',
        currency: 'COP',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(valor);
}

// ───────────────────────────────────────────────────────────────────────────
// UI - LOADING Y TOAST
// ───────────────────────────────────────────────────────────────────────────

function mostrarLoading(mostrar) {
    let overlay = document.getElementById('loadingOverlay');

    if (mostrar) {
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'loadingOverlay';
            overlay.className = 'loading-overlay';
            overlay.innerHTML = '<div class="loading-spinner"></div>';
            document.body.appendChild(overlay);
        }
    } else {
        if (overlay) {
            overlay.remove();
        }
    }
}

function mostrarToast(mensaje, tipo = 'info') {
    const iconos = {
        success: '<svg width="20" height="20" fill="none" viewBox="0 0 24 24" stroke="#10b981" stroke-width="2"><path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>',
        error: '<svg width="20" height="20" fill="none" viewBox="0 0 24 24" stroke="#ef4444" stroke-width="2"><path d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>',
        warning: '<svg width="20" height="20" fill="none" viewBox="0 0 24 24" stroke="#f59e0b" stroke-width="2"><path d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>',
        info: '<svg width="20" height="20" fill="none" viewBox="0 0 24 24" stroke="#7c3aed" stroke-width="2"><path d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>'
    };

    const toast = document.createElement('div');
    toast.className = `toast ${tipo}`;
    toast.innerHTML = `
        <div class="toast-icon">${iconos[tipo] || iconos.info}</div>
        <div class="toast-content">
            <div class="toast-message">${mensaje}</div>
        </div>
        <button class="toast-close" onclick="this.parentElement.remove()">
            <svg width="16" height="16" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path d="M6 18L18 6M6 6l12 12" />
            </svg>
        </button>
    `;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

// ───────────────────────────────────────────────────────────────────────────
// SIDEBAR Y THEME
// ───────────────────────────────────────────────────────────────────────────

function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('collapsed');
    localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
}

function inicializarSidebar() {
    const sidebar = document.getElementById('sidebar');
    const collapsed = localStorage.getItem('sidebarCollapsed') === 'true';
    if (collapsed) {
        sidebar.classList.add('collapsed');
    }
}

function toggleTheme() {
    const html = document.documentElement;
    const currentTheme = html.getAttribute('data-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

    html.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);

    const iconSun = document.getElementById('iconSun');
    const iconMoon = document.getElementById('iconMoon');

    if (newTheme === 'dark') {
        iconSun.style.display = 'none';
        iconMoon.style.display = 'block';
    } else {
        iconSun.style.display = 'block';
        iconMoon.style.display = 'none';
    }
}

function inicializarTheme() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);

    const iconSun = document.getElementById('iconSun');
    const iconMoon = document.getElementById('iconMoon');

    if (savedTheme === 'dark') {
        iconSun.style.display = 'none';
        iconMoon.style.display = 'block';
    }
}

function toggleDrop() {
    const profile = document.getElementById('tbProfile');
    const dropdown = document.getElementById('tbDrop');
    const isOpen = dropdown.classList.toggle('open');
    profile.classList.toggle('open', isOpen);
}

function logout() {
    if (confirm('¿Estás seguro de que deseas cerrar sesión?')) {
        localStorage.clear();
        window.location.href = '/';
    }
}

// Cerrar dropdown al hacer clic fuera
document.addEventListener('click', function(event) {
    const wrap = document.querySelector('.tb-profile-wrap');
    if (wrap && !wrap.contains(event.target)) {
        document.getElementById('tbDrop')?.classList.remove('open');
        document.getElementById('tbProfile')?.classList.remove('open');
    }
});
