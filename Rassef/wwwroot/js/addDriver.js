document.addEventListener('DOMContentLoaded', () => {

    // ============================================
    // 1. DOM Elements
    // ============================================

    const form = document.getElementById('addDriverForm');

    const editCompanyBtn = document.getElementById('editCompanyBtn');
    const companyStaticView = document.getElementById('companyStaticView');
    const companySelectView = document.getElementById('companySelectView');
    const companySelect = document.getElementById('companySelect');
    const companyDisplay = document.getElementById('companyDisplay');

    const driverName = document.getElementById('driverName');
    const driverPhone = document.getElementById('driverPhone');
    const nationalId = document.getElementById('nationalId');


    // ============================================
    // 2. Custom Dropdown - شركة السائق
    // ============================================

    const customDropdown = document.getElementById('customDropdown');

    const dropdownHeader = customDropdown.querySelector('.dropdown-header');

    const selectedValue = customDropdown.querySelector('.selected-value');

    const dropdownItems = customDropdown.querySelectorAll('.dropdown-item');


    // ============================================
    // 3. Custom Dropdown - الشركة الجديدة
    // ============================================

    const newCompanyTypeDropdown =
        document.getElementById('newCompanyTypeDropdown');

    const newCompanyTypeHeader =
        document.getElementById('newCompanyTypeHeader');

    const newCompanySelectedValue =
        document.getElementById('newCompanySelectedValue');

    const newCompanyItems =
        newCompanyTypeDropdown.querySelectorAll('.dropdown-item');

    const newCompanyTypeSelect =
        document.getElementById('newCompanyTypeSelect');


    // ============================================
    // 4. Toggle First Dropdown
    // شركة السائق
    // ============================================

    dropdownHeader.addEventListener('click', (e) => {

        e.stopPropagation();

        // إغلاق Dropdown الشركة الجديدة
        newCompanyTypeDropdown.classList.remove('open');

        // فتح / إغلاق Dropdown شركة السائق
        customDropdown.classList.toggle('open');
    });


    // ============================================
    // 5. Select Item - First Dropdown
    // شركة السائق
    // ============================================

    dropdownItems.forEach(item => {

        item.addEventListener('click', (e) => {

            e.stopPropagation();

            const value = item.getAttribute('data-value');

            // تحديث القيمة الظاهرة
            selectedValue.textContent = value;

            // تحديث الـ Native Select
            companySelect.value = value;

            // إزالة selected من كل العناصر
            dropdownItems.forEach(el => {
                el.classList.remove('selected');
            });

            // تحديد العنصر الحالي
            item.classList.add('selected');

            // إغلاق Dropdown
            customDropdown.classList.remove('open');
        });
    });


    // ============================================
    // 6. Toggle Second Dropdown
    // الشركة الجديدة
    // ============================================

    newCompanyTypeHeader.addEventListener('click', (e) => {

        e.stopPropagation();

        // إغلاق Dropdown شركة السائق
        customDropdown.classList.remove('open');

        // فتح / إغلاق Dropdown الشركة الجديدة
        newCompanyTypeDropdown.classList.toggle('open');
    });


    // ============================================
    // 7. Select Item - Second Dropdown
    // الشركة الجديدة
    // ============================================

    newCompanyItems.forEach(item => {

        item.addEventListener('click', (e) => {

            e.stopPropagation();

            const value = item.getAttribute('data-value');

            // تحديث القيمة الظاهرة
            newCompanySelectedValue.textContent = value;

            // إزالة text-muted بعد الاختيار
            newCompanySelectedValue.classList.remove('text-muted');

            // تحديث الـ Native Select
            newCompanyTypeSelect.value = value;

            // إزالة selected من كل العناصر
            newCompanyItems.forEach(el => {
                el.classList.remove('selected');
            });

            // تحديد العنصر الحالي
            item.classList.add('selected');

            // إغلاق Dropdown
            newCompanyTypeDropdown.classList.remove('open');
        });
    });


    // ============================================
    // 8. Close Dropdowns on Outside Click
    // ============================================

    document.addEventListener('click', (e) => {

        // إغلاق Dropdown شركة السائق
        if (!customDropdown.contains(e.target)) {
            customDropdown.classList.remove('open');
        }

        // إغلاق Dropdown الشركة الجديدة
        if (!newCompanyTypeDropdown.contains(e.target)) {
            newCompanyTypeDropdown.classList.remove('open');
        }
    });


    // ============================================
    // 9. Logic to Toggle Company Selection
    // ============================================

    const toggleCompanySelection = () => {

        // إخفاء العرض الثابت
        companyStaticView.classList.add('d-none');

        // إظهار Dropdown شركة السائق
        companySelectView.classList.remove('d-none');

        // تفعيل الـ Select للـ Validation
        companySelect.disabled = false;
    };


    // ============================================
    // 10. Create Driver Object
    // ============================================

    const createDriverObject = () => {

        const selectedCompany =
            companyStaticView.classList.contains('d-none')
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


    // ============================================
    // 11. Save Driver to LocalStorage
    // ============================================

    const saveDriver = (driver) => {

        const existingDrivers =
            JSON.parse(localStorage.getItem('drivers')) || [];

        existingDrivers.push(driver);

        localStorage.setItem(
            'drivers',
            JSON.stringify(existingDrivers)
        );
    };


    // ============================================
    // 12. Redirect on Success
    // ============================================

    const redirectToDrivers = () => {

        window.location.href = window.appRoutes.companyDrivers;
    };


    // ============================================
    // 13. Form Validation & Submit
    // ============================================

    const validateForm = (event) => {

        event.preventDefault();

        // التحقق من صحة البيانات
        if (!form.checkValidity()) {

            event.stopPropagation();

            form.classList.add('was-validated');

            return;
        }

        // إنشاء بيانات السائق
        const newDriver = createDriverObject();

        // حفظ البيانات
        saveDriver(newDriver);

        // الانتقال للصفحة التالية
        redirectToDrivers();
    };


    // ============================================
    // 14. Initialization
    // ============================================

    const initializePage = () => {

        // زر تعديل شركة السائق
        editCompanyBtn.addEventListener(
            'click',
            toggleCompanySelection
        );

        // Submit Form
        form.addEventListener(
            'submit',
            validateForm
        );
    };


    // ============================================
    // 15. Start
    // ============================================

    initializePage();

});