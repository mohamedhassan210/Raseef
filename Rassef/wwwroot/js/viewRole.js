document.addEventListener("DOMContentLoaded", function () {
    const filterButtons = document.querySelectorAll(".filter-btn");
    const searchInput = document.getElementById("searchInput");
    const roleCards = document.querySelectorAll(".role-card");

    let currentFilter = "الكل";

    // 1. أزرار الفلترة
    filterButtons.forEach(button => {
        button.addEventListener("click", function () {
            filterButtons.forEach(btn => btn.classList.remove("active"));
            this.classList.add("active");

            currentFilter = this.getAttribute("data-filter").trim();
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
        const searchText = searchInput ? searchInput.value.toLowerCase().trim() : "";

        roleCards.forEach(card => {
            const cardStatus = card.getAttribute("data-status").trim(); // (إنتظار، جاري، تم)
            const searchKeywords = card.getAttribute("data-search").toLowerCase();

            // مطابقة الفلتر
            let matchesFilter = (currentFilter === "الكل") || (cardStatus === currentFilter);

            // مطابقة البحث
            let matchesSearch = searchKeywords.includes(searchText);

            // إظهار أو إخفاء العنصر
            if (matchesFilter && matchesSearch) {
                card.style.display = "flex"; // استخدام flex لضبط محاذاة الكارت الأفقي
            } else {
                card.style.display = "none";
            }
        });
    }
});