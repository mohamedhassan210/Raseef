document.addEventListener('DOMContentLoaded', () => {

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
    const confirmDriverDock = document.getElementById('confirm-driver-dock');
    const ticketNumberDisplay = document.getElementById('ticket-number');

    const deptDropdownContainer = document.getElementById('dept-dropdown-container');
    const deptDropdownHeader = document.getElementById('dept-dropdown-header');
    const deptSelectedValue = document.getElementById('dept-selected-value');
    const deptDropdownList = document.getElementById('dept-dropdown-list');
    const deptErrorMsg = document.getElementById('dept-error-msg');
    let selectedDepartmentValue = null;

    // Custom Dropdown Elements (Dock) — اختياري، بيتفلتر حسب القسم المختار
    const dockDropdownContainer = document.getElementById('dock-dropdown-container');
    const dockDropdownHeader = document.getElementById('dock-dropdown-header');
    const dockSelectedValue = document.getElementById('dock-selected-value');
    const dockDropdownList = document.getElementById('dock-dropdown-list');
    let selectedDockValue = null;

    // NEW — رقم الأفيز / رقم التصريح / نوع الإذن (إجبارية، زي صفحة إضافة شاحنة جديدة)
    const modalAvizNumberInput = document.getElementById('modalAvizNumber');
    const modalPermitNumberInput = document.getElementById('modalPermitNumber');
    const avizErrorMsg = document.getElementById('aviz-error-msg');
    const permitNumberErrorMsg = document.getElementById('permit-number-error-msg');
    const permitTypeDropdownContainer = document.getElementById('permit-type-dropdown-container');
    const permitTypeDropdownHeader = document.getElementById('permit-type-dropdown-header');
    const permitTypeSelectedValue = document.getElementById('permit-type-selected-value');
    const permitTypeDropdownList = document.getElementById('permit-type-dropdown-list');
    const permitTypeErrorMsg = document.getElementById('permit-type-error-msg');
    const confirmAviz = document.getElementById('confirm-aviz');
    const confirmPermitNumber = document.getElementById('confirm-permit-number');
    const confirmPermitType = document.getElementById('confirm-permit-type');
    let selectedPermitTypeValue = null;

    const closeAllDropdowns = (except) => {
        [deptDropdownContainer, dockDropdownContainer, permitTypeDropdownContainer].forEach(d => {
            if (d && d !== except) d.classList.remove('open');
        });
    };

    const resetPermitFields = () => {
        if (modalAvizNumberInput) modalAvizNumberInput.value = '';
        if (modalPermitNumberInput) modalPermitNumberInput.value = '';
        if (avizErrorMsg) avizErrorMsg.style.display = 'none';
        if (permitNumberErrorMsg) permitNumberErrorMsg.style.display = 'none';
        selectedPermitTypeValue = null;
        if (permitTypeSelectedValue) {
            permitTypeSelectedValue.textContent = 'اختر نوع الإذن';
            permitTypeSelectedValue.classList.add('text-muted');
        }
        if (permitTypeDropdownHeader) permitTypeDropdownHeader.classList.remove('error');
        if (permitTypeErrorMsg) permitTypeErrorMsg.style.display = 'none';
        if (permitTypeDropdownList) permitTypeDropdownList.querySelectorAll('.dropdown-item').forEach(i => i.classList.remove('selected'));
        if (permitTypeDropdownContainer) permitTypeDropdownContainer.classList.remove('open');
    };

    const initPermitTypeDropdown = () => {
        if (!permitTypeDropdownList) return;

        const permitTypes = Array.isArray(window.permitTypesData) ? window.permitTypesData : [];

        permitTypeDropdownList.innerHTML = permitTypes.length > 0
            ? permitTypes.map(pt =>
                `<div class="dropdown-item" data-id="${pt.id}" data-value="${pt.name}">${pt.name}</div>`
              ).join('')
            : `<div class="dropdown-item disabled text-muted">لا توجد أنواع أذونات متاحة</div>`;

        const items = permitTypeDropdownList.querySelectorAll('.dropdown-item:not(.disabled)');
        items.forEach(item => {
            item.addEventListener('click', (e) => {
                e.stopPropagation();
                selectedPermitTypeValue = {
                    id: parseInt(item.getAttribute('data-id')),
                    name: item.getAttribute('data-value')
                };
                permitTypeSelectedValue.textContent = selectedPermitTypeValue.name;
                permitTypeSelectedValue.classList.remove('text-muted');
                items.forEach(i => i.classList.remove('selected'));
                item.classList.add('selected');
                permitTypeDropdownContainer.classList.remove('open');
                permitTypeDropdownHeader.classList.remove('error');
                if (permitTypeErrorMsg) permitTypeErrorMsg.style.display = 'none';
            });
        });

        permitTypeDropdownHeader.addEventListener('click', (e) => {
            e.stopPropagation();
            closeAllDropdowns(permitTypeDropdownContainer);
            permitTypeDropdownContainer.classList.toggle('open');
            permitTypeDropdownHeader.classList.remove('error');
            if (permitTypeErrorMsg) permitTypeErrorMsg.style.display = 'none';
        });

        // ضغطة على أي مكان فاضي جوه المودال تقفل القوائم المفتوحة
        if (deptModal) {
            deptModal.addEventListener('click', (e) => {
                if (!e.target.closest('.custom-dropdown')) closeAllDropdowns(null);
            });
        }
    };
    initPermitTypeDropdown();

    const refreshDockOptions = (departmentId) => {
        if (!dockDropdownList) return;

        selectedDockValue = null;
        dockDropdownList.innerHTML = '';
        dockSelectedValue.textContent = 'بدون رصيف محدد';
        dockSelectedValue.classList.add('text-muted');

        if (!departmentId) {
            dockSelectedValue.textContent = 'اختار القسم أولاً';
            return;
        }

        const docks = (window.allDocks || []).filter(d => String(d.departmentId) === String(departmentId));

        if (docks.length === 0) {
            const empty = document.createElement('div');
            empty.className = 'dropdown-item disabled text-muted';
            empty.textContent = 'لا توجد أرصفة لهذا القسم';
            dockDropdownList.appendChild(empty);
            return;
        }

        docks.forEach(dock => {
            const item = document.createElement('div');
            item.className = 'dropdown-item' + (dock.isSelectable ? '' : ' disabled');
            item.textContent = dock.name + ' — ' + dock.occupancy + '/' + dock.maxTruckCount + dock.disabledReasonLabel;

            if (dock.isSelectable) {
                item.addEventListener('click', (e) => {
                    e.stopPropagation();
                    selectedDockValue = { id: dock.id, name: dock.name };
                    dockSelectedValue.textContent = dock.name;
                    dockSelectedValue.classList.remove('text-muted');
                    dockDropdownList.querySelectorAll('.dropdown-item').forEach(i => i.classList.remove('selected'));
                    item.classList.add('selected');
                    dockDropdownContainer.classList.remove('open');
                });
            }

            dockDropdownList.appendChild(item);
        });
    };

    if (dockDropdownHeader) {
        dockDropdownHeader.addEventListener('click', (e) => {
            e.stopPropagation();
            closeAllDropdowns(dockDropdownContainer);
            dockDropdownContainer.classList.toggle('open');
        });
    }

    const filterDrivers = () => {
        if (!searchInput || !cardsContainer) return;
        const query = searchInput.value.trim().toLowerCase();
        const cards = cardsContainer.querySelectorAll('.driver-card');

        cards.forEach(card => {
            const name = (card.getAttribute('data-name') || "").toLowerCase();
            const nationalId = (card.getAttribute('data-national-id') || "").toLowerCase();

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

    const populateConfirmationData = (driverName, departmentName = "غير متوفر", dockName = "بدون رصيف محدد") => {
        const company = window.pageData?.companyName || "تحويل داخلي";
        const truck = window.pageData?.truckName || "غير محدد";

        confirmCompany.textContent = company;
        confirmTruck.textContent = truck;
        confirmDriverName.textContent = driverName;
        confirmDriverdep.textContent = departmentName;
        if (confirmDriverDock) confirmDriverDock.textContent = dockName;

        if (confirmAviz) confirmAviz.textContent = modalAvizNumberInput ? modalAvizNumberInput.value.trim() : '';
        if (confirmPermitNumber) confirmPermitNumber.textContent = modalPermitNumberInput ? modalPermitNumberInput.value.trim() : '';
        if (confirmPermitType) confirmPermitType.textContent = selectedPermitTypeValue ? selectedPermitTypeValue.name : '';
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
                refreshDockOptions(null);
                resetPermitFields();

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

    const initDepartmentDropdown = () => {
        const departmentsData = Array.isArray(window.departmentsData) ? window.departmentsData : [];

        deptDropdownList.innerHTML = departmentsData.length > 0
            ? departmentsData.map(dept =>
                `<div class="dropdown-item" data-id="${dept.id}" data-value="${dept.name}">${dept.name}</div>`
              ).join('')
            : `<div class="dropdown-item disabled text-muted">لا توجد أقسام متاحة</div>`;

        const deptItems = deptDropdownList.querySelectorAll('.dropdown-item:not(.disabled)');

        deptDropdownHeader.addEventListener('click', (e) => {
            e.stopPropagation();
            closeAllDropdowns(deptDropdownContainer);
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

                refreshDockOptions(selectedDepartmentValue.id);
            });
        });

        btnConfirmDept.addEventListener('click', () => {
            if (!selectedDepartmentValue) {
                deptDropdownHeader.classList.add('error');
                deptErrorMsg.style.display = 'block';
                return;
            }
            if (!selectedDockValue) {
                if (dockDropdownHeader) dockDropdownHeader.classList.add('error');
                return;
            }

            // NEW — رقم الأفيز / رقم التصريح / نوع الإذن إجبارية
            let hasPermitError = false;

            if (modalAvizNumberInput && !modalAvizNumberInput.value.trim()) {
                if (avizErrorMsg) avizErrorMsg.style.display = 'block';
                hasPermitError = true;
            } else if (avizErrorMsg) {
                avizErrorMsg.style.display = 'none';
            }

            if (modalPermitNumberInput && !modalPermitNumberInput.value.trim()) {
                if (permitNumberErrorMsg) permitNumberErrorMsg.style.display = 'block';
                hasPermitError = true;
            } else if (permitNumberErrorMsg) {
                permitNumberErrorMsg.style.display = 'none';
            }

            if (permitTypeDropdownList && !selectedPermitTypeValue) {
                if (permitTypeDropdownHeader) permitTypeDropdownHeader.classList.add('error');
                if (permitTypeErrorMsg) permitTypeErrorMsg.style.display = 'block';
                hasPermitError = true;
            }

            if (hasPermitError) return;

            const pendingData = JSON.parse(localStorage.getItem('pendingDriver')) || {};
            pendingData.department = selectedDepartmentValue;
            localStorage.setItem('pendingDriver', JSON.stringify(pendingData));

            populateConfirmationData(pendingData.name, pendingData.department.name, selectedDockValue.name);
            showConfirmationModal(confirmModal);
        });
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

    const initializeModals = () => {
        initDepartmentDropdown();

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
                truckId: parseInt(window.pageData?.truckId) || 0,
                driverId: parseInt(pendingData.driverId || pendingData.id) || 0,
                departmentId: selectedDepartmentValue ? selectedDepartmentValue.id : 0,
                dockId: selectedDockValue ? selectedDockValue.id : null,
                avizNumber: modalAvizNumberInput ? modalAvizNumberInput.value.trim() : '',
                permitNumber: modalPermitNumberInput ? modalPermitNumberInput.value.trim() : '',
                permitTypeId: selectedPermitTypeValue ? selectedPermitTypeValue.id : null
            };

            let ticketInfo = null;
            let errorMessage = null;

            try {
                const endpoint = window.routes?.createTransferTicket || '/Truck/CreateTransferTicket';
                const response = await fetch(endpoint, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });

                const data = await response.json().catch(() => null);

                if (response.ok && data?.success) {
                    ticketInfo = data;
                } else {
                    errorMessage = data?.message || 'حدث خطأ أثناء إصدار الدور. حاول مرة أخرى.';
                }
            } catch (err) {
                console.error('Error creating transfer ticket:', err);
                errorMessage = 'تعذر الاتصال بالخادم. يرجى المحاولة مرة أخرى.';
            }

            btnConfirm.disabled = false;
            btnConfirm.innerHTML = 'تأكيد';

            if (!ticketInfo) {
                alert(errorMessage);
                return;
            }

            ticketNumberDisplay.textContent = ticketInfo.ticketNumber;

            const receiptData = {
                ticketId: ticketInfo.ticketId,
                ticketNumber: ticketInfo.ticketNumber,
                requestType: 'تحويل',
                waitingCount: ticketInfo.waitingCount !== undefined ? String(ticketInfo.waitingCount) : '0',
                department: ticketInfo.departmentName || (selectedDepartmentValue ? selectedDepartmentValue.name : 'غير محدد'),
                dockNumber: ticketInfo.dockName || window.pageData?.dockName || 'A',
                employeeName: ticketInfo.employeeName || window.pageData?.employeeName || 'المسؤول',
                createdAt: new Date().toISOString()
            };

            localStorage.setItem('receiptData', JSON.stringify(receiptData));
            showConfirmationModal(successModal);
        });

        btnBack.addEventListener('click', () => {
            if (window.routes && window.routes.backRoute) {
                window.location.href = window.routes.backRoute;
            } else {
                window.location.href = "/Truck/MainTraDrivers";
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
