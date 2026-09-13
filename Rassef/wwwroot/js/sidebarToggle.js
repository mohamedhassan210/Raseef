/*
 * sidebarToggle.js — المسؤول الوحيد عن فتح/قفل السايدبار على الموبايل
 * (زرار ☰ #mobileToggle) في كل صفحات الأدمن اللي بتستخدم sidebar.css.
 *
 * ليه ملف منفصل؟ لأن بعض الصفحات (Department, Warehouses, Dock,
 * Position, Suppliers, ...) معندهاش أي JS خالص، والبعض التاني
 * (dashboard.js) عنده كود تاني كتير ومعرفش يلمس زرار الموبايل خالص.
 * الملف ده آمن يتحط في أي صفحة، حتى لو الزرار أو السايدبار مش موجودين
 * فيها أصلاً (مفيش هيحصل Error).
 */
document.addEventListener('DOMContentLoaded', function () {
    var toggleBtn = document.getElementById('mobileToggle');
    var sidebar = document.querySelector('.dashboard-sidebar');

    if (!toggleBtn || !sidebar) return;

    toggleBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        sidebar.classList.toggle('active');
    });

    // قفل السايدبار لو المستخدم دوس في أي حتة تانية بره منه
    document.addEventListener('click', function (e) {
        if (
            sidebar.classList.contains('active') &&
            !sidebar.contains(e.target) &&
            e.target !== toggleBtn &&
            !toggleBtn.contains(e.target)
        ) {
            sidebar.classList.remove('active');
        }
    });

    // قفل السايدبار تلقائياً لو المستخدم دوس على أي لينك جواه
    // (يعني بعد ما يختار صفحة تانية من القائمة، القائمة تتقفل وراه)
    sidebar.querySelectorAll('a').forEach(function (link) {
        link.addEventListener('click', function () {
            sidebar.classList.remove('active');
        });
    });
});
