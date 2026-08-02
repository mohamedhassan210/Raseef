document.addEventListener('DOMContentLoaded', () => {
    const canvas = document.querySelector('.viewport-canvas');
    const zoneTransfer = document.getElementById('zone-transfer');
    const zoneSupply = document.getElementById('zone-supply');
    const logoRaseefi = document.getElementById('logo-raseefi');
    const logoFathalla = document.getElementById('logo-fathalla');

    const logoRaseefiState = (show) => {
        if (show) logoRaseefi.classList.remove('logo-hidden');
        else logoRaseefi.classList.add('logo-hidden');
    };

    const logoFathallahState = (show) => {
        if (show) logoFathalla.classList.remove('logo-hidden');
        else logoFathalla.classList.add('logo-hidden');
    };

    const activateTransfer = () => {
        canvas.classList.add('state-hover-transfer');
        canvas.classList.remove('state-hover-supply');
        // إخفاء اللوجوهين معاً
        logoFathallahState(true);
        logoRaseefiState(false);
    };

    const activateSupply = () => {
        canvas.classList.add('state-hover-supply');
        canvas.classList.remove('state-hover-transfer');
        // إخفاء اللوجوهين معاً 
        logoRaseefiState(true);
        logoFathallahState(false);
    };

    const resetState = () => {
        canvas.classList.remove('state-hover-transfer', 'state-hover-supply');
        // إظهار اللوجوهين مرة تانية لما الماوس يخرج بره الـ canvas
        logoRaseefiState(true);
        logoFathallahState(true);
    };

    // Events 
    zoneTransfer.addEventListener('mouseenter', activateTransfer);
    zoneSupply.addEventListener('mouseenter', activateSupply);
    canvas.addEventListener('mouseleave', resetState);

    // استخدام مسارات MVC الممررة عبر window.appRoutes
    zoneTransfer.addEventListener('click', () => {
        if (window.appRoutes && window.appRoutes.viewRole) {
            window.location.href = window.appRoutes.viewRole;
        }
    });

    zoneSupply.addEventListener('click', () => {
        if (window.appRoutes && window.appRoutes.supOrTra) {
            window.location.href = window.appRoutes.supOrTra;
        }
    });
});

// تفعيل زر تسجيل الخروج للانتقال لصفحة Login بناءً على MVC Routing
const logoutBtn = document.getElementById('logout-btn');
if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {
        if (window.appRoutes && window.appRoutes.login) {
            window.location.href = window.appRoutes.login;
        }
    });
}