document.addEventListener('DOMContentLoaded', () => {

    // ============================================
    // 1. DOM Elements (Form & Inputs)
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
    // 2. DOM Elements (Modals)
    // ============================================
    const modalOverlay = document.getElementById('custom-modal-overlay');
    const deptModal = document.getElementById('department-modal');
    const confirmModal = document.getElementById('confirmation-modal');

    const btnConfirmDept = document.getElementById('btn-confirm-dept');
    const btnEdit = document.getElementById('btn-edit');
    const btnConfirm = document.getElementById('btn-confirm');

    const confirmCompany = document.getElementById('confirm-company');
    const confirmDriverName = document.getElementById('confirm-driver-name');
    const confirmDriverId = document.getElementById('confirm-driver-id');
    const confirmDriverdep = document.getElementById('confirm-driver-dep');

    const deptDropdownContainer = document.getElementById('dept-dropdown-container');
    const deptDropdownHeader = document.getElementById('dept-dropdown-header');
    const deptSelectedValue = document.getElementById('dept-selected-value');
    const deptDropdownList = document.getElementById('dept-dropdown-list');
    const deptErrorMsg = document.getElementById('dept-error-msg');

    let selectedDepartmentValue = null;
    let pendingDriverInfo = {};

    // ============================================
    // 3. Custom Dropdown - شركة السائق 
    // ============================================
    const customDropdown = document.getElementById('customDropdown');
    const dropdownHeader = customDropdown.querySelector('.dropdown-header');
    const selectedValue = customDropdown.querySelector('.selected-value');
    const dropdownItems = customDropdown.querySelectorAll('.dropdown-item');

    dropdownHeader.addEventListener('click', (e) => {
        e.stopPropagation();
        customDropdown.classList.toggle('open');
    });

    dropdownItems.forEach(item => {
        item.addEventListener('click', (e) => {
            e.stopPropagation();
            const value = item.getAttribute('data-value');
            const text = item.textContent;

            selectedValue.textContent = text;
            companySelect.value = value;

            dropdownItems.forEach(el => el.classList.remove('selected'));
            item.classList.add('selected');
            customDropdown.classList.remove('open');
        });
    });

    document.addEventListener('click', (e) => {
        if (!customDropdown.contains(e.target)) {
            customDropdown.classList.remove('open');
        }
    });

    const toggleCompanySelection = () => {
        companyStaticView.classList.add('d-none');
        companySelectView.classList.remove('d-none');
        companySelect.disabled = false;

        // إزالة الحقل المخفي الخاص بالـ Id حتى لا يتم إرسال قيمتين للسيرفر
        const hiddenSupplier = document.getElementById('hiddenSupplierId');
        if (hiddenSupplier) hiddenSupplier.remove();
    };

    if (editCompanyBtn) {
        editCompanyBtn.addEventListener('click', toggleCompanySelection);
    }

    // ============================================
    // 4. Modal System Functions
    // ============================================
    const showConfirmationModal = (modalElement) => {
        deptModal.classList.remove('active-modal');
        deptModal.classList.add('d-none');
        confirmModal.classList.remove('active-modal');
        confirmModal.classList.add('d-none');

        modalOverlay.classList.add('active');
        modalElement.classList.remove('d-none');

        setTimeout(() => {
            modalElement.classList.add('active-modal');
        }, 10);
        document.body.style.overflow = 'hidden';
    };

    const closeModal = () => {
        modalOverlay.classList.remove('active');
        deptModal.classList.remove('active-modal');
        confirmModal.classList.remove('active-modal');
        if (deptDropdownContainer) deptDropdownContainer.classList.remove('open');
        document.body.style.overflow = '';
    };

    const populateConfirmationData = (departmentName = "غير متوفر") => {
        // تحديد اسم الشركة المختار
        let selectedCompanyName = "غير متوفر";
        if (!companyStaticView.classList.contains('d-none')) {
            selectedCompanyName = companyDisplay.value;
        } else {
            // جلب النص الخاص بالـ Option المختار من الـ Select
            const selectedOption = companySelect.options[companySelect.selectedIndex];
            if (selectedOption) selectedCompanyName = selectedOption.text;
        }

        confirmCompany.textContent = selectedCompanyName;
        confirmDriverName.textContent = pendingDriverInfo.name || "غير متوفر";
        confirmDriverId.textContent = pendingDriverInfo.nationalId || "غير متوفر";
        confirmDriverdep.textContent = departmentName;
    };

    // ============================================
    // 5. Department Custom Dropdown (للمودال)
    // ============================================
    const initDepartmentDropdown = () => {
        const departmentsData = [
            { id: 101, name: "قسم الاستلام" },
            { id: 102, name: "قسم المخازن" },
            { id: 103, name: "قسم التوزيع" },
            { id: 104, name: "قسم المبيعات" }
        ];

        if (deptDropdownList) {
            deptDropdownList.innerHTML = departmentsData.map(dept =>
                `<div class="dropdown-item" data-id="${dept.id}" data-value="${dept.name}">${dept.name}</div>`
            ).join('');
        }

        const deptItems = deptDropdownList ? deptDropdownList.querySelectorAll('.dropdown-item') : [];

        if (deptDropdownHeader) {
            deptDropdownHeader.addEventListener('click', (e) => {
                e.stopPropagation();
                deptDropdownContainer.classList.toggle('open');
                deptDropdownHeader.classList.remove('error');
                deptErrorMsg.style.display = 'none';
            });
        }

        deptItems.forEach(item => {
            item.addEventListener('click', (e) => {
                e.stopPropagation();
                selectedDepartmentValue = {
                    id: parseInt(item.getAttribute('data-id')),
                    name: item.getAttribute('data-value')
                };
                deptSelectedValue.textContent = selectedDepartmentValue.name;
                deptSelectedValue.classList.remove('text-muted');

                deptItems.forEach(el => el.classList.remove('selected'));
                item.classList.add('selected');

                deptDropdownContainer.classList.remove('open');
                deptDropdownHeader.classList.remove('error');
                deptErrorMsg.style.display = 'none';
            });
        });

        if (btnConfirmDept) {
            btnConfirmDept.addEventListener('click', () => {
                if (!selectedDepartmentValue) {
                    deptDropdownHeader.classList.add('error');
                    deptErrorMsg.style.display = 'block';
                    deptDropdownHeader.style.animation = 'shake 0.4s';
                    setTimeout(() => deptDropdownHeader.style.animation = '', 400);
                    return;
                }
                populateConfirmationData(selectedDepartmentValue.name);
                showConfirmationModal(confirmModal);
            });
        }
    };

    // ============================================
    // 6. Form Validation & Submit (Trigger Modals)
    // ============================================
    const validateForm = (event) => {
        event.preventDefault(); // إيقاف الإرسال التقليدي في البداية

        if (!form.checkValidity()) {
            event.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        pendingDriverInfo = {
            name: driverName.value.trim(),
            phone: driverPhone.value.trim(),
            nationalId: nationalId.value.trim()
        };

        // تصفير وفتح مودال الأقسام
        selectedDepartmentValue = null;
        if (deptSelectedValue) {
            deptSelectedValue.textContent = 'اختار القسم';
            deptSelectedValue.classList.add('text-muted');
        }
        if (deptDropdownHeader) deptDropdownHeader.classList.remove('error');
        if (deptErrorMsg) deptErrorMsg.style.display = 'none';
        document.querySelectorAll('#dept-dropdown-list .dropdown-item').forEach(i => i.classList.remove('selected'));

        showConfirmationModal(deptModal);
    };

    // ============================================
    // 7. Initialize Modals Events
    // ============================================
    const initializeModals = () => {
        initDepartmentDropdown();

        if (btnEdit) {
            btnEdit.addEventListener('click', () => {
                closeModal();
            });
        }

        if (btnConfirm) {
            btnConfirm.addEventListener('click', () => {

                // جلب اسم السائق المدخل
                const driverNameVal = driverName ? driverName.value.trim() : '';

                // تخزين بيانات السائق مؤقتاً للعودة بها لصفحة الشاحنة
                localStorage.setItem('newDriverName', driverNameVal);
                localStorage.setItem('newDriverId', '1'); // أو ضع الـ ID الفعلي لو متوفر بعد الحفظ

                // تمرير القسم المختار للكنترولر في حالة وجود حقل له في الـ Model
                if (selectedDepartmentValue) {
                    const hiddenDeptInput = document.createElement('input');
                    hiddenDeptInput.type = 'hidden';
                    hiddenDeptInput.name = 'DepartmentId';
                    hiddenDeptInput.value = selectedDepartmentValue.id;
                    form.appendChild(hiddenDeptInput);
                }

                // تغيير نص الزر
                btnConfirm.disabled = true;
                btnConfirm.innerHTML = 'جاري الحفظ... <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';

                // التحقق مما إذا كان هناك رابط عودة محفوظ (تم القدوم من صفحة الشاحنات)
                const returnUrl = localStorage.getItem('returnTruckUrl');
                if (returnUrl) {
                    localStorage.removeItem('returnTruckUrl'); // مسحه بعد الاستخدام
                    form.action = returnUrl; // يمكنك توجيه الفورم الرابط السابق مباشرة أو ترك الإرسال العادي
                }

                // الإرسال الفعلي للفورم للسيرفر
                form.submit();
            });
        }

        modalOverlay.addEventListener('click', (e) => {
            if (e.target === modalOverlay) {
                if (!deptModal.classList.contains('d-none') || !confirmModal.classList.contains('d-none')) {
                    closeModal();
                }
                if (deptDropdownContainer) deptDropdownContainer.classList.remove('open');
            }
        });
    };

    // ============================================
    // 8. Start
    // ============================================
    if (form) {
        form.addEventListener('submit', validateForm);
    }
    initializeModals();
});