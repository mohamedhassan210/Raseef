
//Images 
document.addEventListener('DOMContentLoaded', () => {

    // Placeholder function for fallback images
    const getPlaceholder = (text) => `data:image/svg+xml;charset=UTF-8,%3Csvg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 100 100'%3E%3Crect fill='%23F9F2F2' width='100' height='100' rx='15'/%3E%3Ctext fill='%23EB842D' font-family='sans-serif' font-size='20' font-weight='bold' x='50' y='55' text-anchor='middle' dominant-baseline='middle'%3E${encodeURIComponent(text)}%3C/text%3E%3C/svg%3E`;

    // 1. Fetch real Database Data passed from Razor View (Fallback to empty array if null)
    const suppliersData = window.dbSuppliers || [];

    // 2. DOM Elements
    const supplierListContainer = document.getElementById('supplierList');
    const searchInput = document.getElementById('searchInput');

    // 3. Render Function
    const renderSuppliers = (data) => {
        // Clear current list
        supplierListContainer.innerHTML = '';

        // Handle empty state
        if (!data || data.length === 0) {
            supplierListContainer.innerHTML = `
                <div class="no-results fade-in">
                    <i class="bi bi-search" style="font-size: 2rem; color: var(--primary-color); display: block; margin-bottom: 10px;"></i>
                    عفواً، لا يوجد موردين.
                </div>
            `;
            return;
        }

        // Generate Cards
        data.forEach((supplier, index) => {
            const card = document.createElement('div');
            card.className = 'supplier-card';

            card.style.animation = `fadeInUp 0.4s ease forwards ${index * 0.05}s`;
            card.style.opacity = '0';

            const name = supplier.name || 'مورد';
            const phone = supplier.phone || '';
            const logo = supplier.logo || '';

            card.innerHTML = `
                <div class="card-right">
                    <div class="logo-box">
                        <img src="${logo}" alt="${name}" 
                             onerror="this.onerror=null; this.src='${getPlaceholder(name.split(' ')[1] || name)}'">
                    </div>
                    <div class="supplier-info">
                        <h3 class="supplier-name">${name}</h3>
                        <p class="supplier-phone">
                            <i class="bi bi-telephone-fill"></i>
                            <span dir="ltr">${phone}</span>
                        </p>
                    </div>
                </div>
                <div class="card-left">
                    <button class="btn-select" onclick="selectSupplier(${supplier.id}, '${name}', event)">
                        اختيار الشركة
                    </button>
                </div>
            `;
            supplierListContainer.appendChild(card);
        });
    };

    // 4. Search & Filter Logic
    const handleSearch = (e) => {
        const searchTerm = e.target.value.trim().toLowerCase();

        const filteredSuppliers = suppliersData.filter(supplier => {
            const supplierName = (supplier.name || '').toLowerCase();
            const supplierPhone = supplier.phone || '';
            return supplierName.includes(searchTerm) || supplierPhone.includes(searchTerm);
        });

        renderSuppliers(filteredSuppliers);
    };

    // 5. Event Listeners
    if (searchInput) {
        searchInput.addEventListener('input', handleSearch);
    }

    // Initial Render using database suppliers
    renderSuppliers(suppliersData);

    // Animations
    const styleSheet = document.createElement("style");
    styleSheet.innerText = `
        @keyframes fadeInUp {
            from { opacity: 0; transform: translateY(15px); }
            to { opacity: 1; transform: translateY(0); }
        }
    `;
    document.head.appendChild(styleSheet);
});

// Global Function for selection button
window.selectSupplier = (id, name, event) => {
    console.log(`Selected: ${name} (ID: ${id})`);

    const btn = event.currentTarget;
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="bi bi-check-circle-fill"></i> تم الاختيار';
    btn.style.backgroundColor = '#332D24';
    btn.style.color = '#fff';

    setTimeout(() => {
        btn.innerHTML = originalText;
        btn.style.backgroundColor = '';
        btn.style.color = '';
    }, 3000);

    // Navigate to Truck Controller with supplier ID parameter
    if (window.appRoutes && window.appRoutes.truckIndex) {
        window.location.href = `${window.appRoutes.truckIndex}?supplierId=${id}`;
    }
};