/* ==========================================================================
   callStation.js - Next Next Next Live Calling Station Client
   ========================================================================== */

document.addEventListener("DOMContentLoaded", function () {
    const btnNext = document.getElementById("btnSuperNext");
    const deptSelect = document.getElementById("stationDeptSelect");
    const toastAlert = document.getElementById("stationToast");

    function showToast(message, isSuccess = true) {
        if (!toastAlert) return;
        toastAlert.style.display = "flex";
        toastAlert.className = "station-toast " + (isSuccess ? "toast-success" : "toast-error");
        toastAlert.innerHTML = `<i class="fas ${isSuccess ? 'fa-check-circle' : 'fa-exclamation-triangle'}"></i> <span>${message}</span>`;
        setTimeout(() => {
            toastAlert.style.display = "none";
        }, 5000);
    }

    async function refreshStationState() {
        const deptId = deptSelect ? deptSelect.value : "";
        try {
            const res = await fetch(`/QueueTicket/GetStationState?departmentId=${deptId}`);
            const data = await res.json();
            if (data.success) {
                renderStationUI(data);
            }
        } catch (err) {
            console.error("Error refreshing station state:", err);
        }
    }

    function renderStationUI(data) {
        // Update Current In-Progress Slot
        const currentContainer = document.getElementById("currentSlotContainer");
        if (currentContainer) {
            if (data.currentTicket) {
                const t = data.currentTicket;
                currentContainer.innerHTML = `
                    <div class="card-top-tag">
                        <span class="tag-title"><i class="fas fa-truck-loading"></i> الدور الحالي بالرصيف</span>
                        <span class="tag-badge">جاري التنفيذ</span>
                    </div>
                    <div class="hero-ticket-number">${t.ticketNumber}</div>
                    <div class="hero-details-list">
                        <div class="hero-detail-row">
                            <span class="label">الشاحنة</span>
                            <span class="val" style="direction: ltr; text-align: right;">${t.truckNumber}</span>
                        </div>
                        <div class="hero-detail-row">
                            <span class="label">السائق</span>
                            <span class="val">${t.driverName}</span>
                        </div>
                        <div class="hero-detail-row">
                            <span class="label">الرصيف</span>
                            <span class="val" style="color: var(--primary-orange);">${t.dockName}</span>
                        </div>
                        <div class="hero-detail-row">
                            <span class="label">القسم</span>
                            <span class="val">${t.departmentName}</span>
                        </div>
                        <div class="hero-detail-row" style="grid-column: span 2;">
                            <span class="label">الجهة / المورد</span>
                            <span class="val">${t.companyName} (${t.requestType})</span>
                        </div>
                    </div>
                `;
            } else {
                currentContainer.innerHTML = `
                    <div class="card-top-tag">
                        <span class="tag-title"><i class="fas fa-truck-loading"></i> الدور الحالي</span>
                    </div>
                    <div class="empty-slot-msg">لا يوجد دور قيد التفريغ حالياً</div>
                `;
            }
        }

        // Update Next In-Queue Slot
        const nextContainer = document.getElementById("nextSlotContainer");
        if (nextContainer) {
            if (data.nextTicket) {
                const t = data.nextTicket;
                nextContainer.innerHTML = `
                    <div class="card-top-tag">
                        <span class="tag-title"><i class="fas fa-clock"></i> الدور القادم للاستدعاء</span>
                        <span class="tag-badge">في الانتظار ⏳</span>
                    </div>
                    <div class="hero-ticket-number">${t.ticketNumber}</div>
                    <div class="hero-details-list">
                        <div class="hero-detail-row">
                            <span class="label">الشاحنة</span>
                            <span class="val" style="direction: ltr; text-align: right;">${t.truckNumber}</span>
                        </div>
                        <div class="hero-detail-row">
                            <span class="label">السائق</span>
                            <span class="val">${t.driverName}</span>
                        </div>
                        <div class="hero-detail-row">
                            <span class="label">الرصيف المقترح</span>
                            <span class="val" style="color: var(--primary-orange);">${t.dockName}</span>
                        </div>
                        <div class="hero-detail-row">
                            <span class="label">القسم</span>
                            <span class="val">${t.departmentName}</span>
                        </div>
                        <div class="hero-detail-row" style="grid-column: span 2;">
                            <span class="label">الجهة / المورد</span>
                            <span class="val">${t.companyName} (${t.requestType})</span>
                        </div>
                    </div>
                `;
            } else {
                nextContainer.innerHTML = `
                    <div class="card-top-tag">
                        <span class="tag-title"><i class="fas fa-clock"></i> الدور القادم</span>
                    </div>
                    <div class="empty-slot-msg">قائمة الانتظار فارغة حالياً 🥳</div>
                `;
            }
        }

        // Update Stats Counters
        const waitCountEl = document.getElementById("stationWaitCount");
        if (waitCountEl) waitCountEl.textContent = data.waitingCount || 0;

        // Update Upcoming Chips
        const chipsContainer = document.getElementById("upcomingChipsContainer");
        if (chipsContainer) {
            if (data.waitingQueue && data.waitingQueue.length > 0) {
                chipsContainer.innerHTML = data.waitingQueue.map(item => `
                    <div class="queue-chip">
                        <span class="chip-num">${item.ticketNumber}</span>
                        <div class="chip-text">
                            <span class="driver">${item.driverName}</span>
                            <span class="truck">${item.truckNumber} • ${item.dockName}</span>
                        </div>
                    </div>
                `).join('');
            } else {
                chipsContainer.innerHTML = `<span style="color: var(--text-muted); font-size: 0.9rem;">لا توجد أدوار أخرى في الانتظار.</span>`;
            }
        }
    }

    if (btnNext) {
        btnNext.addEventListener("click", async function () {
            btnNext.disabled = true;
            btnNext.innerHTML = `<i class="fas fa-spinner fa-spin"></i> <span>جاري استدعاء الدور...</span>`;

            const deptId = deptSelect ? deptSelect.value : "";
            try {
                const res = await fetch(`/QueueTicket/CallNext?departmentId=${deptId}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });
                const data = await res.json();
                if (data.success) {
                    showToast(`📢 تم استدعاء الدور ${data.ticketNumber} للشاحنة ${data.truckPlate} (السائق: ${data.driverName}) على الرصيف ${data.dockName}`, true);
                    await refreshStationState();
                } else {
                    showToast(data.message || 'لا توجد أي شاحنات في قائمة الانتظار.', false);
                }
            } catch (err) {
                console.error(err);
                showToast('حدث خطأ أثناء استدعاء الدور.', false);
            } finally {
                btnNext.disabled = false;
                btnNext.innerHTML = `<i class="fas fa-step-forward"></i> <span>Next — استدعاء الدور التالي ⏭️</span>`;
            }
        });
    }

    if (deptSelect) {
        deptSelect.addEventListener("change", () => {
            refreshStationState();
        });
    }
});
