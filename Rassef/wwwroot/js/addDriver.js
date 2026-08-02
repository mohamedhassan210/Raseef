document.addEventListener('DOMContentLoaded', () => {
    
    // 1. DOM Elements
    const form = document.getElementById('addDriverForm');
    const editCompanyBtn = document.getElementById('editCompanyBtn');
    const companyStaticView = document.getElementById('companyStaticView');
    const companySelectView = document.getElementById('companySelectView');
    const companySelect = document.getElementById('companySelect');
    const companyDisplay = document.getElementById('companyDisplay');
    const driverName = document.getElementById('driverName');
    const driverPhone = document.getElementById('driverPhone');
    const nationalId = document.getElementById('nationalId');

    // 2. Custom Dropdown Elements
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
            
            // Update Native Hidden Select Value for Form Validation
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

    // 3. Logic to toggle Company selection
    const toggleCompanySelection = () => {
        companyStaticView.classList.add('d-none');
        companySelectView.classList.remove('d-none');
        companySelect.disabled = false; // تفعيل الحقل للـ Validation
    };

    // 4. Create structured object from inputs
    const createDriverObject = () => {
        const selectedCompany = companyStaticView.classList.contains('d-none') 
            ? companySelect.value 
            : companyDisplay.value;

        return {
            id: Date.now(),
            name: driverName.value.trim(),
            phone: driverPhone.value.trim(),
            nationalId: nationalId.value.trim(),
            company: selectedCompany
        };
    };

    // 5. Save to LocalStorage
    const saveDriver = (driver) => {
        const existingDrivers = JSON.parse(localStorage.getItem('drivers')) || [];
        existingDrivers.push(driver);
        localStorage.setItem('drivers', JSON.stringify(existingDrivers));
    };

    // 6. Redirect on Success
    const redirectToDrivers = () => {
        window.location.href = "companyDrivers.html";
    };

    // 7. Handle Form Submission & Validation
    const validateForm = (event) => {
        event.preventDefault();
        
        // التحقق من صحة البيانات بالاعتماد على Bootstrap
        if (!form.checkValidity()) {
            event.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        // في حالة صحة البيانات
        const newDriver = createDriverObject();
        saveDriver(newDriver);
        redirectToDrivers();
    };

    // 8. Initialization
    const initializePage = () => {
        editCompanyBtn.addEventListener('click', toggleCompanySelection);
        form.addEventListener('submit', validateForm);
    };

    // Bootstrap execution
    initializePage();
});