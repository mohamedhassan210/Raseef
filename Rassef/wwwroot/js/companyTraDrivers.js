document.addEventListener('DOMContentLoaded', () => {
    // 1. DOM Elements
    const searchInput = document.getElementById('search-input');
    const cardsContainer = document.getElementById('cards-container');

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

    // 2. Search & Filter Logic (تحديث للبحث برقم لوحة السيارة)
    const filterTrucks = () => {
        const query = searchInput.value.trim().toLowerCase();
        const cards = cardsContainer.querySelectorAll('.truck-card');

        cards.forEach(card => {
            const plate = (card.getAttribute('data-plate') || "").toLowerCase();

            // شيلنا المسافات من اللوحة في البحث عشان لو اليوزر كتب الرقم ورا بعضه
            if (plate.replace(/\s+/g, '').includes(query.replace(/\s+/g, ''))) {
                card.style.display = 'flex';
                card.style.animation = 'fadeIn 0.3s ease-in-out';
            } else {
                card.style.display = 'none';
            }
        });
    };

    if (searchInput) {
        searchInput.addEventListener('input', filterTrucks);
    }

    // 3. Modal System & Workflow
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

    const populateConfirmationData = (truckPlate, departmentName = "غير متوفر") => {
        const company = "تحويل داخلي";

        confirmCompany.textContent = company;
        confirmTruck.textContent = truckPlate; // حطينا رقم السيارة هنا
        confirmDriverName.textContent = "غير محدد"; // لو مفيش سائق مرتبط حالياً
        confirmDriverdep.textContent = departmentName;
    };

    const attachSelectionEvents = () => {
        const selectButtons = document.querySelectorAll('.select-btn');
        selectButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                const card = e.target.closest('.truck-card');
                const plateData = card.getAttribute('data-plate');

                selectedDepartmentValue = null;
                deptSelectedValue.textContent = 'اختار القسم';
                deptSelectedValue.classList.add('text-muted');
                deptDropdownHeader.classList.remove('error');
                deptErrorMsg.style.display = 'none';
                const deptItems = document.querySelectorAll('#dept-dropdown-list .dropdown-item');
                deptItems.forEach(i => i.classList.remove('selected'));
                deptDropdownContainer.classList.remove('open');

                // تعديل التخزين ليكون خاص بالسيارة
                localStorage.setItem('pendingTruck', JSON.stringify({
                    plate: plateData
                }));

                showConfirmationModal(deptModal);
            });
        });
    };
    attachSelectionEvents();

    // 4. Department Custom Dropdown Logic
    const initDepartmentDropdown = () => {
        const departmentsData = [
            { id: 101, name: "قسم الاستلام" },
            { id: 102, name: "قسم المخازن" },
            { id: 103, name: "قسم التوزيع" },
            { id: 104, name: "قسم المبيعات" }
        ];

        //deptDropdownList.innerHTML = departmentsData.map(dept =>
        //    `<div class="dropdown-item" data-id="${dept.id}" data-value="${dept.name}">${dept.name}</div>`
        //).join('');

        //const deptItems = deptDropdownList.querySelectorAll('.dropdown-item');

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
                deptDropdownHeader.style.animation = 'shake 0.4s';
                setTimeout(() => deptDropdownHeader.style.animation = '', 400);
                return;
            }

            const pendingData = JSON.parse(localStorage.getItem('pendingTruck')) || {};
            pendingData.department = selectedDepartmentValue;
            localStorage.setItem('pendingTruck', JSON.stringify(pendingData));

            populateConfirmationData(pendingData.plate, pendingData.department.name);
            showConfirmationModal(confirmModal);
        });
    };

    // 5. Ticket Badge Visual Counter Logic
    let shiftTicketCounter = 0;
    const generateTicketNumber = () => {
        shiftTicketCounter++;
        let rawDockName = window.pageData?.dockName;
        let dockInitial = rawDockName && rawDockName.length > 0 ? rawDockName.charAt(0).toUpperCase() : 'A';
        return `${dockInitial}${shiftTicketCounter}`;
    };

    const printTicket = () => {
        btnPrint.innerHTML = 'جاري الانتقال للإيصال... <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true" style="margin-right: 8px;"></span>';
        btnPrint.disabled = true;

        const ticketNum = ticketNumberDisplay.textContent;
        const deptName = selectedDepartmentValue ? selectedDepartmentValue.name : 'غير محدد';
        const empName = window.pageData?.employeeName || 'محمد حسين';
        const dock = window.pageData?.dockName || 'A';
        const waitCount = '0';

        const receiptData = {
            ticketNumber: ticketNum,
            waitingCount: waitCount,
            department: deptName,
            dockNumber: dock,
            employeeName: empName,
            createdAt: new Date().toISOString()
        };

        localStorage.setItem('receiptData', JSON.stringify(receiptData));

        setTimeout(() => {
            if (window.routes && window.routes.receiptPage) {
                window.location.href = window.routes.receiptPage;
            } else {
                window.location.href = "/Driver/Recript";
            }
        }, 800);
    };

    // 6. Initialize App
    const initializeModals = () => {
        initDepartmentDropdown();

        btnEdit.addEventListener('click', () => {
            if (window.routes && window.routes.backRoute) {
                window.location.href = window.routes.backRoute;
            }
        });

        btnConfirm.addEventListener('click', () => {
            const ticketNum = generateTicketNumber();
            ticketNumberDisplay.textContent = ticketNum;
            showConfirmationModal(successModal);
        });

        btnBack.addEventListener('click', () => {
            if (window.routes && window.routes.backRoute) {
                window.location.href = window.routes.backRoute;
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