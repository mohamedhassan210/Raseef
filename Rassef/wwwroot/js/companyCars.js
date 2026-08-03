document.addEventListener('DOMContentLoaded', () => {
    // 1. تحديد العناصر الأساسية من الـ DOM
    const searchInput = document.getElementById('search-input');
    const cardsContainer = document.getElementById('cards-container');

    // التأكد من وجود الحاوية لتجنب الأخطاء في الصفحات الأخرى
    if (!cardsContainer) return;

    const cards = cardsContainer.querySelectorAll('.truck-card');
    const selectButtons = document.querySelectorAll('.select-btn');

    // 2. نظام البحث (Filter) للعمل على الكروت المطبوعة من الـ Server
    const handleSearch = () => {
        const query = searchInput.value.trim().toLowerCase();

        cards.forEach(card => {
            const plateText = card.getAttribute('data-plate').toLowerCase();
            const vehicleTitle = card.getAttribute('data-type').toLowerCase();

            // يبحث برقم اللوحة أو نوع الشاحنة
            if (plateText.includes(query) || vehicleTitle.includes(query)) {
                card.style.display = 'flex';
                card.style.animation = 'fadeIn 0.3s ease-in-out';
            } else {
                card.style.display = 'none';
            }
        });
    };

    // تفعيل البحث عند الكتابة
    if (searchInput) {
        searchInput.addEventListener('input', handleSearch);

        // تحسين تجربة المستخدم عند تفريغ حقل البحث (كما طلبت في كودك)
        searchInput.addEventListener('search', () => {
            if (searchInput.value.trim() === '') {
                handleSearch(); // نعيد عرض كل الكروت
            }
        });
    }

    // 3. تفعيل حدث الضغط على زر "اختيار السيارة"
    selectButtons.forEach(button => {
        button.addEventListener('click', (e) => {
            const card = e.target.closest('.truck-card');

            // سحب البيانات من الـ data-attributes التي أضفناها في Razor
            const plate = card.getAttribute('data-plate');
            const truckType = card.getAttribute('data-type');
            const companyName = card.getAttribute('data-company');
            const truckId = card.getAttribute('data-id'); // مهم جداً للربط مع قاعدة البيانات لاحقاً

            // حفظ البيانات في الـ LocalStorage كما طلبت
            const selectedTruck = {
                id: truckId,
                plate: plate,
                company: companyName,
                type: truckType
            };

            localStorage.setItem('selectedTruck', JSON.stringify(selectedTruck));
            console.log(`Car selected: ID: ${truckId}, Plate: ${plate}, Company: ${companyName}, Type: ${truckType}`);

            window.location.href = "/Driver/Index"; // عدل هذا المسار حسب الـ Controller الخاص بالسائقين
        });
    });
});