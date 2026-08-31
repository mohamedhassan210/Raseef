document.addEventListener("DOMContentLoaded", function () {
    const filterButtons = document.querySelectorAll(".filter-btn");
    const searchInput = document.getElementById("searchInput");
    const searchBtn = document.querySelector(".search-icon");
    const roleCards = document.querySelectorAll(".role-card");
    const sectionTitle = document.querySelector(".section-title");

    let currentFilter = "الكل";

    // دالة توحيد وتنظيف النصوص العربية والأرقام
    function normalizeText(text) {
        if (!text) return "";
        return text.toString()
            // توحيد الألف والهمزات والياء والتاء المربوطة
            .replace(/[أإآ]/g, "ا")
            .replace(/ة/g, "ه")
            .replace(/ى/g, "ي")
            .replace(/[\u064B-\u065F]/g, "") // إزالة التشكيل
            // تحويل الأرقام الهندية/المشرقية إلى إنجليزية
            .replace(/[٠-٩]/g, function (d) {
                return d.charCodeAt(0) - 1632;
            })
            // إزالة المسافات الزائدة والحروف الخاصة للمقارنة المرنة
            .replace(/[_\-\/\\]/g, " ")
            .toLowerCase()
            .trim();
    }

    // تصنيف الحالة للكارت
    function getStatusCategory(status) {
        if (!status) return "";
        const s = normalizeText(status);
        if (s.includes("انتظار") || s.includes("طابور") || s.includes("معلق")) return "waiting";
        if (s.includes("جاري") || s.includes("تنفيذ") || s.includes("تشغيل")) return "active";
        if (s.includes("تم") || s.includes("مكتمل") || s.includes("منتهي") || s.includes("خروج")) return "done";
        return s;
    }

    // تصنيف الفلتر النشط
    function getFilterCategory(filter) {
        if (!filter || filter === "الكل") return "all";
        const f = normalizeText(filter);
        if (f.includes("انتظار")) return "waiting";
        if (f.includes("جاري")) return "active";
        if (f.includes("تم") || f.includes("مكتمل")) return "done";
        return f;
    }

    // 1. تفعيل أزرار الفلترة
    filterButtons.forEach(button => {
        button.addEventListener("click", function (e) {
            e.preventDefault();
            filterButtons.forEach(btn => btn.classList.remove("active"));
            this.classList.add("active");

            currentFilter = this.getAttribute("data-filter") ? this.getAttribute("data-filter").trim() : "الكل";
            filterCards();
        });
    });

    // 2. تفعيل البحث الفوري والضغط على Enter
    if (searchInput) {
        searchInput.addEventListener("input", filterCards);
        searchInput.addEventListener("keyup", function (e) {
            if (e.key === "Enter") {
                filterCards();
            }
        });
    }

    // زر أيقونة البحث
    if (searchBtn) {
        searchBtn.addEventListener("click", function (e) {
            e.preventDefault();
            filterCards();
        });
    }

    // دالة الفلترة والبحث المجمعة
    function filterCards() {
        const rawSearch = searchInput ? searchInput.value : "";
        const normSearch = normalizeText(rawSearch);
        const searchWords = normSearch.split(" ").filter(w => w.length > 0);
        const filterCat = getFilterCategory(currentFilter);

        let visibleCount = 0;

        roleCards.forEach(card => {
            const cardStatus = card.getAttribute("data-status") || "";
            const searchAttr = card.getAttribute("data-search") || "";
            const cardText = card.innerText || ""; // قراءة جميع النصوص الظاهرة كـ Fallback

            const fullCardText = normalizeText(searchAttr + " " + cardText);
            const cardCat = getStatusCategory(cardStatus);

            // 1. مطابقة الفلتر
            const matchesFilter = (filterCat === "all") || (cardCat === filterCat);

            // 2. مطابقة الكلمات المفتاحية بالكامل (AND Search)
            let matchesSearch = true;
            if (searchWords.length > 0) {
                matchesSearch = searchWords.every(word => fullCardText.includes(word));
            }

            // إظهار أو إخفاء العنصر
            if (matchesFilter && matchesSearch) {
                card.style.setProperty("display", "grid", "important");
                visibleCount++;
            } else {
                card.style.setProperty("display", "none", "important");
            }
        });

        // تحديث عداد قائمة الأدوار في العنوان إذا وُجد
        if (sectionTitle) {
            sectionTitle.textContent = `قائمة الأدوار (${visibleCount})`;
        }

        // تحديث أرقام الكروت الإحصائية الحقيقية
        updateStats();
    }

    function updateStats() {
        const waitSpan = document.getElementById("waitCount");
        const activeSpan = document.getElementById("activeCount");
        const doneSpan = document.getElementById("doneCount");

        let wait = 0, active = 0, done = 0;

        roleCards.forEach(card => {
            const cardStatus = card.getAttribute("data-status") || "";
            const cat = getStatusCategory(cardStatus);
            if (cat === "waiting") wait++;
            else if (cat === "active") active++;
            else if (cat === "done") done++;
        });

        if (waitSpan) waitSpan.textContent = wait;
        if (activeSpan) activeSpan.textContent = active;
        if (doneSpan) doneSpan.textContent = done;
    }

    // تشغيل الفلترة المبدئية عند التحميل
    filterCards();
});