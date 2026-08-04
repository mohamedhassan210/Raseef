document.addEventListener('DOMContentLoaded', () => {

    // 1. Toggle Company Input to Select Dropdown
    const editCompanyBtn = document.getElementById('editCompanyBtn');
    const companyStaticView = document.getElementById('companyStaticView');
    const companySelectView = document.getElementById('companySelectView');
    const companySelect = document.getElementById('companySelect');
    const submitbtn = document.getElementById('submit-btn');

    if (editCompanyBtn) {
        editCompanyBtn.addEventListener('click', () => {
            if (companyStaticView) companyStaticView.classList.add('d-none');
            if (companySelectView) companySelectView.classList.remove('d-none');
            if (companySelect) companySelect.disabled = false; // تفعيل الـ select للـ validation
        });
    }

    // 2. Custom Dropdown Logic
    const dropdowns = document.querySelectorAll('.custom-dropdown');

    dropdowns.forEach(customDropdown => {
        const dropdownHeader = customDropdown.querySelector('.dropdown-header');
        const selectedValue = customDropdown.querySelector('.selected-value');
        const dropdownItems = customDropdown.querySelectorAll('.dropdown-item');
        // الوصول للقائمة المخفية الخاصة بكل Dropdown لتحديث الـ Validation
        const nativeSelect = customDropdown.parentElement.querySelector('select.d-none');

        // Toggle Dropdown Menu
        if (dropdownHeader) {
            dropdownHeader.addEventListener('click', (e) => {
                e.stopPropagation();

                // إغلاق أي Dropdown أخرى مفتوحة قبل فتح هذه لتجنب تداخل القوائم
                document.querySelectorAll('.custom-dropdown').forEach(d => {
                    if (d !== customDropdown) d.classList.remove('open');
                });

                customDropdown.classList.toggle('open');
            });
        }

        // Select Item Logic
        dropdownItems.forEach(item => {
            item.addEventListener('click', (e) => {
                e.stopPropagation();

                const value = item.getAttribute('data-value');

                // Update UI Text
                if (selectedValue) {
                    selectedValue.textContent = value;
                    selectedValue.classList.remove('placeholder-color');
                }

                // Update Native Hidden Select Value for Form Extraction
                if (nativeSelect) {
                    nativeSelect.value = value;
                    // إطلاق الحدث يدوياً لكي يشعر به الـ Bootstrap Validation وتختفي العلامة الحمراء
                    nativeSelect.dispatchEvent(new Event('change', { bubbles: true }));
                }

                // Handle active/selected classes visually
                dropdownItems.forEach(el => el.classList.remove('selected'));
                item.classList.add('selected');

                // Close Dropdown
                customDropdown.classList.remove('open');
            });
        });
    });

    // Close Dropdown on outside click
    document.addEventListener('click', (e) => {
        document.querySelectorAll('.custom-dropdown').forEach(customDropdown => {
            if (!customDropdown.contains(e.target)) {
                customDropdown.classList.remove('open');
            }
        });
    });

    // 3. Form Validation & Submission
    const form = document.getElementById('addCarForm');

    if (form) {
        form.addEventListener('submit', (event) => {
            event.preventDefault();

            // التحقق من صحة البيانات بناءً على Bootstrap
            if (!form.checkValidity()) {
                event.stopPropagation();
                form.classList.add('was-validated');
                return;
            }

            // استخراج اسم الشركة سواء من العرض الثابت أو القائمة المنسدلة
            let selectedCompany = "";
            if (companyStaticView && !companyStaticView.classList.contains('d-none')) {
                selectedCompany = document.getElementById('companyDisplay')?.value || "";
            } else if (companySelect) {
                selectedCompany = companySelect.value;
            }

            // تجميع بيانات الشاحنة بشكل آمن (مع جلب قيمة السائق المفقودة)
            const driverSelect = document.getElementById('driverSelect');
            const plateLetters = document.getElementById('plateLetters');
            const plateNumbers = document.getElementById('plateNumbers');
            const storageCapacity = document.getElementById('storageCapacity');
            const truckType = document.querySelector('input[name="truckType"]:checked');

            const newTruck = {
                id: Date.now(),
                company: selectedCompany,
                plateLetters: plateLetters ? plateLetters.value.trim() : "",
                plateNumbers: plateNumbers ? plateNumbers.value.trim() : "",
                storage: storageCapacity ? storageCapacity.value : "",
                type: truckType ? truckType.value : "",
                driver: driverSelect ? driverSelect.value : "" // <-- تم إضافة السائق هنا
            };

            // الحفظ في التخزين المحلي
            const existingTrucks = JSON.parse(localStorage.getItem('trucks')) || [];
            existingTrucks.push(newTruck);
            localStorage.setItem('trucks', JSON.stringify(existingTrucks));

            // قراءة الـ ID من الـ URL لإعادة إرساله
            const urlParams = new URLSearchParams(window.location.search);
            const supplierId = urlParams.get('supplierId');

            // تغيير حالة الزر لمنع تكرار الضغط (UX Enhancement)
            if (submitbtn) {
                submitbtn.textContent = 'جاري الإضافة...';
                submitbtn.disabled = true;
            }

            // التوجيه مع الحفاظ على الـ Route الخاص بـ MVC
            setTimeout(() => {
                if (window.routes && window.routes.truckIndex) {
                    window.location.href = supplierId
                        ? `${window.routes.truckIndex}?supplierId=${supplierId}`
                        : window.routes.truckIndex;
                } else {
                    // Fallback in case window.routes is not defined
                    window.location.href = supplierId
                        ? `companyCars.html?supplierId=${supplierId}`
                        : 'companyCars.html';
                }
            }, 300);
        });
    }
});