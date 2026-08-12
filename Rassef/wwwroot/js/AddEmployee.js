document.addEventListener('DOMContentLoaded', () => {

    // --- 1. Variables & Elements ---
    const form = document.getElementById('addEmployeeForm');
    const customDropdowns = document.querySelectorAll('.custom-dropdown-container');

    // --- 2. Initialize Custom Dropdowns ---
    const initializeDropdowns = () => {
        customDropdowns.forEach(container => {
            const dropdown = container.querySelector('.custom-dropdown');
            const header = dropdown.querySelector('.dropdown-header');
            const list = dropdown.querySelector('.dropdown-list');
            const items = dropdown.querySelectorAll('.dropdown-item');
            const selectedValueText = dropdown.querySelector('.selected-value');
            const hiddenSelect = container.querySelector('.visually-hidden-select');

            // فتح/إغلاق القائمة
            header.addEventListener('click', (e) => {
                e.stopPropagation();
                const isOpen = dropdown.classList.contains('open');

                // إغلاق كل القوائم الأخرى أولاً
                closeAllDropdowns();

                if (!isOpen) {
                    dropdown.classList.add('open');
                    dropdown.setAttribute('aria-expanded', 'true'); 
                    adjustDropdownPosition(dropdown, list);
                }
            });

            // اختيار عنصر
            items.forEach(item => {
                item.addEventListener('click', (e) => {
                    e.stopPropagation();
                    const value = item.getAttribute('data-value');

                    // تحديث النص
                    selectedValueText.textContent = value;
                    selectedValueText.classList.remove('text-muted');

                    // تحديث الـ Select المخفي
                    hiddenSelect.value = value;

                    // إزالة الكلاس من الباقي وإضافته للمختار
                    items.forEach(el => el.classList.remove('selected'));
                    item.classList.add('selected');

                    // معالجة الـ Validation UI فوراً عند الاختيار
                    if (form.classList.contains('was-validated')) {
                        dropdown.classList.remove('is-invalid-dropdown');
                    }

                    // إغلاق القائمة
                    dropdown.classList.remove('open');
                    dropdown.setAttribute('aria-expanded', 'false');
                });
            });
        });

        // إغلاق القوائم عند الضغط خارجها
        document.addEventListener('click', () => closeAllDropdowns());
    };

    const closeAllDropdowns = () => {
        document.querySelectorAll('.custom-dropdown.open').forEach(dropdown => {
            dropdown.classList.remove('open');
            dropdown.setAttribute('aria-expanded', 'false');
        });
    };

    // منع خروج القائمة خارج الشاشة للأسفل (Smart Positioning)
    const adjustDropdownPosition = (dropdown, list) => {
        const rect = dropdown.getBoundingClientRect();
        const listHeight = 220; // Max height in CSS
        const spaceBelow = window.innerHeight - rect.bottom;

        if (spaceBelow < listHeight && rect.top > listHeight) {
            list.style.top = 'auto';
            list.style.bottom = 'calc(100% + 6px)';
            list.style.transformOrigin = 'bottom center';
        } else {
            list.style.top = 'calc(100% + 6px)';
            list.style.bottom = 'auto';
            list.style.transformOrigin = 'top center';
        }
    };

    // --- 3. Form Validation Logic ---
    const initializeValidation = () => {
        // Validation لحظي أثناء الكتابة أو عند فقدان التركيز
        const inputs = form.querySelectorAll('input');
        inputs.forEach(input => {
            input.addEventListener('blur', () => {
                if (form.classList.contains('was-validated')) {
                    input.checkValidity();
                }
            });
        });
    };

    const validateForm = (event) => {
        event.preventDefault();

        let isValid = true;

        // التحقق من الحقول المخفية (الـ Selects المربوطة بالـ Custom Dropdowns)
        customDropdowns.forEach(container => {
            const hiddenSelect = container.querySelector('.visually-hidden-select');
            const dropdown = container.querySelector('.custom-dropdown');

            if (!hiddenSelect.value) {
                dropdown.classList.add('is-invalid-dropdown');
                isValid = false;
            } else {
                dropdown.classList.remove('is-invalid-dropdown');
            }
        });

        if (!form.checkValidity() || !isValid) {
            event.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        // إذا نجح الـ Validation
        saveEmployee();
    };

    // --- 4. Data Handling ---
    const createEmployeeObject = () => {
        return {
            id: Date.now(),
            name: document.getElementById('empName').value.trim(),
            phone: document.getElementById('empPhone').value.trim(),
            email: document.getElementById('empEmail').value.trim(),
            nationalId: document.getElementById('empNationalId').value.trim(),
            role: document.getElementById('empRoleSelect').value,
            department: document.getElementById('empDeptSelect').value
        };
    };

    const saveEmployee = () => {
        const newEmployee = createEmployeeObject();
        const existingEmployees = JSON.parse(localStorage.getItem('employees')) || [];
        existingEmployees.push(newEmployee);
        localStorage.setItem('employees', JSON.stringify(existingEmployees));

        redirectToEmployees();
    };

    const redirectToEmployees = () => {
        // استخدام المسار الممرر من Razor View
        if (window.appUrls && window.appUrls.employeeIndex) {
            window.location.href = window.appUrls.employeeIndex;
        } else {
            // كـ Fallback في حالة عدم وجود الـ Routing
            window.location.href = "EmployeesList.html";
        }
    };

    // --- 5. Bootstrapping ---
    initializeDropdowns();
    initializeValidation();
    form.addEventListener('submit', validateForm);

});