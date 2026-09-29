// تصدير Excel لصفحتي طلبات التوريد / التحويل.
// الصفحة بتفلتر الكروت عند الـ Browser، فهنا بنجمع الطلبات الظاهرة فعلاً + الفلاتر
// المطبّقة (تاريخ / قسم / بحث) ونبعتهم للسيرفر، والسيرفر بيرجّع ملف .xlsx كامل
// (موظف الحجز، موظف الخدمة، مدة الانتظار، مدة الخدمة + Header وFooter).
window.RequestsExport = (function () {
    const pad = (n) => String(n).padStart(2, '0');

    // yyyy-MM-dd بالتوقيت المحلي (toISOString بيرجّع UTC وممكن يبوّظ اليوم)
    const fmt = (d) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;

    /**
     * بيحوّل فلتر التاريخ النشط لـ { mode, from, to } بنفس منطق الفلترة اللي في الصفحة.
     * "تحرير" بدون أي تاريخ مدخل = مفيش فلتر تاريخ (mode = all).
     */
    function buildDateState(activeFilter, fromValue, toValue) {
        const now = new Date();

        if (activeFilter === 'today') {
            return { mode: 'today', from: fmt(now), to: fmt(now) };
        }
        if (activeFilter === 'week') {
            const start = new Date(now);
            start.setDate(start.getDate() - 7);
            return { mode: 'week', from: fmt(start), to: fmt(now) };
        }
        if (activeFilter === 'month') {
            const first = new Date(now.getFullYear(), now.getMonth(), 1);
            const last = new Date(now.getFullYear(), now.getMonth() + 1, 0);
            return { mode: 'month', from: fmt(first), to: fmt(last) };
        }
        if (activeFilter === 'custom') {
            const from = fromValue || '';
            const to = toValue || '';
            if (!from && !to) return { mode: 'all', from: '', to: '' };
            return { mode: 'custom', from, to };
        }
        return { mode: 'all', from: '', to: '' };
    }

    function init(options) {
        const button = options.button;
        if (!button) return;

        const originalHtml = button.innerHTML;

        const setBusy = (busy) => {
            button.disabled = busy;
            button.style.opacity = busy ? '0.7' : '';
            button.style.pointerEvents = busy ? 'none' : '';
            if (busy) {
                button.innerHTML = '<i class="fas fa-spinner fa-spin"></i><span>جاري التصدير...</span>';
            } else {
                button.innerHTML = originalHtml;
            }
        };

        button.addEventListener('click', async () => {
            const state = options.getState();

            if (!state.ids || state.ids.length === 0) {
                alert('لا توجد طلبات لتصديرها بالفلاتر الحالية.');
                return;
            }

            setBusy(true);
            try {
                const response = await fetch(options.endpoint, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
                    },
                    credentials: 'same-origin',
                    body: JSON.stringify(state)
                });

                if (!response.ok) {
                    let message = 'تعذر تصدير الملف. يرجى المحاولة مرة أخرى.';
                    try {
                        const err = await response.json();
                        if (err && err.message) message = err.message;
                    } catch (e) { /* الرد مش JSON */ }
                    alert(message);
                    return;
                }

                const blob = await response.blob();
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `${options.filePrefix}_${fmt(new Date())}.xlsx`;
                document.body.appendChild(a);
                a.click();
                a.remove();
                setTimeout(() => URL.revokeObjectURL(url), 1000);
            } catch (e) {
                alert('تعذر الاتصال بالخادم. يرجى المحاولة مرة أخرى.');
            } finally {
                setBusy(false);
            }
        });
    }

    return { init, buildDateState };
})();
