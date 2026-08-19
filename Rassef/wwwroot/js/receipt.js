document.addEventListener('DOMContentLoaded', () => {

    const emptyState = document.getElementById('empty-state');
    const receiptMain = document.getElementById('receipt-main');

    // الأزرار
    const btnBack = document.getElementById('btn-back');
    const btnBackEmpty = document.getElementById('btn-back-empty'); // مسكنا زرار الـ Empty State
    const btnPrint = document.getElementById('btn-print-receipt');

    // عناصر الداتا في الـ UI
    const elTicketNum = document.getElementById('rec-ticket-num');
    const elWaiting = document.getElementById('rec-waiting');
    const elDepartment = document.getElementById('rec-department');
    const elDock = document.getElementById('rec-dock');
    const elEmployee = document.getElementById('rec-employee');
    const elDate = document.getElementById('rec-date');
    const elTime = document.getElementById('rec-time');

    const serverData = window.serverReceipt;
    const rawData = localStorage.getItem('receiptData');

    if (!serverData && !rawData) {
        // حالة عدم وجود بيانات
        emptyState.classList.remove('d-none');

        // تفعيل زر الرجوع في حالة الـ Empty State
        if (btnBackEmpty) {
            btnBackEmpty.addEventListener('click', () => {
                window.location.href = window.routes?.viewRole || window.routes?.backRoute || '/Authentication/viewRole';
            });
        }
        return;
    }

    // إظهار حاوية الإيصال
    receiptMain.classList.remove('d-none');
    emptyState.classList.add('d-none');

    const receiptData = serverData || JSON.parse(rawData);

    // دالة لتهيئة التاريخ والوقت
    const formatDateTime = (isoString) => {
        const d = new Date(isoString);

        // التاريخ: D / M / YYYY
        const dateFormatted = `${d.getDate()} / ${d.getMonth() + 1} / ${d.getFullYear()}`;

        // الوقت: hh:mm AM/PM
        const timeFormatted = d.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        });

        return { dateFormatted, timeFormatted };
    };

    // دالة لفك تشفير الرموز الخاصة بـ HTML إن وُجدت
    const decodeHtmlEntities = (text) => {
        if (!text) return '';
        const parser = new DOMParser();
        const dom = parser.parseFromString(
            '<!doctype html><body>' + text,
            'text/html'
        );
        return dom.body.textContent || text;
    };

    const elRequestType = document.getElementById('rec-request-type');

    // تطبيق البيانات على الـ UI
    if (receiptData.ticketNumber) elTicketNum.textContent = receiptData.ticketNumber;
    if (elRequestType && receiptData.requestType) {
        elRequestType.textContent = receiptData.requestType;
    }
    if (receiptData.waitingCount !== undefined) elWaiting.textContent = receiptData.waitingCount;
    if (receiptData.departmentName || receiptData.department) {
        elDepartment.textContent = decodeHtmlEntities(receiptData.departmentName || receiptData.department);
    }
    if (receiptData.dockName || receiptData.dockNumber) {
        elDock.textContent = receiptData.dockName || receiptData.dockNumber;
    }

    const empName = decodeHtmlEntities(receiptData.employeeName);
    const loggedInName = window.currentUserName || 'المسؤول';
    elEmployee.textContent = (empName && empName !== '--' && empName !== 'موظف الاستقبال') ? empName : loggedInName;

    // تطبيق الوقت
    if (receiptData.createdAt) {
        const { dateFormatted, timeFormatted } = formatDateTime(receiptData.createdAt);
        elDate.textContent = dateFormatted;
        elTime.textContent = timeFormatted;
    }

    // إعداد أزرار الأكشن
    if (btnBack) {
        btnBack.addEventListener('click', () => {
            if (window.routes && window.routes.viewRole) {
                window.location.href = window.routes.viewRole;
            } else if (window.routes && window.routes.backRoute) {
                window.location.href = window.routes.backRoute;
            } else {
                window.location.href = window.routes?.driversPage || '/Driver/Index';
            }
        });
    }

    if (btnPrint) {
        btnPrint.addEventListener('click', () => {
            window.print();
        });
    }

    // أول ما صفحة الريسيبت تفتح، تطبع نفسها تلقائياً
    setTimeout(() => {
        window.print();
    }, 500);
});