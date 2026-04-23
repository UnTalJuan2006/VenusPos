// ═══════════════════════════════════════════════════════════════════════════
// VENUSPOS - RESERVA UTILITIES
// Funciones compartidas para el flujo de reservas
// ═══════════════════════════════════════════════════════════════════════════

// ───────────────────────────────────────────────────────────────────────────
// LocalStorage Management
// ───────────────────────────────────────────────────────────────────────────

const ReservaStorage = {
    // Keys
    KEYS: {
        CLIENTE: 'reserva_cliente',
        MASCOTA: 'reserva_mascota',
        SERVICIOS: 'reserva_servicios',
        PRECIO: 'reserva_precio',
        HORARIO: 'reserva_horario',
        EMPLEADO: 'reserva_empleado',
        METODO_PAGO: 'reserva_metodo_pago',
        CONFIRMADA: 'reserva_confirmada'
    },

    // Save data
    set(key, data) {
        localStorage.setItem(key, JSON.stringify(data));
    },

    // Get data
    get(key) {
        const item = localStorage.getItem(key);
        return item ? JSON.parse(item) : null;
    },

    // Remove data
    remove(key) {
        localStorage.removeItem(key);
    },

    // Clear all reservation data
    clearAll() {
        Object.values(this.KEYS).forEach(key => {
            localStorage.removeItem(key);
        });
    },

    // Get cliente
    getCliente() {
        return this.get(this.KEYS.CLIENTE);
    },

    // Set cliente
    setCliente(cliente) {
        this.set(this.KEYS.CLIENTE, cliente);
    },

    // Get mascota
    getMascota() {
        return this.get(this.KEYS.MASCOTA);
    },

    // Set mascota
    setMascota(mascota) {
        this.set(this.KEYS.MASCOTA, mascota);
    },

    // Get servicios (array of IDs)
    getServicios() {
        return this.get(this.KEYS.SERVICIOS);
    },

    // Set servicios (array of IDs)
    setServicios(servicios) {
        this.set(this.KEYS.SERVICIOS, servicios);
    },

    // Get precio calculation
    getPrecio() {
        return this.get(this.KEYS.PRECIO);
    },

    // Set precio calculation
    setPrecio(precio) {
        this.set(this.KEYS.PRECIO, precio);
    },

    // Get horario
    getHorario() {
        return this.get(this.KEYS.HORARIO);
    },

    // Set horario
    setHorario(horario) {
        this.set(this.KEYS.HORARIO, horario);
    },

    // Get empleado
    getEmpleado() {
        return this.get(this.KEYS.EMPLEADO);
    },

    // Set empleado
    setEmpleado(empleado) {
        this.set(this.KEYS.EMPLEADO, empleado);
    },

    // Get metodo de pago
    getMetodoPago() {
        return this.get(this.KEYS.METODO_PAGO) || 'Efectivo';
    },

    // Set metodo de pago
    setMetodoPago(metodoPago) {
        this.set(this.KEYS.METODO_PAGO, metodoPago);
    },

    // Get confirmed reservation
    getConfirmada() {
        return this.get(this.KEYS.CONFIRMADA);
    },

    // Set confirmed reservation
    setConfirmada(reserva) {
        this.set(this.KEYS.CONFIRMADA, reserva);
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Date and Time Formatting
// ───────────────────────────────────────────────────────────────────────────

const DateUtils = {
    // Format date to Spanish long format: "lunes, 24 de marzo de 2025"
    formatLongDate(dateString) {
        // Parse date as local time to avoid timezone issues
        const [year, month, day] = dateString.split('T')[0].split('-');
        const date = new Date(year, month - 1, day);
        const options = {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        };
        return date.toLocaleDateString('es-ES', options);
    },

    // Format date to short format: "24/03/2025"
    formatShortDate(dateString) {
        // Parse date as local time to avoid timezone issues
        const [year, month, day] = dateString.split('T')[0].split('-');
        const date = new Date(year, month - 1, day);
        return date.toLocaleDateString('es-ES');
    },

    // Format time from TimeOnly string "14:30:00" to "2:30 PM"
    formatTime(timeString) {
        const [hours, minutes] = timeString.split(':');
        const hour = parseInt(hours);
        const ampm = hour >= 12 ? 'PM' : 'AM';
        const hour12 = hour % 12 || 12;
        return `${hour12}:${minutes} ${ampm}`;
    },

    // Format time range "14:30:00" - "16:30:00" to "2:30 PM - 4:30 PM"
    formatTimeRange(startTime, endTime) {
        return `${this.formatTime(startTime)} - ${this.formatTime(endTime)}`;
    },

    // Get date for API (YYYY-MM-DD)
    toAPIDate(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    },

    // Parse API date string to Date object
    parseAPIDate(dateString) {
        return new Date(dateString);
    },

    // Check if date is today
    isToday(date) {
        const today = new Date();
        return date.getDate() === today.getDate() &&
               date.getMonth() === today.getMonth() &&
               date.getFullYear() === today.getFullYear();
    },

    // Check if date is in the past
    isPast(date) {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        return date < today;
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Currency Formatting
// ───────────────────────────────────────────────────────────────────────────

const CurrencyUtils = {
    // Format number to Colombian Pesos: "$50.000"
    format(amount) {
        return new Intl.NumberFormat('es-CO', {
            style: 'currency',
            currency: 'COP',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(amount);
    },

    // Format without currency symbol: "50.000"
    formatNumber(amount) {
        return new Intl.NumberFormat('es-CO', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(amount);
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Toast Notifications
// ───────────────────────────────────────────────────────────────────────────

const Toast = {
    show(message, type = 'info') {
        // Remove existing toasts
        const existing = document.querySelector('.toast-notification');
        if (existing) existing.remove();

        // Create toast element
        const toast = document.createElement('div');
        toast.className = `toast-notification toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <span class="toast-icon">${this.getIcon(type)}</span>
                <span class="toast-message">${message}</span>
            </div>
        `;

        // Add to body
        document.body.appendChild(toast);

        // Animate in
        setTimeout(() => toast.classList.add('toast-show'), 10);

        // Remove after 4 seconds
        setTimeout(() => {
            toast.classList.remove('toast-show');
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    },

    getIcon(type) {
        const icons = {
            success: '✓',
            error: '✕',
            warning: '⚠',
            info: 'ℹ'
        };
        return icons[type] || icons.info;
    },

    success(message) {
        this.show(message, 'success');
    },

    error(message) {
        this.show(message, 'error');
    },

    warning(message) {
        this.show(message, 'warning');
    },

    info(message) {
        this.show(message, 'info');
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Loading State Management
// ───────────────────────────────────────────────────────────────────────────

const LoadingUtils = {
    // Show loading spinner on button
    showButtonLoading(button, text = 'Cargando...') {
        button.disabled = true;
        button.dataset.originalText = button.innerHTML;
        button.innerHTML = `
            <span class="spinner"></span>
            ${text}
        `;
    },

    // Hide loading spinner on button
    hideButtonLoading(button) {
        button.disabled = false;
        if (button.dataset.originalText) {
            button.innerHTML = button.dataset.originalText;
        }
    },

    // Show full page loading overlay
    showOverlay(message = 'Cargando...') {
        const overlay = document.createElement('div');
        overlay.id = 'loading-overlay';
        overlay.className = 'loading-overlay';
        overlay.innerHTML = `
            <div class="loading-content">
                <div class="loading-spinner"></div>
                <p>${message}</p>
            </div>
        `;
        document.body.appendChild(overlay);
    },

    // Hide full page loading overlay
    hideOverlay() {
        const overlay = document.getElementById('loading-overlay');
        if (overlay) overlay.remove();
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Validation Utilities
// ───────────────────────────────────────────────────────────────────────────

const ValidationUtils = {
    // Validate email format
    isValidEmail(email) {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    },

    // Validate required field
    isRequired(value) {
        return value !== null && value !== undefined && value.toString().trim() !== '';
    },

    // Show validation error on input
    showError(input, message) {
        const formGroup = input.closest('.form-group');
        if (formGroup) {
            formGroup.classList.add('has-error');

            // Remove existing error message
            const existingError = formGroup.querySelector('.error-message');
            if (existingError) existingError.remove();

            // Add error message
            const errorDiv = document.createElement('div');
            errorDiv.className = 'error-message';
            errorDiv.textContent = message;
            formGroup.appendChild(errorDiv);
        }
    },

    // Clear validation error
    clearError(input) {
        const formGroup = input.closest('.form-group');
        if (formGroup) {
            formGroup.classList.remove('has-error');
            const errorMessage = formGroup.querySelector('.error-message');
            if (errorMessage) errorMessage.remove();
        }
    },

    // Clear all errors in form
    clearAllErrors(form) {
        const formGroups = form.querySelectorAll('.form-group.has-error');
        formGroups.forEach(group => {
            group.classList.remove('has-error');
            const errorMessage = group.querySelector('.error-message');
            if (errorMessage) errorMessage.remove();
        });
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Navigation Utilities
// ───────────────────────────────────────────────────────────────────────────

const NavigationUtils = {
    // Go to page
    goTo(page) {
        window.location.href = page;
    },

    // Go back
    goBack() {
        window.history.back();
    },

    // Check if cliente is logged in
    isClienteLoggedIn() {
        const cliente = ReservaStorage.getCliente();
        return cliente && cliente.id;
    },

    // Require cliente login
    requireCliente() {
        if (!this.isClienteLoggedIn()) {
            Toast.warning('Debes iniciar sesión para continuar');
            setTimeout(() => {
                this.goTo('/AgendaCita.html');
            }, 1500);
            return false;
        }
        return true;
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Progress Indicator
// ───────────────────────────────────────────────────────────────────────────

const ProgressUtils = {
    // Update progress bar
    update(currentStep, totalSteps = 5) {
        const progressBar = document.querySelector('.progress-fill');
        const stepIndicators = document.querySelectorAll('.step-indicator');

        if (progressBar) {
            const percentage = (currentStep / totalSteps) * 100;
            progressBar.style.width = `${percentage}%`;
        }

        if (stepIndicators.length > 0) {
            stepIndicators.forEach((indicator, index) => {
                if (index < currentStep) {
                    indicator.classList.add('completed');
                    indicator.classList.remove('active');
                } else if (index === currentStep) {
                    indicator.classList.add('active');
                    indicator.classList.remove('completed');
                } else {
                    indicator.classList.remove('active', 'completed');
                }
            });
        }
    }
};
