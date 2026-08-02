document.addEventListener('DOMContentLoaded', () => {
    const canvas = document.querySelector('.viewport-canvas');
    const zoneTransfer = document.getElementById('zone-transfer');
    const zoneSupply = document.getElementById('zone-supply');
    const logoRaseefi = document.getElementById('logo-raseefi');
    const logoFathalla = document.getElementById('logo-fathalla');

    let isAnimating = false;

    const toggleLogo = (logo, show) => {
        if (show) {
            logo.classList.remove('logo-hidden');
        } else {
            logo.classList.add('logo-hidden');
        }
    };

    const getAnimationDuration = () => {
        const durationStr = getComputedStyle(document.documentElement).getPropertyValue('--duration-standard').trim();
        return durationStr.endsWith('ms') ? parseInt(durationStr, 10) : parseFloat(durationStr) * 1000 || 800;
    };

    const handleZoneClick = (stateClass, showRaseefi, showFathalla, targetUrl) => {
        if (isAnimating) return;
        isAnimating = true;

        document.body.style.pointerEvents = 'none';

        canvas.classList.add(stateClass);
        toggleLogo(logoRaseefi, showRaseefi);
        toggleLogo(logoFathalla, showFathalla);

        setTimeout(() => {
            window.location.href = targetUrl;
        }, getAnimationDuration());
    };

    if (zoneTransfer) {
        zoneTransfer.addEventListener('click', () => {
            const targetUrl = zoneTransfer.getAttribute('data-url') || 'transfer.html';
            handleZoneClick('state-hover-transfer', false, true, targetUrl);
        });
    }

    if (zoneSupply) {
        zoneSupply.addEventListener('click', () => {
            const targetUrl = zoneSupply.getAttribute('data-url') || 'suppliers.html';
            handleZoneClick('state-hover-supply', true, false, targetUrl);
        });
    }
});

// تفعيل زر تسجيل الخروج مع قراءة الرابط من الـ HTML
const logoutBtn = document.getElementById('logout-btn');
if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {
        const targetUrl = logoutBtn.getAttribute('data-url') || 'index.html';
        window.location.href = targetUrl;
    });
}

// تفعيل زر خطوة للخلف مع قراءة الرابط من الـ HTML
const stepbackBtn = document.getElementById('stepback-btn');
if (stepbackBtn) {
    stepbackBtn.addEventListener('click', () => {
        const targetUrl = stepbackBtn.getAttribute('data-url') || 'addRoleOrviewRole.cshtml';
        window.location.href = targetUrl;
    });
}