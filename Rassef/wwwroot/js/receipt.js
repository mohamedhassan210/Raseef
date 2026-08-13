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

    // تطبيق البيانات على الـ UI
    elTicketNum.textContent = receiptData.ticketNumber || '--';
    elWaiting.textContent = receiptData.waitingCount || '--';
    elDepartment.textContent = receiptData.department || '--';
    elDock.textContent = receiptData.dockNumber || '--';
    elEmployee.textContent = receiptData.employeeName || '--';

    // تطبيق الوقت
    if (receiptData.createdAt) {
        const { dateFormatted, timeFormatted } = formatDateTime(receiptData.createdAt);
        elDate.textContent = dateFormatted;
        elTime.textContent = timeFormatted;
    }

    // إعداد أزرار الأكشن
    btnBack.addEventListener('click', () => {
        // هنا بيقرأ المسار الديناميكي اللي جياله من الـ CSHTML
        window.location.href = window.routes?.driversPage || '/Driver/Index';
    });

    btnPrint.addEventListener('click', () => {
        window.print();
    });


    // تطبيق الوقت
    if (receiptData.createdAt) {
        const { dateFormatted, timeFormatted } = formatDateTime(receiptData.createdAt);
        elDate.textContent = dateFormatted;
        elTime.textContent = timeFormatted;
    }

    // إعداد أزرار الأكشن
    btnBack.addEventListener('click', () => {
        window.location.href = window.routes?.driversPage || '/Driver/Index';
    });

    btnPrint.addEventListener('click', () => {
        window.print();
    });

    // 👇 الإضافة الجديدة: أول ما صفحة الريسيبت تفتح وتظهر، تطبع نفسها تلقائياً
    setTimeout(() => {
        window.print();
    }, 500); // تأخير نص ثانية عشان نضمن إن الصفحة رسمت نفسها والبيانات ظهرت قبل ما نافذة الطباعة تفتح
});