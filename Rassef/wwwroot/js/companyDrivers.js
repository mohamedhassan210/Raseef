document.addEventListener('DOMContentLoaded', () => {

    // ==========================================
    // 1. DOM Elements
    // ==========================================
    const searchInput = document.getElementById('search-input');
    const cardsContainer = document.getElementById('cards-container');

    // Modal Elements
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

    // Custom Dropdown Elements (Department)
    const deptDropdownContainer = document.getElementById('dept-dropdown-container');
    const deptDropdownHeader = document.getElementById('dept-dropdown-header');
    const deptSelectedValue = document.getElementById('dept-selected-value');
    const deptDropdownList = document.getElementById('dept-dropdown-list');
    const deptErrorMsg = document.getElementById('dept-error-msg');
    let selectedDepartmentValue = null;

    // ==========================================
    // 2. Search & Filter Logic 
    // ==========================================
    const filterDrivers = () => {
        const query = searchInput.value.trim().toLowerCase();
        const cards = cardsContainer.querySelectorAll('.driver-card');

        cards.forEach(card => {
            const name = (card.getAttribute('data-name') || "").toLowerCase();
            const nationalId = (card.getAttribute('data-id') || "").toLowerCase();

            if (name.includes(query) || nationalId.includes(query)) {
                card.style.display = 'flex';
                card.style.animation = 'fadeIn 0.3s ease-in-out';
            } else {
                card.style.display = 'none';
            }
        });
    };

    if (searchInput) {
        searchInput.addEventListener('input', filterDrivers);
    }

    // ==========================================
    // 3. Modal System & Workflow
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
        deptDropdownContainer.classList.remove('open');
        document.body.style.overflow = '';
    };

    const populateConfirmationData = (driverName, departmentName = "غير متوفر") => {
        const company = window.pageData?.companyName || "غير محدد";
        const truck = window.pageData?.truckName || "غير محدد";

        confirmCompany.textContent = company;
        confirmTruck.textContent = truck;
        confirmDriverName.textContent = driverName;
        confirmDriverdep.textContent = departmentName;
    };

    const attachSelectionEvents = () => {
        const selectButtons = document.querySelectorAll('.select-btn');
        selectButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                const card = e.target.closest('.driver-card');
                const driverName = card.getAttribute('data-name');
                const driverId = card.getAttribute('data-id');
                const driverNationalId = card.getAttribute('data-national-id');

                selectedDepartmentValue = null;
                deptSelectedValue.textContent = 'اختار القسم';
                deptSelectedValue.classList.add('text-muted');
                deptDropdownHeader.classList.remove('error');
                deptErrorMsg.style.display = 'none';
                const deptItems = document.querySelectorAll('#dept-dropdown-list .dropdown-item');
                deptItems.forEach(i => i.classList.remove('selected'));
                deptDropdownContainer.classList.remove('open');

                localStorage.setItem('pendingDriver', JSON.stringify({
                    name: driverName,
                    driverId: driverId,
                    id: driverId,
                    nationalId: driverNationalId || driverId
                }));

                showConfirmationModal(deptModal);
            });
        });
    };
    attachSelectionEvents();

    // ==========================================
    // 4. Department Custom Dropdown Logic
    // ==========================================
    const initDepartmentDropdown = () => {
        const departmentsData = (window.departmentsData && window.departmentsData.length > 0)
            ? window.departmentsData
            : [
                { id: 101, name: "قسم الاستلام" },
                { id: 102, name: "قسم المخازن" },
                { id: 103, name: "قسم التوزيع" },
                { id: 104, name: "قسم المبيعات" }
            ];

        deptDropdownList.innerHTML = departmentsData.map(dept =>
            `<div class="dropdown-item" data-id="${dept.id}" data-value="${dept.name}">${dept.name}</div>`
        ).join('');

        const deptItems = deptDropdownList.querySelectorAll('.dropdown-item');

        deptDropdownHeader.addEventListener('click', (e) => {
            e.stopPropagation();
            deptDropdownContainer.classList.toggle('open');
            deptDropdownHeader.classList.remove('error');
            deptErrorMsg.style.display = 'none';
        });

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

        btnConfirmDept.addEventListener('click', () => {
            if (!selectedDepartmentValue) {
                deptDropdownHeader.classList.add('error');
                deptErrorMsg.style.display = 'block';
                return;
            }

            const pendingData = JSON.parse(localStorage.getItem('pendingDriver')) || {};
            pendingData.department = selectedDepartmentValue;
            localStorage.setItem('pendingDriver', JSON.stringify(pendingData));

            populateConfirmationData(pendingData.name, pendingData.department.name);
            showConfirmationModal(confirmModal);
        });
    };

    // ==========================================
    // 5. Ticket Generation
    // ==========================================
    let shiftTicketCounter = 0;
    const generateTicketNumber = () => {
        shiftTicketCounter++;
        let rawDockName = window.pageData?.dockName;
        let dockInitial = rawDockName && rawDockName.length > 0 
            ? rawDockName.charAt(0).toUpperCase() 
            : 'A';

        return `${dockInitial}${shiftTicketCounter}`;
    };

    const printTicket = () => {
        btnPrint.innerHTML = 'جاري الانتقال للإيصال... <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true" style="margin-right: 8px;"></span>';
        btnPrint.disabled = true;

        const raw = localStorage.getItem('receiptData');
        const receiptData = raw ? JSON.parse(raw) : {};
        const ticketIdParam = receiptData.ticketId ? `?ticketId=${receiptData.ticketId}` : '';

        setTimeout(() => {
            if (window.routes && window.routes.receiptPage) {
                window.location.href = window.routes.receiptPage + ticketIdParam;
            } else {
                window.location.href = "/Driver/Recript" + ticketIdParam;
            }
        }, 500);
    };

    // ==========================================
    // 6. Initialize App
    // ==========================================
    const initializeModals = () => {
        initDepartmentDropdown();

        // زر التعديل (يرجع للمسار الجديد SupOrTra)
        btnEdit.addEventListener('click', () => {
            if (window.routes && window.routes.backRoute) {
                window.location.href = window.routes.backRoute;
            } else {
                closeModal();
            }
        });

        btnConfirm.addEventListener('click', async () => {
            btnConfirm.disabled = true;
            btnConfirm.innerHTML = 'جاري الحفظ وإصدار الدور... <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';

            const pendingData = JSON.parse(localStorage.getItem('pendingDriver')) || {};
            const payload = {
                supplierId: parseInt(window.pageData?.supplierId) || 0,
                truckId: parseInt(window.pageData?.truckId) || 0,
                driverId: parseInt(pendingData.driverId || pendingData.id || pendingData.nationalId) || 0,
                departmentId: selectedDepartmentValue ? selectedDepartmentValue.id : 0
            };

            let ticketInfo = null;
            try {
                const endpoint = window.routes?.createSupplierTicket || '/Driver/CreateSupplierTicket';
                const response = await fetch(endpoint, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    ticketInfo = await response.json();
                }
            } catch (err) {
                console.error('Error creating supplier ticket:', err);
            }

            btnConfirm.disabled = false;
            btnConfirm.innerHTML = 'تأكيد';

            const ticketNum = ticketInfo?.ticketNumber || generateTicketNumber();
            ticketNumberDisplay.textContent = ticketNum;

            const deptName = ticketInfo?.departmentName || (selectedDepartmentValue ? selectedDepartmentValue.name : 'غير محدد');
            const empName = ticketInfo?.employeeName || window.pageData?.employeeName || 'المسؤول';
            const dock = ticketInfo?.dockName || window.pageData?.dockName || 'A';
            const waitCount = ticketInfo?.waitingCount !== undefined ? String(ticketInfo.waitingCount) : '0';

            const receiptData = {
                ticketId: ticketInfo?.ticketId || null,
                ticketNumber: ticketNum,
                requestType: 'توريد',
                waitingCount: waitCount,
                department: deptName,
                dockNumber: dock,
                employeeName: empName,
                createdAt: new Date().toISOString()
            };

            localStorage.setItem('receiptData', JSON.stringify(receiptData));
            showConfirmationModal(successModal);
        });

        // زر الرجوع في مودال النجاح (يرجع للمسار الجديد SupOrTra)
        btnBack.addEventListener('click', () => {
            if (window.routes && window.routes.backRoute) {
                window.location.href = window.routes.backRoute;
            } else if (window.routes && window.routes.supplierIndex) {
                window.location.href = window.routes.supplierIndex;
            } else {
                window.location.href = "/Supplier/Index";
            }
        });

        btnPrint.addEventListener('click', () => {
            printTicket();
        });

        modalOverlay.addEventListener('click', (e) => {
            if (e.target === modalOverlay) {
                if (!deptModal.classList.contains('d-none') || !confirmModal.classList.contains('d-none')) {
                    closeModal();
                }
                deptDropdownContainer.classList.remove('open');
            }
        });

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                if (deptDropdownContainer.classList.contains('open')) {
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