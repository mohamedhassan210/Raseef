document.addEventListener("DOMContentLoaded", function () {
    const filterButtons = document.querySelectorAll(".filter-btn");
    const searchInput = document.getElementById("searchInput");
    const roleCards = document.querySelectorAll(".role-card");

    let currentFilter = "الكل";

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
        return f;
    }

    // 1. أزرار الفلترة
    filterButtons.forEach(button => {
        button.addEventListener("click", function (e) {
            e.preventDefault();
            filterButtons.forEach(btn => btn.classList.remove("active"));
            this.classList.add("active");

            currentFilter = this.getAttribute("data-filter") ? this.getAttribute("data-filter").trim() : "الكل";
            filterCards();
        });
    });

    // 2. شريط البحث
    if (searchInput) {
        searchInput.addEventListener("input", function () {
            filterCards();
        });
    }

    // دالة الفلترة الأساسية
    function filterCards() {
        const searchText = searchInput ? searchInput.value.trim() : "";
        const filterCat = getFilterCategory(currentFilter);

        roleCards.forEach(card => {
            const cardStatus = card.getAttribute("data-status") || "";
            const searchKeywords = card.getAttribute("data-search") || "";
            const cardCat = getStatusCategory(cardStatus);

            // مطابقة الفلتر
            let matchesFilter = (filterCat === "all") || (cardCat === filterCat);

            // مطابقة البحث
            let matchesSearch = !searchText || normalizeText(searchKeywords).includes(normalizeText(searchText));

            // إظهار أو إخفاء العنصر
            if (matchesFilter && matchesSearch) {
                card.style.display = ""; // إعادة النمط الافتراضي من الـ CSS (grid)
            } else {
                card.style.display = "none";
            }
        });
    }

    // تشغيل الفلترة المبدئية عند التحميل
    filterCards();
});