/**
 * RASEEF — App State & Utils
 * ===========================
 * Shared globally across all dashboard pages.
 */

// 1. APP STATE
const AppState = {
    flowType: null,
    selectedSupplier: null,
    selectedTruck: null,
    selectedDriver: null,
    orderType: null,

    save() {
        sessionStorage.setItem('raseef_state', JSON.stringify({
            flowType: this.flowType,
            selectedSupplier: this.selectedSupplier,
            selectedTruck: this.selectedTruck,
            selectedDriver: this.selectedDriver,
            orderType: this.orderType,
        }));
    },

    load() {
        try {
            const saved = JSON.parse(sessionStorage.getItem('raseef_state') || '{}');
            Object.assign(this, saved);
        } catch (e) { /* ignore */ }
    },

    reset() {
        this.flowType = null;
        this.selectedSupplier = null;
        this.selectedTruck = null;
        this.selectedDriver = null;
        this.orderType = null;
        sessionStorage.removeItem('raseef_state');
    },

    setSupplier(supplier) { this.selectedSupplier = supplier; this.save(); },
    setTruck(truck) { this.selectedTruck = truck; this.save(); },
    setDriver(driver) { this.selectedDriver = driver; this.save(); },
    setFlowType(type) { this.flowType = type; this.save(); }
};

// 2. UTILITY FUNCTIONS
function navigateTo(page) {
    document.body.style.opacity = '0';
    document.body.style.transition = 'opacity 0.2s ease';
    setTimeout(() => { window.location.href = page; }, 200);
}

function showToast(message, type = 'info', duration = 3000) {
    // Remove existing toasts
    document.querySelectorAll('.toast').forEach(t => t.remove());

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'toastIn 0.3s ease reverse forwards';
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

function debounce(fn, delay = 350) {
    let timer;
    return function (...args) {
        clearTimeout(timer);
        timer = setTimeout(() => fn.apply(this, args), delay);
    };
}

function formatPhone(phone) {
    if (!phone) return '';
    const digits = phone.replace(/\D/g, '');
    if (digits.length === 11) {
        return `${digits.slice(0, 4)} ${digits.slice(4, 7)} ${digits.slice(7)}`;
    }
    return phone;
}

function requireAuth() {
    return true; // Guard Placeholder
}

// 3. PAGE INIT
document.addEventListener('DOMContentLoaded', () => {
    // Load saved state
    AppState.load();

    // Fade in on page load
    document.body.style.opacity = '0';
    requestAnimationFrame(() => {
        document.body.style.transition = 'opacity 0.3s ease';
        document.body.style.opacity = '1';
    });
});