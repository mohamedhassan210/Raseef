/* ==========================================================================
   exitGate.js - Exit Gate Checkout Client
   ========================================================================== */

document.addEventListener("DOMContentLoaded", function () {
    const txtCode = document.getElementById("txtTicketCode");
    const btnSearch = document.getElementById("btnSearchCode");
    const previewContainer = document.getElementById("previewContainer");
    const receiptContainer = document.getElementById("receiptContainer");
    const toastAlert = document.getElementById("exitToastAlert");

    function showToast(message, isSuccess = true) {
        if (!toastAlert) return;
        toastAlert.style.display = "flex";
        toastAlert.className = "exit-toast " + (isSuccess ? "toast-success" : "toast-error");
        toastAlert.innerHTML = `<i class="fas ${isSuccess ? 'fa-check-circle' : 'fa-exclamation-triangle'}"></i> <span>${message}</span>`;
        setTimeout(() => {
            toastAlert.style.display = "none";
        }, 5000);
    }

    async function searchTicket() {
        const code = txtCode ? txtCode.value.trim() : "";
        if (!code) {
            showToast("يرجى كتابة أو مسح كود التذكرة أولاً.", false);
            return;
        }

        btnSearch.disabled = true;
        btnSearch.innerHTML = `<i class="fas fa-spinner fa-spin"></i>`;
        previewContainer.style.display = "none";
        receiptContainer.style.display = "none";

        try {
            const res = await fetch(`/QueueTicket/SearchTicketForCheckout?query=${encodeURIComponent(code)}`);
            const data = await res.json();
            if (data.success) {
                renderPreview(data);
            } else {
                showToast(data.message || "لم يتم العثور على التذكرة.", false);
            }
        } catch (err) {
            console.error(err);
            showToast("حدث خطأ أثناء البحث عن التذكرة.", false);
        } finally {
            btnSearch.disabled = false;
            btnSearch.innerHTML = `<i class="fas fa-search"></i> <span>فحص</span>`;
        }
    }

    function renderPreview(data) {
        previewContainer.style.display = "block";
        previewContainer.innerHTML = `
            <div class="preview-header">
                <span class="preview-ticket-num">${data.ticketNumber}</span>
                <span class="preview-status-tag">${data.status}</span>
            </div>

            <div class="preview-grid">
                <div class="preview-item">
                    <span class="label">رقم اللوحة</span>
                    <span class="val" style="direction: ltr; text-align: right;">${data.truckPlate}</span>
                </div>
                <div class="preview-item">
                    <span class="label">اسم السائق</span>
                    <span class="val">${data.driverName}</span>
                </div>
                <div class="preview-item">
                    <span class="label">الجهة / المورد</span>
                    <span class="val">${data.companyOrType} (${data.requestType})</span>
                </div>
                <div class="preview-item">
                    <span class="label">الرصيف والقسم</span>
                    <span class="val">${data.dockName} — ${data.departmentName}</span>
                </div>
                <div class="preview-item">
                    <span class="label">وقت الدخول</span>
                    <span class="val">${data.entryTimeFormatted}</span>
                </div>
                <div class="preview-item">
                    <span class="label">المدة المنقضية</span>
                    <span class="val" style="color: var(--status-done);">${data.durationFormatted}</span>
                </div>
            </div>

            ${data.isAlreadyCompleted ? `
                <div style="background: #E8F5E9; color: #2E7D32; padding: 14px; border-radius: 12px; font-weight: 800; text-align: center; border: 1.5px solid #A5D6A7;">
                    <i class="fas fa-check-circle"></i> هذه التذكرة مكتملة وتم تسجيل خروجها بالفعل سابقاً.
                </div>
            ` : `
                <button id="btnConfirmCheckout" class="btn-confirm-checkout" type="button">
                    <i class="fas fa-check-circle"></i>
                    <span>تأكيد خروج الشاحنة وإنهاء الدور (مكتمل) ✅</span>
                </button>
            `}
        `;

        const btnConfirm = document.getElementById("btnConfirmCheckout");
        if (btnConfirm) {
            btnConfirm.addEventListener("click", () => confirmCheckout(data.ticketNumber));
        }
    }

    async function confirmCheckout(ticketNumber) {
        const btnConfirm = document.getElementById("btnConfirmCheckout");
        if (btnConfirm) {
            btnConfirm.disabled = true;
            btnConfirm.innerHTML = `<i class="fas fa-spinner fa-spin"></i> <span>جاري تسجيل الخروج...</span>`;
        }

        try {
            const res = await fetch('/QueueTicket/CheckoutTicket', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ticketCode: ticketNumber })
            });
            const data = await res.json();
            if (data.success) {
                showToast(data.message, true);
                previewContainer.style.display = "none";
                renderReceipt(data);
                if (txtCode) txtCode.value = "";
            } else {
                showToast(data.message || "حدث خطأ أثناء تأكيد الخروج.", false);
                if (btnConfirm) {
                    btnConfirm.disabled = false;
                    btnConfirm.innerHTML = `<i class="fas fa-check-circle"></i> <span>تأكيد خروج الشاحنة وإنهاء الدور (مكتمل) ✅</span>`;
                }
            }
        } catch (err) {
            console.error(err);
            showToast("حدث خطأ في الاتصال بالخادم.", false);
        }
    }

    function renderReceipt(data) {
        receiptContainer.style.display = "block";
        receiptContainer.innerHTML = `
            <div class="success-icon"><i class="fas fa-check"></i></div>
            <h2>تم إنهاء الدور وخروج الشاحنة بنجاح</h2>
            <p style="color: var(--text-muted); font-weight: 600; margin-bottom: 20px;">تم تسجيل وقت المغادرة وحفظ سجل الخروج في النظام</p>

            <div class="preview-grid" style="text-align: right; max-width: 600px; margin: 0 auto 24px;">
                <div class="preview-item">
                    <span class="label">رقم التذكرة</span>
                    <span class="val" style="color: var(--status-done); font-size: 1.2rem;">${data.ticketNumber}</span>
                </div>
                <div class="preview-item">
                    <span class="label">رقم اللوحة</span>
                    <span class="val" style="direction: ltr; text-align: right;">${data.truckPlate}</span>
                </div>
                <div class="preview-item">
                    <span class="label">السائق</span>
                    <span class="val">${data.driverName}</span>
                </div>
                <div class="preview-item">
                    <span class="label">إجمالي مدة التواجد</span>
                    <span class="val" style="color: var(--status-done);">${data.durationFormatted}</span>
                </div>
            </div>

            <button onclick="window.location.reload()" class="btn-search-code" style="padding: 12px 30px; font-size: 1rem;">
                <i class="fas fa-barcode"></i> <span>مسح تذكرة شاحنة جديدة</span>
            </button>
        `;
    }

    if (btnSearch) {
        btnSearch.addEventListener("click", searchTicket);
    }

    if (txtCode) {
        txtCode.addEventListener("keypress", function (e) {
            if (e.key === "Enter") {
                searchTicket();
            }
        });
        txtCode.focus();
    }
});
