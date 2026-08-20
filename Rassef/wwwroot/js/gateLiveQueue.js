/* ==========================================================================
   gateLiveQueue.js - Operator / Gate Control Live Actions
   ========================================================================== */

document.addEventListener("DOMContentLoaded", function () {
    const filterButtons = document.querySelectorAll(".chip-btn");
    const searchInput = document.getElementById("searchTicketsInput");
    const toastContainer = document.getElementById("liveToastAlert");
    const btnCallNext = document.getElementById("btnCallNextTicket");

    let currentFilter = "الكل";

    function showToast(message, isSuccess = true) {
        if (!toastContainer) return;
        toastContainer.style.display = "flex";
        toastContainer.className = "toast-msg-container " + (isSuccess ? "toast-msg-success" : "toast-msg-error");
        toastContainer.innerHTML = `<i class="fas ${isSuccess ? 'fa-check-circle' : 'fa-exclamation-triangle'}"></i> <span>${message}</span>`;
        setTimeout(() => {
            toastContainer.style.display = "none";
        }, 5000);
    }

    function normalizeText(text) {
        if (!text) return "";
        return text.toString()
            .replace(/[أإآ]/g, "ا")
            .replace(/ة/g, "ه")
            .replace(/ى/g, "ي")
            .toLowerCase()
            .trim();
    }

    function getStatusCategory(status) {
        if (!status) return "";
        const s = normalizeText(status);
        if (s.includes("انتظار") || s.includes("طابور") || s.includes("معلق")) return "waiting";
        if (s.includes("جاري") || s.includes("تنفيذ") || s.includes("تشغيل")) return "active";
        if (s.includes("تم") || s.includes("مكتمل") || s.includes("منتهي") || s.includes("خروج")) return "done";
        return s;
    }

    function getFilterCategory(filter) {
        if (!filter || filter === "الكل") return "all";
        const f = normalizeText(filter);
        if (f.includes("انتظار") || f.includes("طابور")) return "waiting";
        if (f.includes("جاري") || f.includes("تنفيذ")) return "active";
        if (f.includes("تم") || f.includes("مكتمل")) return "done";
        if (f.includes("توريد")) return "supply";
        if (f.includes("تحويل")) return "transfer";
        return f;
    }

    filterButtons.forEach(button => {
        button.addEventListener("click", function (e) {
            e.preventDefault();
            filterButtons.forEach(btn => btn.classList.remove("active"));
            this.classList.add("active");

            currentFilter = this.getAttribute("data-filter") ? this.getAttribute("data-filter").trim() : "الكل";
            filterCards();
        });
    });

    if (searchInput) {
        searchInput.addEventListener("input", function () {
            filterCards();
        });
    }

    function filterCards() {
        const searchText = searchInput ? searchInput.value.trim() : "";
        const filterCat = getFilterCategory(currentFilter);
        const cards = document.querySelectorAll(".ticket-card");

        cards.forEach(card => {
            const cardStatus = card.getAttribute("data-status") || "";
            const cardType = card.getAttribute("data-type") || "";
            const searchKeywords = card.getAttribute("data-search") || "";
            const cardCat = getStatusCategory(cardStatus);

            let matchesFilter = false;
            if (filterCat === "all") {
                matchesFilter = true;
            } else if (filterCat === "supply") {
                matchesFilter = cardType.includes("توريد");
            } else if (filterCat === "transfer") {
                matchesFilter = cardType.includes("تحويل");
            } else {
                matchesFilter = (cardCat === filterCat);
            }

            let matchesSearch = !searchText || normalizeText(searchKeywords).includes(normalizeText(searchText));

            if (matchesFilter && matchesSearch) {
                card.style.display = "";
            } else {
                card.style.display = "none";
            }
        });
    }

    window.updateOperatorTicketUI = function (ticketId, targetStatus) {
        const card = document.getElementById(`ticket-card-${ticketId}`);
        if (!card) {
            setTimeout(() => window.location.reload(), 800);
            return;
        }

        const norm = normalizeText(targetStatus);
        const actionsContainer = document.getElementById(`ticket-actions-${ticketId}`);
        const statusPill = card.querySelector(".ticket-status-pill");

        if (norm.includes("جاري") || norm.includes("تنفيذ")) {
            card.setAttribute("data-status", "جاري");
            if (statusPill) {
                statusPill.className = "ticket-status-pill status-pill-active";
                statusPill.innerHTML = `<i class="fas fa-truck-loading"></i> <span>جاري التنفيذ</span>`;
            }
            if (actionsContainer) {
                actionsContainer.innerHTML = `
                    <button class="btn-card-complete" type="button" onclick="updateOperatorStatus(${ticketId}, 'مكتمل')">
                        <i class="fas fa-check-double"></i> <span>إنهاء وتفريغ (مكتمل)</span>
                    </button>
                `;
            }
        } else if (norm.includes("مكتمل") || norm.includes("تم")) {
            card.setAttribute("data-status", "مكتمل");
            if (statusPill) {
                statusPill.className = "ticket-status-pill status-pill-done";
                statusPill.innerHTML = `<i class="fas fa-check-circle"></i> <span>مكتمل</span>`;
            }
            if (actionsContainer) {
                actionsContainer.innerHTML = `
                    <div class="badge-done-label"><i class="fas fa-check-circle"></i> تم الانتهاء بنجاح</div>
                `;
            }
        }

        recalculateStats();
        filterCards();
    };

    function recalculateStats() {
        const cards = document.querySelectorAll(".ticket-card");
        let wait = 0, active = 0, done = 0;

        cards.forEach(c => {
            const cat = getStatusCategory(c.getAttribute("data-status") || "");
            if (cat === "waiting") wait++;
            else if (cat === "active") active++;
            else if (cat === "done") done++;
        });

        const waitEl = document.getElementById("statWaitCount");
        const activeEl = document.getElementById("statActiveCount");
        const doneEl = document.getElementById("statDoneCount");

        if (waitEl) waitEl.textContent = wait;
        if (activeEl) activeEl.textContent = active;
        if (doneEl) doneEl.textContent = done;
    }

    window.updateOperatorStatus = async function (ticketId, targetStatus) {
        try {
            const res = await fetch('/QueueTicket/UpdateStatus', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ticketId: ticketId, status: targetStatus })
            });
            const data = await res.json();
            if (data.success) {
                showToast(data.message || `تم تغيير حالة الدور بنجاح إلى ${targetStatus}`, true);
                window.updateOperatorTicketUI(ticketId, targetStatus);
            } else {
                showToast(data.message || 'تعذر تحديث الحالة.', false);
            }
        } catch (err) {
            console.error(err);
            showToast('حدث خطأ في الاتصال بالخادم.', false);
        }
    };

    if (btnCallNext) {
        btnCallNext.addEventListener("click", async function () {
            btnCallNext.disabled = true;
            btnCallNext.innerHTML = `<i class="fas fa-spinner fa-spin"></i> <span>جاري استدعاء الدور القادم...</span>`;

            try {
                const res = await fetch('/QueueTicket/CallNext', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });
                const data = await res.json();
                if (data.success) {
                    showToast(`📢 تم استدعاء الدور ${data.ticketNumber} للشاحنة ${data.truckPlate} (السائق: ${data.driverName}) على الرصيف ${data.dockName}`, true);
                    if (data.ticketId) {
                        window.updateOperatorTicketUI(data.ticketId, 'جاري');
                    } else {
                        setTimeout(() => window.location.reload(), 1000);
                    }
                } else {
                    showToast(data.message || 'لا توجد أي أدوار في قائمة الانتظار حالياً.', false);
                }
            } catch (err) {
                console.error(err);
                showToast('حدث خطأ أثناء استدعاء الدور.', false);
            } finally {
                btnCallNext.disabled = false;
                btnCallNext.innerHTML = `<i class="fas fa-step-forward"></i> <span>استدعاء الدور التالي (Next ⏭️)</span>`;
            }
        });
    }

    filterCards();
});
