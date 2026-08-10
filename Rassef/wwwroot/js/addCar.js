document.addEventListener('DOMContentLoaded', () => {

    // ==========================================
    // 1. DOM Elements (Form & Dropdowns)
    // ==========================================
    const editCompanyBtn = document.getElementById('editCompanyBtn');
    const companyStaticView = document.getElementById('companyStaticView');
    const companySelectView = document.getElementById('companySelectView');
    const companySelect = document.getElementById('companySelect');
    const submitbtn = document.getElementById('submit-btn');
    const form = document.getElementById('addCarForm');

    // ==========================================
    // 2. DOM Elements (Modals)
    // ==========================================
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
    const confirmTruck = document.getElementById('confirm-truck');
    const confirmDriverName = document.getElementById('confirm-driver-name');
    const confirmDriverdep = document.getElementById('confirm-driver-dep');
    const ticketNumberDisplay = document.getElementById('ticket-number');

    const deptDropdownContainer = document.getElementById('dept-dropdown-container');
    const deptDropdownHeader = document.getElementById('dept-dropdown-header');
    const deptSelectedValue = document.getElementById('dept-selected-value');
    const deptDropdownList = document.getElementById('dept-dropdown-list');
    const deptErrorMsg = document.getElementById('dept-error-msg');

    let selectedDepartmentValue = null;
    let pendingTruckInfo = {}; // متغير لحفظ بيانات الفورم لعرضها في المودال

    // ==========================================
    // 3. Toggle Company Input logic
    // ==========================================
    if (editCompanyBtn) {
        editCompanyBtn.addEventListener('click', () => {
            if (companyStaticView) companyStaticView.classList.add('d-none');
            if (companySelectView) companySelectView.classList.remove('d-none');
            if (companySelect) companySelect.disabled = false;
        });
    }

    // ==========================================
    // Custom Form Dropdown Logic (للشركة) - محدث لدعم أي شركة
    // ==========================================
    document.querySelectorAll('.custom-dropdown').forEach(customDropdown => {
        if (customDropdown.id === 'dept-dropdown-container') return;

        const dropdownHeader = customDropdown.querySelector('.dropdown-header');
        const selectedValue = customDropdown.querySelector('.selected-value');
        const dropdownItems = customDropdown.querySelectorAll('.dropdown-item');

        const container = customDropdown.closest('.col-md-6') || customDropdown.parentElement;
        const nativeSelect = container.querySelector('select.d-none');

        if (dropdownHeader) {
            dropdownHeader.addEventListener('click', (e) => {
                e.stopPropagation();
                document.querySelectorAll('.custom-dropdown').forEach(d => {
                    if (d !== customDropdown) d.classList.remove('open');
                });
                customDropdown.classList.toggle('open');
            });
        }

        dropdownItems.forEach(item => {
            item.addEventListener('click', (e) => {
                e.stopPropagation();
                const value = item.getAttribute('data-value');

                if (selectedValue) {
                    selectedValue.textContent = value;
                    selectedValue.classList.remove('placeholder-color');
                }

                if (nativeSelect) {
                    // التأكد من إضافة الخيار لو مش موجود في الـ Select المخفي عشان الـ Validation يقبله
                    let optionExists = Array.from(nativeSelect.options).some(option => option.value === value);
                    if (!optionExists) {
                        const newOption = new Option(value, value, true, true);
                        nativeSelect.add(newOption);
                    }

                    nativeSelect.value = value;
                    nativeSelect.disabled = false; // فك التعطيل عشان يقبل الـ Submit
                    nativeSelect.dispatchEvent(new Event('change', { bubbles: true }));
                }

                dropdownItems.forEach(el => el.classList.remove('selected'));
                item.classList.add('selected');
                customDropdown.classList.remove('open');
            });
        });
    });

    document.addEventListener('click', (e) => {
        document.querySelectorAll('.custom-dropdown').forEach(customDropdown => {
            if (!customDropdown.contains(e.target)) {
                customDropdown.classList.remove('open');
            }
        });
    });

    // ==========================================
    // 4. Modal System Functions
    // ==========================================
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
        confirmCompany.textContent = pendingTruckInfo.company || "غير متوفر";
        confirmTruck.textContent = pendingTruckInfo.truckPlate || "غير متوفر";
        confirmDriverName.textContent = pendingTruckInfo.driverName || "غير محدد"; // مفيش سواق في شاشة الإضافة
        confirmDriverdep.textContent = departmentName;
    };

    // ==========================================
    // 5. Department Custom Dropdown Logic (للمودال)
    // ==========================================
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

    // ==========================================
    // 6. Ticket & Print Logic
    // ==========================================
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
        const empName = window.pageData?.employeeName || 'اسم الموظف';
        const dock = window.pageData?.dockName || 'A';

        const receiptData = {
            ticketNumber: ticketNum,
            waitingCount: '0',
            department: deptName,
            dockNumber: dock,
            employeeName: empName,
            createdAt: new Date().toISOString()
        };

        localStorage.setItem('receiptData', JSON.stringify(receiptData));

        setTimeout(() => {
            if (window.routes && window.routes.receiptPage) {
                window.location.href = window.routes.receiptPage;
            }
        }, 1500);
    };

    // ==========================================
    // 7. Form Submission (The Trigger!)
    // ==========================================
    if (form) {
        form.addEventListener('submit', (event) => {
            // نمنع إرسال الفورم الفوري والانتقال لصفحة أخرى
            event.preventDefault();

            if (!form.checkValidity()) {
                event.stopPropagation();
                form.classList.add('was-validated');
                return;
            }

            // استخراج اسم الشركة بشكل صحيح ومضمون
            let selectedCompany = "";
            const isSelectVisible = companySelectView && !companySelectView.classList.contains('d-none');

            if (isSelectVisible && companySelect) {
                selectedCompany = companySelect.value;
                console.log(selectedCompany)

            } else {
                selectedCompany = document.getElementById('companyDisplay')?.value;
                console.log(selectedCompany)
            }

            const plateLetters = document.getElementById('plateLetters');
            const plateNumbers = document.getElementById('plateNumbers');
            const fullPlate = `${plateLetters ? plateLetters.value.trim() : ""} ${plateNumbers ? plateNumbers.value.trim() : ""}`;

            // تجهيز البيانات
            pendingTruckInfo = {
                company: selectedCompany,
                truckPlate: fullPlate,
                driverName: "غير محدد"
            };

            // تصفير مودال الأقسام ثم فتحه
            selectedDepartmentValue = null;
            if (deptSelectedValue) {
                deptSelectedValue.textContent = 'اختار الشاحنة';
                deptSelectedValue.classList.add('text-muted');
            }
            if (deptDropdownHeader) deptDropdownHeader.classList.remove('error');
            if (deptErrorMsg) deptErrorMsg.style.display = 'none';
            document.querySelectorAll('#dept-dropdown-list .dropdown-item').forEach(i => i.classList.remove('selected'));

            showConfirmationModal(deptModal);
        });
    }

    // ==========================================
    // 8. Initialize Modals Events
    // ==========================================
    const initializeModals = () => {
        initDepartmentDropdown();

        if (btnEdit) {
            btnEdit.addEventListener('click', () => {
                closeModal(); // قفل المودال عشان اليوزر يرجع يعدل في الفورم براحته
            });
        }

        if (btnConfirm) {
            btnConfirm.addEventListener('click', () => {
                const ticketNum = generateTicketNumber();
                if (ticketNumberDisplay) ticketNumberDisplay.textContent = ticketNum;
                showConfirmationModal(successModal);
            });
        }

        if (btnBack) {
            btnBack.addEventListener('click', () => {
                closeModal();
            });
        }

        if (btnPrint) {
            btnPrint.addEventListener('click', () => {
                printTicket();
            });
        }

        // إغلاق المودال عند الضغط خارجه أو بالـ ESC
        modalOverlay.addEventListener('click', (e) => {
            if (e.target === modalOverlay) {
                if (!deptModal.classList.contains('d-none') || !confirmModal.classList.contains('d-none')) {
                    closeModal();
                }
                if (deptDropdownContainer) deptDropdownContainer.classList.remove('open');
            }
        });

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                if (deptDropdownContainer && deptDropdownContainer.classList.contains('open')) {
                    deptDropdownContainer.classList.remove('open');
                } else if (modalOverlay.classList.contains('active')) {
                    if (!deptModal.classList.contains('d-none') || !confirmModal.classList.contains('d-none')) {
                        closeModal();
                    }
                }
            }
        });
    };

    initializeModals();
});