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
    const successModal = document.getElementById('success-modal');

    const btnConfirmDept = document.getElementById('btn-confirm-dept');
    const btnEdit = document.getElementById('btn-edit');
    const btnConfirm = document.getElementById('btn-confirm');
    const btnBack = document.getElementById('btn-back');
    const btnPrint = document.getElementById('btn-print');

    const confirmCompany = document.getElementById('confirm-company');
    const confirmDriverName = document.getElementById('confirm-driver-name');
    const confirmDriverId = document.getElementById('confirm-driver-id');
    const confirmDriverdep = document.getElementById('confirm-driver-dep');
    const ticketNumberDisplay = document.getElementById('ticket-number');

    const deptDropdownContainer = document.getElementById('dept-dropdown-container');
    const deptDropdownHeader = document.getElementById('dept-dropdown-header');
    const deptSelectedValue = document.getElementById('dept-selected-value');
    const deptDropdownList = document.getElementById('dept-dropdown-list');
    const deptErrorMsg = document.getElementById('dept-error-msg');

    let selectedDepartmentValue = null;
    let pendingDriverInfo = {};

    // ============================================
    // 3. Custom Dropdown - شركة السائق (الأساسية)
    // ============================================
    const customDropdown = document.getElementById('customDropdown');

    if (customDropdown) {
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
                selectedValue.textContent = value;
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
    }

    const toggleCompanySelection = () => {
        companyStaticView.classList.add('d-none');
        companySelectView.classList.remove('d-none');
        companySelect.disabled = false;
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
        successModal.classList.remove('active-modal');
        successModal.classList.add('d-none');

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
        successModal.classList.remove('active-modal');
        if (deptDropdownContainer) deptDropdownContainer.classList.remove('open');
        document.body.style.overflow = '';
    };

    const populateConfirmationData = (departmentName = "غير متوفر") => {
        confirmCompany.textContent = pendingDriverInfo.company || "غير محدد";
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
    // 6. Ticket & Print Logic
    // ============================================
    let shiftTicketCounter = 0;
    const generateTicketNumber = () => {
        shiftTicketCounter++;
        let rawDockName = window.pageData?.dockName || 'A';
        let dockInitial = rawDockName.length > 0 ? rawDockName.charAt(0).toUpperCase() : 'A';
        return `${dockInitial}${shiftTicketCounter}`;
    };

    const printTicket = () => {
        if (btnPrint) {
            btnPrint.innerHTML = 'جاري التجهيز... <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true" style="margin-right: 8px;"></span>';
            btnPrint.disabled = true;
        }

        const ticketNum = ticketNumberDisplay ? ticketNumberDisplay.textContent : '0';
        const deptName = selectedDepartmentValue ? selectedDepartmentValue.name : 'غير محدد';
        const empName = window.pageData?.employeeName || 'محمد حسين';
        const dock = window.pageData?.dockName || 'A';

        const receiptData = {
            ticketNumber: ticketNum,
            requestType: 'تحويل',
            waitingCount: '0',
            department: deptName,
            dockNumber: dock,
            employeeName: empName,
            createdAt: new Date().toISOString()
        };

        localStorage.setItem('receiptData', JSON.stringify(receiptData));

        setTimeout(() => {
            if (window.appRoutes && window.appRoutes.receiptPage) {
                window.location.href = window.appRoutes.receiptPage;
            } else {
                window.location.href = window.appRoutes.companyDrivers;
            }
        }, 1500);
    };

    // ============================================
    // 7. Form Validation & Submit (تفعيل المودالات)
    // ============================================
    const validateForm = (event) => {
        event.preventDefault();

        // التحقق من صحة البيانات بالمدخلات
        if (!form.checkValidity()) {
            event.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        // معرفة اسم الشركة المعروض أو المختار
        let currentCompany = '';
        if (companyStaticView && !companyStaticView.classList.contains('d-none')) {
            currentCompany = companyDisplay.value;
        } else {
            currentCompany = companySelect.value;
        }

        // حفظ بيانات السائق مؤقتاً لاستعراضها في المودال
        pendingDriverInfo = {
            name: driverName.value.trim(),
            phone: driverPhone.value.trim(),
            nationalId: nationalId.value.trim(),
            company: currentCompany
        };

        // بدلاً من التوجيه المباشر -> يفتح مودال اختيار القسم أولاً
        showConfirmationModal(deptModal);
    };

    // ============================================
    // 8. Initialize Modals Events
    // ============================================
    const initializeModals = () => {
        initDepartmentDropdown();

        if (btnEdit) {
            btnEdit.addEventListener('click', () => {
                closeModal();
            });
        }

        if (btnConfirm) {
            btnConfirm.addEventListener('click', async () => {
                // إرسال البيانات للـ Controller في الخلفية عبر AJAX (إذا لزم الأمر)
                const formData = new FormData(form);
                if (selectedDepartmentValue) {
                    formData.append('DepartmentId', selectedDepartmentValue.id);
                }

                try {
                    // يمكن تفعيل الإرسال الفعلي بالسيرفر هنا:
                    /*
                    await fetch(form.action, {
                        method: 'POST',
                        body: formData
                    });
                    */
                } catch (err) {
                    console.error("خطأ أثناء حفظ السائق:", err);
                }

                // توليد البون وعرض مودال النجاح
                const ticketNum = generateTicketNumber();
                if (ticketNumberDisplay) ticketNumberDisplay.textContent = ticketNum;
                showConfirmationModal(successModal);
            });
        }

        if (btnBack) {
            btnBack.addEventListener('click', () => {
                window.location.href = window.appRoutes.companyDrivers;
            });
        }

        if (btnPrint) {
            btnPrint.addEventListener('click', () => {
                printTicket();
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
    // 9. Start
    // ============================================
    if (form) {
        form.addEventListener('submit', validateForm);
    }
    initializeModals();

});