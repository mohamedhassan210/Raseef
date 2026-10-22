// تصدير Excel موحّد لأي صفحة فيها جدول.
// الصفحة بتجهّز عناوين الأعمدة والصفوف الظاهرة (بعد الفلاتر) ووصف الفلاتر، والسيرفر
// (ReportExport/Table) بيرجّع ملف .xlsx بنفس الـ Header والـ Footer الموحّدين:
//   Header : عنوان التقرير + المخزن + الفلاتر المطبّقة (لو فيه)
//   Footer : عدد السجلات + محرّر التقرير (اللي عامل login) + تاريخ ووقت الإصدار
//
// الاستخدام:
//   ExcelReport.exportObjects({ title, fileName, rows: [{col: val, ...}], filterLines: [...] });
//   ExcelReport.exportTable({ title, fileName, headers: [...], rows: [[...], ...], filterLines: [...] });
//
// بديل مكتبة SheetJS من الـ CDN (اللي كانت بتفشل لو السيرفر مفيهوش إنترنت).
window.ExcelReport = (function () {
    const DEFAULT_URL = '/ReportExport/Table';
    const busy = {};

    const pad = (n) => String(n).padStart(2, '0');
    const stamp = () => {
        const d = new Date();
        return `${d.getFullYear()}${pad(d.getMonth() + 1)}${pad(d.getDate())}_${pad(d.getHours())}${pad(d.getMinutes())}`;
    };

    const endpoint = () =>
        (window.appRoutes && window.appRoutes.reportExportTable) || DEFAULT_URL;

    /** سطر "بحث: ..." لو فيه نص بحث فعلاً، وإلا مصفوفة فاضية. */
    function searchFilterLines(term) {
        const t = (term == null ? '' : String(term)).trim();
        return t ? ['بحث: ' + t] : [];
    }

    async function readError(response) {
        let message = null;
        let bodySnippet = '';
        try {
            const text = await response.text();
            bodySnippet = text.slice(0, 500);
            try {
                const json = JSON.parse(text);
                if (json && json.message) message = json.message;
            } catch (e) { /* الرد مش JSON (مثلاً صفحة خطأ HTML) */ }
        } catch (e) { /* مفيش body */ }

        if (!message) {
            if (response.status === 401) message = 'انتهت الجلسة. يرجى تسجيل الدخول مرة أخرى.';
            else if (response.status === 403) message = 'ليس لديك صلاحية لتصدير هذا الملف.';
            else message = `تعذر إنشاء ملف الإكسل (خطأ ${response.status}). يرجى المحاولة مرة أخرى؛ إذا تكررت المشكلة أبلغ الدعم الفني.`;
        }

        // بنسجّل التفاصيل في الـ Console عشان أي عطل يبان سببه بسرعة
        console.error('[ExcelReport] export failed', response.status, response.url, bodySnippet);
        return message;
    }

    /**
     * options: { title, sheetName?, fileName?, headers: string[], rows: any[][],
     *            filterLines?: string[], includeWarehouse?: boolean }
     */
    async function exportTable(options) {
        const rows = options.rows || [];
        if (!options.headers || options.headers.length === 0 || rows.length === 0) {
            alert('لا توجد بيانات لتصديرها.');
            return false;
        }

        // بعض صفحات اللوحة فيها أكتر من نسخة من كود التصدير بتتربط على نفس الزرار؛
        // الحماية دي بتمنع تنزيل نفس الملف أكتر من مرة بضغطة واحدة.
        const key = options.fileName || options.title;
        if (busy[key]) return false;
        busy[key] = true;
        setTimeout(() => { busy[key] = false; }, 2000);

        try {
            const response = await fetch(endpoint(), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'same-origin',
                body: JSON.stringify({
                    title: options.title,
                    sheetName: options.sheetName || options.title,
                    fileName: options.fileName || options.title,
                    includeWarehouse: options.includeWarehouse !== false,
                    filterLines: options.filterLines || [],
                    headers: options.headers,
                    rows: rows
                })
            });

            if (!response.ok) {
                alert(await readError(response));
                return false;
            }

            const blob = await response.blob();
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `${options.fileName || options.title}_${stamp()}.xlsx`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            setTimeout(() => URL.revokeObjectURL(url), 1000);
            return true;
        } catch (e) {
            console.error('[ExcelReport] network error', e);
            alert('تعذر الاتصال بالخادم. يرجى المحاولة مرة أخرى.');
            return false;
        }
    }

    /** نفس exportTable بس الصفوف بتيجي كـ objects (أول object بيحدد الأعمدة). */
    function exportObjects(options) {
        const objects = options.rows || [];
        if (objects.length === 0) return exportTable(Object.assign({}, options, { headers: [], rows: [] }));
        const headers = Object.keys(objects[0]);
        const rows = objects.map(o => headers.map(h => (o[h] === undefined ? null : o[h])));
        return exportTable(Object.assign({}, options, { headers, rows }));
    }

    return { exportTable, exportObjects, searchFilterLines };
})();
