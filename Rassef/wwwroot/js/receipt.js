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

    // جلب البيانات من LocalStorage
    const rawData = localStorage.getItem('receiptData');

    if (!rawData) {
        // حالة عدم وجود بيانات
        emptyState.classList.remove('d-none');

        // تفعيل زر الرجوع في حالة الـ Empty State
        if (btnBackEmpty) {
            btnBackEmpty.addEventListener('click', () => {
                window.location.href = window.routes?.driversPage || '/Driver/Index';
            });
        }
        return;
    }

    // إظهار حاوية الإيصال
    receiptMain.classList.remove('d-none');

    const receiptData = JSON.parse(rawData);

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

    // تطبيق البيانات على الـ UI
    elTicketNum.textContent = receiptData.ticketNumber || '--';
    elWaiting.textContent = receiptData.waitingCount || '--';
    elDepartment.textContent = decodeHtmlEntities(receiptData.department) || '--';
    elDock.textContent = receiptData.dockNumber || '--';

    const empName = decodeHtmlEntities(receiptData.employeeName);
    elEmployee.textContent = (empName && empName !== '--') ? empName : 'موظف الاستقبال';

    // تطبيق الوقت
    if (receiptData.createdAt) {
        const { dateFormatted, timeFormatted } = formatDateTime(receiptData.createdAt);
        elDate.textContent = dateFormatted;
        elTime.textContent = timeFormatted;
    }

    // إعداد أزرار الأكشن
    if (btnBack) {
        btnBack.addEventListener('click', () => {
            window.location.href = window.routes?.driversPage || '/Driver/Index';
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