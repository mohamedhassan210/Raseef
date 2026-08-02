document.addEventListener('DOMContentLoaded', () => {
    
    // Placeholder Base64 image function to ensure the UI doesn't break if images are missing
    const getPlaceholder = (text) => `data:image/svg+xml;charset=UTF-8,%3Csvg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 100 100'%3E%3Crect fill='%23F9F2F2' width='100' height='100' rx='15'/%3E%3Ctext fill='%23EB842D' font-family='sans-serif' font-size='20' font-weight='bold' x='50' y='55' text-anchor='middle' dominant-baseline='middle'%3E${encodeURIComponent(text)}%3C/text%3E%3C/svg%3E`;

    // 1. Data Structure (Mock Database) matching the design exactly
    const suppliersData = [
        { 
            id: 1,  
            name: 'شركة جهينة', 
            phone: '01005568324', 
            logo: 'assets/juhayna.png' // Replace with actual logo path
        },
        { 
            id: 2, 
            name: 'شركة حلواني اخوان', 
            phone: '01005568324', 
            logo: 'assets/halwani.png'
        },
        {  
            id: 3, 
            name: 'شركة اكوافينا', 
            phone: '01005568324', 
            logo: 'assets/aquafina.png'
        },
        { 
            id: 4, 
            name: 'شركة المكتبة الرقمية', 
            phone: '01005568324', 
            logo: 'assets/digital-library.png'
        },
        { 
            id: 5, 
            name: 'شركة جهينة', 
            phone: '01005568324', 
            logo: 'assets/juhayna.png'
        }
    ];

    // 2. DOM Elements
    const supplierListContainer = document.getElementById('supplierList');
    const searchInput = document.getElementById('searchInput');

    // 3. Render Function
    const renderSuppliers = (data) => {
        // Clear current list
        supplierListContainer.innerHTML = '';

        // Handle empty state
        if (data.length === 0) {
            supplierListContainer.innerHTML = `
                <div class="no-results fade-in">
                    <i class="bi bi-search" style="font-size: 2rem; color: var(--primary-color); display: block; margin-bottom: 10px;"></i>
                    عفواً، لا يوجد موردين بهذا الاسم.
                </div>
            `;
            return;
        }

        // Generate Cards
        data.forEach((supplier, index) => {
            const card = document.createElement('div');
            card.className = 'supplier-card';
            
            // Add a slight animation delay for list loading effect
            card.style.animation = `fadeInUp 0.4s ease forwards ${index * 0.05}s`;
            card.style.opacity = '0';

            card.innerHTML = `
                <div class="card-right">
                    <div class="logo-box">
                        <img src="${supplier.logo}" alt="${supplier.name}" 
                             onerror="this.onerror=null; this.src='${getPlaceholder(supplier.name.split(' ')[1] || supplier.name)}'">
                    </div>
                    <div class="supplier-info">
                        <h3 class="supplier-name">${supplier.name}</h3>
                        <p class="supplier-phone">
                            <i class="bi bi-telephone-fill"></i>
                            <span dir="ltr">${supplier.phone}</span>
                        </p>
                    </div>
                </div>
                <div class="card-left">
                    <button class="btn-select" onclick="selectSupplier(${supplier.id}, '${supplier.name}')">
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
            return supplier.name.toLowerCase().includes(searchTerm) || 
                   supplier.phone.includes(searchTerm);
        });

        renderSuppliers(filteredSuppliers);
    };

    // 5. Event Listeners
    searchInput.addEventListener('input', handleSearch);

    // Initial Render
    renderSuppliers(suppliersData);
    
    // Add Keyframes for JS animations dynamically
    const styleSheet = document.createElement("style");
    styleSheet.innerText = `
        @keyframes fadeInUp {
            from { opacity: 0; transform: translateY(15px); }
            to { opacity: 1; transform: translateY(0); }
        }
    `;
    document.head.appendChild(styleSheet);
});

// Global Function for button click simulation
window.selectSupplier = (id, name) => {
    // You can replace this with your actual business logic
    console.log(`تم اختيار: ${name} (ID: ${id})`);
    
    // Optional: Visual feedback for clicking
    const btn = event.currentTarget;
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="bi bi-check-circle-fill"></i> تم الاختيار';
    btn.style.backgroundColor = '#332D24'; // Dark text color from palette
    btn.style.color = '#fff';
    
    setTimeout(() => {
        btn.innerHTML = originalText;
        btn.style.backgroundColor = ''; 
        btn.style.color = '';
    }, 3000);

    window.location.href = 'companyCars.html';
};