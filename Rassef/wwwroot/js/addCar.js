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
    // Custom Form Dropdown Logic (للشركة والديناميك)
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
                const value = item.getAttribute('data-value'); // ID المورد
                const text = item.textContent.trim();          // اسم المورد

                if (selectedValue) {
                    selectedValue.textContent = text;
                    selectedValue.classList.remove('placeholder-color');
                }

                if (nativeSelect) {
                    nativeSelect.value = value;
                    nativeSelect.disabled = false;
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
    const showConfirmationModal = (modalElement) => {
        document.querySelectorAll('.custom-modal-container').forEach(m => {
            m.classList.remove('active-modal');
            m.classList.add('d-none');
        });

        if (modalOverlay) modalOverlay.classList.add('active');
        if (modalElement) {
            modalElement.classList.remove('d-none');
            setTimeout(() => {
                modalElement.classList.add('active-modal');
            }, 10);
        }
        document.body.style.overflow = 'hidden';
    };

    const closeModal = () => {
        if (modalOverlay) modalOverlay.classList.remove('active');
        document.querySelectorAll('.custom-modal-container').forEach(m => {
            m.classList.remove('active-modal');
            m.classList.add('d-none');
        });
        if (deptDropdownContainer) deptDropdownContainer.classList.remove('open');
        document.body.style.overflow = '';
    };

    const hideConfirmationModal = (modalElement) => {
        if (modalElement) {
            modalElement.classList.remove('active-modal');
            modalElement.classList.add('d-none');
        }
        if (modalOverlay) modalOverlay.classList.remove('active');
        document.body.style.overflow = '';
    };

    const populateConfirmationData = (departmentName = "غير متوفر") => {
        confirmCompany.textContent = pendingTruckInfo.company || "غير متوفر";
        confirmTruck.textContent = pendingTruckInfo.truckPlate || "غير متوفر";
        confirmDriverName.textContent = pendingTruckInfo.driverName || "غير محدد";
        confirmDriverdep.textContent = departmentName;
    };

    // ==========================================
    // 5. Department Custom Dropdown Logic (للمودال)
    // ==========================================
    const initDepartmentDropdown = () => {
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
                    id: parseInt(item.getAttribute('data-id') || item.getAttribute('data-value')),
                    name: item.textContent.trim()
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
            event.preventDefault();

            if (!form.checkValidity()) {
                event.stopPropagation();
                form.classList.add('was-validated');
                return;
            }

            let selectedCompanyText = "";
            const isSelectVisible = companySelectView && !companySelectView.classList.contains('d-none');

            if (isSelectVisible) {
                const selectedDropdown = companySelectView.querySelector('.selected-value');
                selectedCompanyText = selectedDropdown ? selectedDropdown.textContent.trim() : "";
            } else {
                selectedCompanyText = document.getElementById('companyDisplay')?.value;
            }

            const plateLetters = document.getElementById('plateLetters');
            const plateNumbers = document.getElementById('plateNumbers');
            const fullPlate = `${plateLetters ? plateLetters.value.trim() : ""} ${plateNumbers ? plateNumbers.value.trim() : ""}`;

            const driverSelectedSpan = document.querySelector('#customDriverDropdown .selected-value');
            const driverNameText = (driverSelectedSpan && !driverSelectedSpan.classList.contains('placeholder-color')) ? driverSelectedSpan.textContent.trim() : "غير محدد";

            pendingTruckInfo = {
                company: selectedCompanyText,
                truckPlate: fullPlate,
                driverName: driverNameText
            };

            selectedDepartmentValue = null;
            if (deptSelectedValue) {
                deptSelectedValue.textContent = 'اختر القسم';
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
                closeModal();
            });
        }

        if (btnConfirm) {
            btnConfirm.addEventListener('click', async () => {
                btnConfirm.disabled = true;
                btnConfirm.innerText = "جاري الحفظ...";

                if (selectedDepartmentValue) {
                    let hiddenDept = document.getElementById('hiddenDepartmentId');
                    if (!hiddenDept) {
                        hiddenDept = document.createElement('input');
                        hiddenDept.type = 'hidden';
                        hiddenDept.id = 'hiddenDepartmentId';
                        hiddenDept.name = 'DepartmentId';
                        form.appendChild(hiddenDept);
                    }
                    hiddenDept.value = selectedDepartmentValue.id;
                }

                form.submit();
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

    // ==========================================
    // 9. Plate Inputs Formatting
    // ==========================================

    /**
     * Real-Time Plate Formatter
     *
     * الوظائف:
     * - الحروف: عربي فقط.
     * - الأرقام: أرقام فقط.
     * - إزالة جميع المسافات اليدوية.
     * - إضافة Space تلقائي بين كل Character.
     * - الحفاظ على مكان الـ Cursor.
     * - يدعم الكتابة، الحذف، الـ Paste، والتنقل داخل النص.
     */
    const setupPlateFormatting = (inputElement, allowedCharactersRegex) => {
        if (!inputElement) return;

        inputElement.addEventListener('input', () => {

            // القيمة الحالية بعد إدخال المستخدم
            const currentValue = inputElement.value;

            // مكان الـ Cursor الحالي بعد عملية الإدخال/الحذف
            const cursorPosition = inputElement.selectionStart ?? currentValue.length;

            // --------------------------------------------------
            // 1. حساب عدد الـ Characters الصالحة قبل الـ Cursor
            // --------------------------------------------------

            const textBeforeCursor = currentValue.slice(0, cursorPosition);

            const validCharactersBeforeCursor = [
                ...textBeforeCursor
            ].filter(char => allowedCharactersRegex.test(char))
                .length;

            // --------------------------------------------------
            // 2. تنظيف القيمة بالكامل
            // --------------------------------------------------

            const rawValue = [
                ...currentValue
            ].filter(char => allowedCharactersRegex.test(char));

            // --------------------------------------------------
            // 3. إضافة Space بين كل Character
            // --------------------------------------------------

            const formattedValue = rawValue.join(' ');

            // --------------------------------------------------
            // 4. تحديث القيمة فقط إذا تغيرت
            // --------------------------------------------------

            if (inputElement.value !== formattedValue) {
                inputElement.value = formattedValue;
            }

            // --------------------------------------------------
            // 5. حساب مكان الـ Cursor الجديد
            // --------------------------------------------------

            let newCursorPosition = 0;

            if (validCharactersBeforeCursor > 0) {

                let characterCounter = 0;

                for (let i = 0; i < formattedValue.length; i++) {

                    if (allowedCharactersRegex.test(formattedValue[i])) {
                        characterCounter++;
                    }

                    if (characterCounter === validCharactersBeforeCursor) {
                        newCursorPosition = i + 1;
                        break;
                    }
                }

            } else {
                newCursorPosition = 0;
            }

            // --------------------------------------------------
            // 6. حماية الـ Cursor من تجاوز طول النص
            // --------------------------------------------------

            newCursorPosition = Math.min(
                newCursorPosition,
                formattedValue.length
            );

            // --------------------------------------------------
            // 7. إعادة الـ Cursor لمكانه
            // --------------------------------------------------

            requestAnimationFrame(() => {
                try {
                    inputElement.setSelectionRange(
                        newCursorPosition,
                        newCursorPosition
                    );
                } catch (error) {
                    console.warn('Could not restore cursor position:', error);
                }
            });
        });
    };


    // ==========================================
    // 9.1 حقل حروف لوحة السيارة
    // ==========================================

    const plateLettersInput = document.getElementById('plateLetters');

    setupPlateFormatting(
        plateLettersInput,
        /[\u0600-\u06FF]/
    );


    // ==========================================
    // 9.2 حقل أرقام لوحة السيارة
    // ==========================================

    const plateNumbersInput = document.getElementById('plateNumbers');

    setupPlateFormatting(
        plateNumbersInput,
        /[0-9]/
    );

    // ==========================================
    // 10. التعامل مع مودال إضافة سائق جديد المباشر (+)
    // ==========================================
    const addDriverModal = document.getElementById('add-driver-modal');
    const openAddDriverModalBtn = document.getElementById('btn-open-inline-driver-modal');
    const btnSaveInlineDriver = document.getElementById('btn-save-inline-driver');
    const btnCancelAddDriver = document.getElementById('btn-cancel-add-driver');

    document.addEventListener('click', (e) => {
        const targetBtn = e.target.closest('#btn-open-inline-driver-modal, .add-driver-btn');
        if (targetBtn) {
            e.preventDefault();
            e.stopPropagation();
            const modal = document.getElementById('add-driver-modal');
            if (modal) {
                showConfirmationModal(modal);
            }
        }
    });

    if (btnCancelAddDriver && addDriverModal) {
        btnCancelAddDriver.addEventListener('click', () => {
            hideConfirmationModal(addDriverModal);
        });
    }

    if (btnSaveInlineDriver) {
        btnSaveInlineDriver.addEventListener('click', () => {
            const nameInput = document.getElementById('inlineDriverFullName');
            const natIdInput = document.getElementById('inlineDriverNationalId');
            const phoneInput = document.getElementById('inlineDriverPhone');
            const nameErr = document.getElementById('inlineDriverNameErr');
            const natIdErr = document.getElementById('inlineDriverNationalIdErr');
            const phoneErr = document.getElementById('inlineDriverPhoneErr');

            let isValid = true;
            if (!nameInput || !nameInput.value.trim()) {
                if (nameErr) nameErr.style.display = 'block';
                isValid = false;
            } else {
                if (nameErr) nameErr.style.display = 'none';
            }

            if (!natIdInput || natIdInput.value.trim().length !== 14) {
                if (natIdErr) natIdErr.style.display = 'block';
                isValid = false;
            } else {
                if (natIdErr) natIdErr.style.display = 'none';
            }

            if (!phoneInput || !phoneInput.value.trim()) {
                if (phoneErr) phoneErr.style.display = 'block';
                isValid = false;
            } else {
                if (phoneErr) phoneErr.style.display = 'none';
            }

            if (!isValid) return;

            let inputName = document.getElementById('hiddenNewDriverName');
            if (!inputName) {
                inputName = document.createElement('input');
                inputName.type = 'hidden';
                inputName.id = 'hiddenNewDriverName';
                inputName.name = 'NewDriverName';
                form.appendChild(inputName);
            }
            inputName.value = nameInput.value.trim();

            let inputNatId = document.getElementById('hiddenNewDriverNationalId');
            if (!inputNatId) {
                inputNatId = document.createElement('input');
                inputNatId.type = 'hidden';
                inputNatId.id = 'hiddenNewDriverNationalId';
                inputNatId.name = 'NewDriverNationalId';
                form.appendChild(inputNatId);
            }
            inputNatId.value = natIdInput.value.trim();

            let inputPhone = document.getElementById('hiddenNewDriverPhone');
            if (!inputPhone) {
                inputPhone = document.createElement('input');
                inputPhone.type = 'hidden';
                inputPhone.id = 'hiddenNewDriverPhone';
                inputPhone.name = 'NewDriverPhone';
                form.appendChild(inputPhone);
            }
            inputPhone.value = phoneInput.value.trim();

            const pNumbersInput = document.getElementById('plateNumbers');
            const pLettersInput = document.getElementById('plateLetters');
            const selectedCompanyText = document.getElementById('companyDisplay')?.value || "";
            const fullPlate = `${pLettersInput ? pLettersInput.value.trim() : ""} ${pNumbersInput ? pNumbersInput.value.trim() : ""}`;

            pendingTruckInfo = {
                company: selectedCompanyText,
                truckPlate: fullPlate,
                driverName: nameInput.value.trim()
            };

            hideConfirmationModal(addDriverModal);
            setTimeout(() => {
                showConfirmationModal(deptModal);
            }, 200);
        });
    }

    const pendingTruckDataRaw = sessionStorage.getItem('pendingTruckData');
    if (pendingTruckDataRaw) {
        try {
            const pendingTruckData = JSON.parse(pendingTruckDataRaw);

            const pNumbersInput = document.getElementById('plateNumbers');
            const pLettersInput = document.getElementById('plateLetters');
            const sCapInput = document.getElementById('storageCapacity');
            const tCoolingInput = document.getElementById('typeCooling');
            const tNormalInput = document.getElementById('typeNormal');

            if (pNumbersInput && pendingTruckData.plateNumbers !== undefined && pendingTruckData.plateNumbers !== '') {
                pNumbersInput.value = pendingTruckData.plateNumbers;
                pNumbersInput.dispatchEvent(new Event('input', { bubbles: true }));
            }
            if (pLettersInput && pendingTruckData.plateLetters !== undefined && pendingTruckData.plateLetters !== '') {
                pLettersInput.value = pendingTruckData.plateLetters;
                pLettersInput.dispatchEvent(new Event('input', { bubbles: true }));
            }
            if (sCapInput && pendingTruckData.storageCapacity !== undefined && pendingTruckData.storageCapacity !== '') {
                sCapInput.value = pendingTruckData.storageCapacity;
            }
            if (pendingTruckData.isRefrigerated !== undefined) {
                if (pendingTruckData.isRefrigerated) {
                    if (tCoolingInput) tCoolingInput.checked = true;
                } else {
                    if (tNormalInput) tNormalInput.checked = true;
                }
            if (window.autoOpenModal || pendingTruckData.autoOpenModal) {
                setTimeout(() => {
                    const selectedCompanyText = document.getElementById('companyDisplay')?.value || "";
                    const fullPlate = `${pLettersInput ? pLettersInput.value.trim() : ""} ${pNumbersInput ? pNumbersInput.value.trim() : ""}`;
                    const driverSelectedSpan = document.querySelector('#customDriverDropdown .selected-value');
                    const driverNameText = (driverSelectedSpan && !driverSelectedSpan.classList.contains('placeholder-color')) ? driverSelectedSpan.textContent.trim() : "السائق الجديد";

                    pendingTruckInfo = {
                        company: selectedCompanyText,
                        truckPlate: fullPlate,
                        driverName: driverNameText
                    };

                    selectedDepartmentValue = null;
                    if (deptSelectedValue) {
                        deptSelectedValue.textContent = 'اختر القسم';
                        deptSelectedValue.classList.add('text-muted');
                    }

                    if (deptModal) {
                        showConfirmationModal(deptModal);
                    }
                }, 350);
            }
        } catch (e) {
            console.error('Error restoring pendingTruckData:', e);
        }
        sessionStorage.removeItem('pendingTruckData');
    }
});