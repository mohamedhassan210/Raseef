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

    // 2. Search & Filter Logic
    const filterTrucks = () => {
        if (!searchInput || !cardsContainer) return;
        const query = searchInput.value.trim().toLowerCase();
        const cards = cardsContainer.querySelectorAll('.truck-card');

        cards.forEach(card => {
            const plate = (card.getAttribute('data-plate') || "").toLowerCase();
            if (plate.replace(/\s+/g, '').includes(query.replace(/\s+/g, ''))) {
                card.style.display = 'flex';
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
        if (!modalOverlay || !modalElement) return;

        if (deptModal) { deptModal.classList.remove('active-modal'); deptModal.classList.add('d-none'); }
        if (confirmModal) { confirmModal.classList.remove('active-modal'); confirmModal.classList.add('d-none'); }
        if (successModal) { successModal.classList.remove('active-modal'); successModal.classList.add('d-none'); }

        modalOverlay.classList.add('active');
        modalElement.classList.remove('d-none');

        setTimeout(() => {
            modalElement.classList.add('active-modal');
        }, 10);

        document.body.style.overflow = 'hidden';
    };

    const closeModal = () => {
        if (!modalOverlay) return;
        modalOverlay.classList.remove('active');
        if (deptModal) deptModal.classList.remove('active-modal');
        if (confirmModal) confirmModal.classList.remove('active-modal');
        if (successModal) successModal.classList.remove('active-modal');
        if (deptDropdownContainer) deptDropdownContainer.classList.remove('open');
        document.body.style.overflow = '';
    };

    const populateConfirmationData = (truckPlate, departmentName = "غير متوفر") => {
        if (confirmCompany) confirmCompany.textContent = "تحويل داخلي";
        if (confirmTruck) confirmTruck.textContent = truckPlate || "غير محدد";
        if (confirmDriverName) confirmDriverName.textContent = "سائق تحويل";
        if (confirmDriverdep) confirmDriverdep.textContent = departmentName;
    };

    const attachSelectionEvents = () => {
        const selectButtons = document.querySelectorAll('.select-btn');
        selectButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                const card = e.target.closest('.truck-card');
                const plateData = card ? card.getAttribute('data-plate') : "";

                selectedDepartmentValue = null;
                if (deptSelectedValue) {
                    deptSelectedValue.textContent = 'اختار القسم';
                    deptSelectedValue.classList.add('text-muted');
                }
                if (deptDropdownHeader) deptDropdownHeader.classList.remove('error');
                if (deptErrorMsg) deptErrorMsg.style.display = 'none';
                document.querySelectorAll('#dept-dropdown-list .dropdown-item').forEach(i => i.classList.remove('selected'));
                if (deptDropdownContainer) deptDropdownContainer.classList.remove('open');

                localStorage.setItem('pendingTruck', JSON.stringify({
                    plate: plateData
                }));

                if (deptModal) {
                    showConfirmationModal(deptModal);
                }
            });
        });
    };
    attachSelectionEvents();

    // 4. Department Custom Dropdown Logic
    const initDepartmentDropdown = () => {
        const departmentsData = (window.departmentsData && window.departmentsData.length > 0)
            ? window.departmentsData
            : [
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
                if (deptDropdownContainer) deptDropdownContainer.classList.toggle('open');
                deptDropdownHeader.classList.remove('error');
                if (deptErrorMsg) deptErrorMsg.style.display = 'none';
            });
        }

        deptItems.forEach(item => {
            item.addEventListener('click', (e) => {
                e.stopPropagation();

                selectedDepartmentValue = {
                    id: parseInt(item.getAttribute('data-id')),
                    name: item.getAttribute('data-value')
                };

                if (deptSelectedValue) {
                    deptSelectedValue.textContent = selectedDepartmentValue.name;
                    deptSelectedValue.classList.remove('text-muted');
                }

                deptItems.forEach(el => el.classList.remove('selected'));
                item.classList.add('selected');

                if (deptDropdownContainer) deptDropdownContainer.classList.remove('open');
                if (deptDropdownHeader) deptDropdownHeader.classList.remove('error');
                if (deptErrorMsg) deptErrorMsg.style.display = 'none';
            });
        });

        if (btnConfirmDept) {
            btnConfirmDept.addEventListener('click', () => {
                if (!selectedDepartmentValue) {
                    if (deptDropdownHeader) {
                        deptDropdownHeader.classList.add('error');
                        deptDropdownHeader.style.animation = 'shake 0.4s';
                        setTimeout(() => deptDropdownHeader.style.animation = '', 400);
                    }
                    if (deptErrorMsg) deptErrorMsg.style.display = 'block';
                    return;
                }

                const pendingData = JSON.parse(localStorage.getItem('pendingTruck')) || {};
                pendingData.department = selectedDepartmentValue;
                localStorage.setItem('pendingTruck', JSON.stringify(pendingData));

                populateConfirmationData(pendingData.plate, pendingData.department.name);
                if (confirmModal) showConfirmationModal(confirmModal);
            });
        }
    };

    // 5. Ticket Generation
    let shiftTicketCounter = 0;
    const generateTicketNumber = () => {
        shiftTicketCounter++;
        let rawDockName = window.pageData?.dockName;
        let dockInitial = rawDockName && rawDockName.length > 0 ? rawDockName.charAt(0).toUpperCase() : 'TR';
        return `${dockInitial}-${String(shiftTicketCounter).padStart(4, '0')}`;
    };

    const printTicket = () => {
        if (btnPrint) {
            btnPrint.innerHTML = 'جاري الانتقال للإيصال... <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true" style="margin-right: 8px;"></span>';
            btnPrint.disabled = true;
        }

        const ticketNum = ticketNumberDisplay ? ticketNumberDisplay.textContent : 'TR-0001';
        const deptName = selectedDepartmentValue ? selectedDepartmentValue.name : 'قسم التحويل';
        const empName = window.pageData?.employeeName || 'موظف النظام';
        const dock = window.pageData?.dockName || 'A1';
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
        }, 600);
    };

    // 6. Initialize App
    const initializeModals = () => {
        initDepartmentDropdown();

        if (btnEdit) {
            btnEdit.addEventListener('click', () => {
                if (deptModal) {
                    showConfirmationModal(deptModal);
                } else {
                    closeModal();
                }
            });
        }

        if (btnConfirm) {
            btnConfirm.addEventListener('click', () => {
                const ticketNum = generateTicketNumber();
                if (ticketNumberDisplay) ticketNumberDisplay.textContent = ticketNum;
                if (successModal) showConfirmationModal(successModal);
            });
        }

        if (btnBack) {
            btnBack.addEventListener('click', () => {
                if (window.routes && window.routes.viewRole) {
                    window.location.href = window.routes.viewRole;
                } else if (window.routes && window.routes.backRoute) {
                    window.location.href = window.routes.backRoute;
                } else {
                    window.location.href = "/Authentication/viewRole";
                }
            });
        }

        if (btnPrint) {
            btnPrint.addEventListener('click', () => {
                printTicket();
            });
        }

        if (modalOverlay) {
            modalOverlay.addEventListener('click', (e) => {
                if (e.target === modalOverlay) {
                    if ((deptModal && !deptModal.classList.contains('d-none')) ||
                        (confirmModal && !confirmModal.classList.contains('d-none'))) {
                        closeModal();
                    }
                    if (deptDropdownContainer) deptDropdownContainer.classList.remove('open');
                }
            });
        }

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                if (deptDropdownContainer && deptDropdownContainer.classList.contains('open')) {
                    deptDropdownContainer.classList.remove('open');
                } else if (modalOverlay && modalOverlay.classList.contains('active')) {
                    if ((deptModal && !deptModal.classList.contains('d-none')) ||
                        (confirmModal && !confirmModal.classList.contains('d-none'))) {
                        closeModal();
                    }
                }
            }
        });
    };

    initializeModals();
});