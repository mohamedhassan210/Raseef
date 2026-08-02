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
        return durationStr.endsWith('ms') ? parseInt(durationStr, 18) : parseFloat(durationStr) * 1000 || 800;
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

    zoneTransfer.addEventListener('click', () => {
        handleZoneClick('state-hover-transfer', false, true, window.appRoutes.viewRole);
    });

    zoneSupply.addEventListener('click', () => {
        handleZoneClick('state-hover-supply', true, false, window.appRoutes.supOrTra);
    });
});

const logoutBtn = document.getElementById('logout-btn');
if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {
        window.location.href = window.appRoutes.login;
    });
}