document.addEventListener('DOMContentLoaded', () => {
    
    // 1. Toggle Company Input to Select Dropdown
    const editCompanyBtn = document.getElementById('editCompanyBtn');
    const companyStaticView = document.getElementById('companyStaticView');
    const companySelectView = document.getElementById('companySelectView');
    const companySelect = document.getElementById('companySelect'); // Hidden Native Select
    const submitbtn = document.getElementById('submit-btn');

    editCompanyBtn.addEventListener('click', () => {
        companyStaticView.classList.add('d-none');
        companySelectView.classList.remove('d-none');
        companySelect.disabled = false; // تفعيل الـ select للـ validation
    });

    // 2. Custom Dropdown Logic
    const customDropdown = document.getElementById('customDropdown');
    const dropdownHeader = customDropdown.querySelector('.dropdown-header');
    const selectedValue = customDropdown.querySelector('.selected-value');
    const dropdownItems = customDropdown.querySelectorAll('.dropdown-item');

    // Toggle Dropdown Menu
    dropdownHeader.addEventListener('click', (e) => {
        e.stopPropagation();
        customDropdown.classList.toggle('open');
    });

    // Select Item Logic
    dropdownItems.forEach(item => {
        item.addEventListener('click', (e) => {
            e.stopPropagation();
            
            const value = item.getAttribute('data-value');
            
            // Update UI Text
            selectedValue.textContent = value;
            selectedValue.classList.remove('placeholder-color');
            
            // Update Native Hidden Select Value for Form Extraction
            companySelect.value = value;
            
            // Handle active/selected classes visually
            dropdownItems.forEach(el => el.classList.remove('selected'));
            item.classList.add('selected');

            // Close Dropdown
            customDropdown.classList.remove('open');
        });
    });

    // Close Dropdown on outside click
    document.addEventListener('click', (e) => {
        if (!customDropdown.contains(e.target)) {
            customDropdown.classList.remove('open');
        }
    });

    // 3. Form Validation & Submission (Existing Logic Kept Safely)
    const form = document.getElementById('addCarForm');
    
    form.addEventListener('submit', (event) => {
        event.preventDefault();
        
        // التحقق من صحة البيانات بناءً على Bootstrap
        if (!form.checkValidity()) {
            event.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        // استخراج البيانات (الـ Dropdown الجديد بيحدث الـ companySelect المخفي)
        const selectedCompany = companyStaticView.classList.contains('d-none') 
                                ? companySelect.value 
                                : document.getElementById('companyDisplay').value;

        const newTruck = {
            id: Date.now(), 
            company: selectedCompany,
            plateLetters: document.getElementById('plateLetters').value.trim(),
            plateNumbers: document.getElementById('plateNumbers').value.trim(),
            storage: document.getElementById('storageCapacity').value,
            type: document.querySelector('input[name="truckType"]:checked').value 
        };

        const existingTrucks = JSON.parse(localStorage.getItem('trucks')) || [];
        existingTrucks.push(newTruck);
        localStorage.setItem('trucks', JSON.stringify(existingTrucks));

        // الانتقال لصفحة الشاحنات
        window.location.href = "companyCars.html";
    });
});