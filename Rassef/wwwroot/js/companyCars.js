document.addEventListener('DOMContentLoaded', () => {

    // 1. Toggle Company Input to Select Dropdown
    const editCompanyBtn = document.getElementById('editCompanyBtn');
    const companyStaticView = document.getElementById('companyStaticView');
    const companySelectView = document.getElementById('companySelectView');
    const companySelect = document.getElementById('companySelect');
    const form = document.getElementById('addCarForm');


    if (
        !editCompanyBtn ||
        !companyStaticView ||
        !companySelectView ||
        !companySelect ||
        !form
    ) {
        return;
    }

    console.log(editCompanyBtn);
    console.log(companyStaticView);
    console.log(companySelectView);
    console.log(companySelect);
    editCompanyBtn.addEventListener('click', () => {
        companyStaticView.classList.add('d-none');
        companySelectView.classList.remove('d-none');
        companySelect.disabled = false;
    });

    // 2. Reusable Custom Dropdown Logic
    const setupCustomDropdown = (dropdownId, selectId) => {
        const customDropdown = document.getElementById(dropdownId);
        if (!customDropdown) return;

        const dropdownHeader = customDropdown.querySelector('.dropdown-header');
        const selectedValue = customDropdown.querySelector('.selected-value');
        const dropdownItems = customDropdown.querySelectorAll('.dropdown-item');
        const nativeSelect = document.getElementById(selectId);

        // Toggle Dropdown Menu
        dropdownHeader.addEventListener('click', (e) => {
            e.stopPropagation();

            // إغلاق أي Dropdown أخرى مفتوحة قبل فتح هذه
            document.querySelectorAll('.custom-dropdown').forEach(d => {
                if (d !== customDropdown) d.classList.remove('open');
            });

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

                // Update Native Hidden Select Value
                nativeSelect.value = value;

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
    };

    // تهيئة الـ Dropdowns الحالية
    setupCustomDropdown('customDropdown', 'companySelect');
    setupCustomDropdown('customDriverDropdown', 'driverSelect');

    // 3. Form Validation & Submission
    form.addEventListener('submit', (event) => {
        event.preventDefault();

        if (!form.checkValidity()) {
            event.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        const selectedCompany = companyStaticView.classList.contains('d-none')
            ? companySelect.value
            : document.getElementById('companyDisplay').value;

        // دمج السائق المختار داخل الأوبجيكت الجديد
        const newTruck = {
            id: Date.now(),
            company: selectedCompany,
            plateLetters: document.getElementById('plateLetters').value.trim(),
            plateNumbers: document.getElementById('plateNumbers').value.trim(),
            storage: document.getElementById('storageCapacity').value,
            type: document.querySelector('input[name="truckType"]:checked').value,
            driver: document.getElementById('driverSelect').value
        };

        const existingTrucks = JSON.parse(localStorage.getItem('trucks')) || [];
        existingTrucks.push(newTruck);
        localStorage.setItem('trucks', JSON.stringify(existingTrucks));

        // تغيير التوجيه ليتوافق مع MVC
        window.location.href = window.routes.truckIndex;
    });
});