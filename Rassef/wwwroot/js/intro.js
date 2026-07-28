document.addEventListener('DOMContentLoaded', () => {
    const splashScreen = document.getElementById('splash-screen');
    const fathallaLogo = document.querySelector('.fathalla-logo');
    const fathallaText = document.querySelector('.fathalla-text');

    // Phase 2 (4.8s): ظهور اللوجو في منتصف الشاشة
    setTimeout(() => {
        if (fathallaLogo) {
            fathallaLogo.classList.add('is-visible');
        }
    }, 4800);

    // Phase 3 (5.6s): تحرك اللوجو جهة اليمين
    setTimeout(() => {
        if (fathallaLogo) {
            fathallaLogo.classList.add('shift-right');
        }
    }, 5600);

    // Phase 4 (6.3s): ظهور النص بجانب اللوجو بمحاذاة قريبة جداً
    setTimeout(() => {
        if (fathallaText) {
            fathallaText.classList.add('is-visible');
        }
    }, 6300);

    // Phase 6 (8.4s): تلاشي الانترو
    setTimeout(() => {
        if (splashScreen) {
            splashScreen.classList.add('fade-out');
        }
    }, 8400);
});