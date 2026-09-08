document.addEventListener('DOMContentLoaded', () => {

    // ==========================================
    // 1. DOM Elements (Form & Dropdowns)
    // ==========================================
    const editCompanyBtn = document.getElementById('editCompanyBtn');
    const companyStaticView = document.getElementById('companyStaticView');
    const companySelectView = document.getElementById('companySelectView');
    const companySelect = document.getElementById('companySelect');
    const submitBtn = document.getElementById('submit-btn');
    const form = document.getElementById('addCarForm');

    const plateNumbersInput = document.getElementById('plateNumbers');
    const plateLettersInput = document.getElementById('plateLetters');
    const storageCapacityInput = document.getElementById('storageCapacity');
    const driverSelect = document.getElementById('driverSelect');

    // ==========================================
    // 2. DOM Elements (Modals)
    // ==========================================
    const modalOverlay = document.getElementById('custom-modal-overlay');
    const deptModal = document.getElementById('department-modal');
    const confirmModal = document.getElementById('confirmation-modal');
    const successModal = document.getElementById('success-modal');
    const addDriverModal = document.getElementById('add-driver-modal');

    const btnOpenAddDriverModal = document.getElementById('btn-open-inline-driver-modal');
    const btnSaveInlineDriver = document.getElementById('btn-save-inline-driver');
    const btnCancelAddDriver = document.getElementById('btn-cancel-add-driver');

    const btnConfirmDept = document.getElementById('btn-confirm-dept');
    const btnEdit = document.getElementById('btn-edit');
    const btnConfirm = document.getElementById('btn-confirm');
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
    let pendingTruckInfo = {}; // بيانات تأكيد العرض في المودال

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
    // 4. Custom Dropdowns Logic (Driver & Company)
    // ==========================================
    document.querySelectorAll('.custom-dropdown').forEach(customDropdown => {
        if (customDropdown.id === 'dept-dropdown-container') return;
        // CHANGED — customDriverDropdown excluded here too. Its items are no longer
        // static (they're re-rendered from AJAX search results), and this loop only
        // wires whatever .dropdown-item elements exist at page load. Section 4b below
        // owns 100% of the driver dropdown's wiring instead, so it applies identically
        // to both the initial server-rendered items and every search re-render.
        if (customDropdown.id === 'customDriverDropdown') return;

        const dropdownHeader = customDropdown.querySelector('.dropdown-header');
        const selectedValue = customDropdown.querySelector('.selected-value');
        const dropdownItems = customDropdown.querySelectorAll('.dropdown-item');

        const container = customDropdown.closest('.col-md-6') || customDropdown.parentElement;
        const nativeSelect = container ? container.querySelector('select') : null;

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
                if (item.classList.contains('disabled')) return;

                const value = item.getAttribute('data-value');
                const text = item.textContent.trim();

                if (selectedValue) {
                    selectedValue.textContent = text;
                    selectedValue.classList.remove('placeholder-color');
                    selectedValue.classList.remove('text-muted');
                }

                if (nativeSelect) {
                    nativeSelect.value = value;
                    nativeSelect.disabled = false;
                    nativeSelect.dispatchEvent(new Event('change', { bubbles: true }));
                }

                dropdownItems.forEach(el => el.classList.remove('selected'));
                item.classList.add('selected');
                customDropdown.classList.remove('open');

                // إزالة علامة الخطأ عند الاختيار
                const invalidFeedback = container ? container.querySelector('.invalid-feedback') : null;
                if (invalidFeedback) invalidFeedback.style.display = 'none';
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
    // 4b. Driver Search & Dynamic Dropdown Logic
    // ==========================================
    const driverDropdown = document.getElementById('customDriverDropdown');
    const driverDropdownHeader = driverDropdown ? driverDropdown.querySelector('.dropdown-header') : null;
    const driverSelectedValueSpan = driverDropdown ? driverDropdown.querySelector('.selected-value') : null;
    const driverSearchInput = document.getElementById('driverSearchInput');
    const driverDropdownItemsContainer = document.getElementById('driverDropdownItems');
    const driverInvalidFeedback = document.getElementById('driverSelectView')
        ? document.getElementById('driverSelectView').querySelector('.invalid-feedback')
        : null;

    let driverSearchDebounceTimer = null;

    // CHANGED — replicates the open/close behavior the generic loop used to provide
    // for this dropdown's header before it was excluded above.
    if (driverDropdownHeader) {
        driverDropdownHeader.addEventListener('click', (e) => {
            e.stopPropagation();
            document.querySelectorAll('.custom-dropdown').forEach(d => {
                if (d !== driverDropdown) d.classList.remove('open');
            });
            if (driverDropdown) driverDropdown.classList.toggle('open');
        });
    }

    const wireDriverItem = (item) => {
        item.addEventListener('click', (e) => {
            e.stopPropagation();
            if (item.classList.contains('disabled')) return;

            const value = item.getAttribute('data-value');
            const text = item.textContent.trim();

            if (driverSelectedValueSpan) {
                driverSelectedValueSpan.textContent = text;
                driverSelectedValueSpan.classList.remove('placeholder-color');
                driverSelectedValueSpan.classList.remove('text-muted');
            }

            if (driverSelect) {
                driverSelect.value = value;
                driverSelect.disabled = false;
                driverSelect.dispatchEvent(new Event('change', { bubbles: true }));
            }

            if (driverDropdownItemsContainer) {
                driverDropdownItemsContainer.querySelectorAll('.dropdown-item').forEach(el => el.classList.remove('selected'));
            }
            item.classList.add('selected');

            if (driverDropdown) driverDropdown.classList.remove('open');

            if (driverInvalidFeedback) driverInvalidFeedback.style.display = 'none';
        });
    };

    const renderDriverItems = (drivers) => {
        if (!driverDropdownItemsContainer) return;

        driverDropdownItemsContainer.innerHTML = '';

        if (!drivers || drivers.length === 0) {
            const emptyItem = document.createElement('div');
            emptyItem.className = 'dropdown-item disabled text-muted';
            emptyItem.textContent = 'لا يوجد سائقين مطابقين';
            driverDropdownItemsContainer.appendChild(emptyItem);
            return;
        }

        drivers.forEach(driver => {
            const item = document.createElement('div');
            item.className = 'dropdown-item';
            item.setAttribute('data-value', driver.id);
            item.textContent = driver.fullName;
            wireDriverItem(item);
            driverDropdownItemsContainer.appendChild(item);
        });
    };

    // Wire whatever items the server already rendered on page load (the initial
    // top 6), so they behave identically to AJAX-rendered ones.
    if (driverDropdownItemsContainer) {
        driverDropdownItemsContainer.querySelectorAll('.dropdown-item').forEach(item => {
            if (!item.classList.contains('disabled')) {
                wireDriverItem(item);
            }
        });
    }

    if (driverSearchInput) {
        // Prevent typing/clicking in the search box from being treated as an
        // outside-click that would close the dropdown (handled above).
        driverSearchInput.addEventListener('click', (e) => e.stopPropagation());

        driverSearchInput.addEventListener('input', () => {
            const term = driverSearchInput.value.trim();

            clearTimeout(driverSearchDebounceTimer);
            driverSearchDebounceTimer = setTimeout(() => {
                fetch(`/SupplierRequest/SearchDrivers?term=${encodeURIComponent(term)}`)
                    .then(response => {
                        if (!response.ok) throw new Error('Search request failed');
                        return response.json();
                    })
                    .then(data => renderDriverItems(data))
                    .catch(() => renderDriverItems([]));
            }, 300);
        });
    }

    // ==========================================
    // 5. Modal System Functions
    // ==========================================
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

    const populateConfirmationData = (departmentName = "غير متوفر") => {
        if (confirmCompany) confirmCompany.textContent = pendingTruckInfo.company || "غير متوفر";
        if (confirmTruck) confirmTruck.textContent = pendingTruckInfo.truckPlate || "غير متوفر";
        if (confirmDriverName) confirmDriverName.textContent = pendingTruckInfo.driverName || "غير محدد";
        if (confirmDriverdep) confirmDriverdep.textContent = departmentName;
    };

    // ==========================================
    // 6. Department Custom Dropdown Logic (المودال)
    // ==========================================
    const initDepartmentDropdown = () => {
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
                    id: parseInt(item.getAttribute('data-id') || item.getAttribute('data-value')),
                    name: item.textContent.trim()
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
                    if (deptDropdownHeader) deptDropdownHeader.classList.add('error');
                    if (deptErrorMsg) deptErrorMsg.style.display = 'block';
                    if (deptDropdownHeader) {
                        deptDropdownHeader.style.animation = 'shake 0.4s';
                        setTimeout(() => deptDropdownHeader.style.animation = '', 400);
                    }
                    return;
                }
                populateConfirmationData(selectedDepartmentValue.name);
                showConfirmationModal(confirmModal);
            });
        }
    };

    initDepartmentDropdown();

    // ==========================================
    // 7. Validation Helpers
    // ==========================================
    const validateTruckFields = () => {
        let isValid = true;

        if (!plateNumbersInput || !plateNumbersInput.value.trim()) {
            if (plateNumbersInput) plateNumbersInput.classList.add('is-invalid');
            isValid = false;
        } else {
            if (plateNumbersInput) plateNumbersInput.classList.remove('is-invalid');
        }

        if (!plateLettersInput || !plateLettersInput.value.trim()) {
            if (plateLettersInput) plateLettersInput.classList.add('is-invalid');
            isValid = false;
        } else {
            if (plateLettersInput) plateLettersInput.classList.remove('is-invalid');
        }

        if (!storageCapacityInput || !storageCapacityInput.value.trim() || parseFloat(storageCapacityInput.value) <= 0) {
            if (storageCapacityInput) storageCapacityInput.classList.add('is-invalid');
            isValid = false;
        } else {
            if (storageCapacityInput) storageCapacityInput.classList.remove('is-invalid');
        }

        if (!isValid) {
            form.classList.add('was-validated');
            // التركيز على أول حقل غير صالح
            const firstInvalid = form.querySelector('.is-invalid, :invalid');
            if (firstInvalid) firstInvalid.focus();
        }

        return isValid;
    };

    const getFullPlateString = () => {
        const letters = plateLettersInput ? plateLettersInput.value.trim() : "";
        const numbers = plateNumbersInput ? plateNumbersInput.value.trim() : "";
        return `${letters} ${numbers}`.trim();
    };

    const getSelectedCompanyName = () => {
        const isSelectVisible = companySelectView && !companySelectView.classList.contains('d-none');
        if (isSelectVisible) {
            const selectedDropdown = companySelectView.querySelector('.selected-value');
            return selectedDropdown ? selectedDropdown.textContent.trim() : "";
        }
        return document.getElementById('companyDisplay')?.value || "";
    };

    // ==========================================
    // 8. زر إضافة سائق جديد (+)
    //    يتحقق من ملء بيانات الشاحنة أولاً!
    // ==========================================
    const handleOpenAddDriver = (e) => {
        if (e) {
            e.preventDefault();
            e.stopPropagation();
        }

        // 1. التحقق من ملء بيانات الشاحنة أولاً
        if (!validateTruckFields()) {
            return;
        }

        // 2. إعادة ضبط حقول مودال السائق
        const nameInput = document.getElementById('inlineDriverFullName');
        const natIdInput = document.getElementById('inlineDriverNationalId');
        const phoneInput = document.getElementById('inlineDriverPhone');
        if (nameInput) nameInput.value = '';
        if (natIdInput) natIdInput.value = '';
        if (phoneInput) phoneInput.value = '';

        const nameErr = document.getElementById('inlineDriverNameErr');
        const natIdErr = document.getElementById('inlineDriverNationalIdErr');
        const phoneErr = document.getElementById('inlineDriverPhoneErr');
        if (nameErr) nameErr.style.display = 'none';
        if (natIdErr) natIdErr.style.display = 'none';
        if (phoneErr) phoneErr.style.display = 'none';

        // 3. فتح مودال السائق
        if (addDriverModal) {
            showConfirmationModal(addDriverModal);
            setTimeout(() => {
                if (nameInput) nameInput.focus();
            }, 100);
        }
    };

    if (btnOpenAddDriverModal) {
        btnOpenAddDriverModal.addEventListener('click', handleOpenAddDriver);
    }

    document.addEventListener('click', (e) => {
        const targetBtn = e.target.closest('#btn-open-inline-driver-modal, .add-driver-btn');
        if (targetBtn) {
            handleOpenAddDriver(e);
        }
    });

    if (btnCancelAddDriver) {
        btnCancelAddDriver.addEventListener('click', () => {
            closeModal();
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

            // 1. فحص الاسم
            const nameVal = nameInput ? nameInput.value.trim() : '';
            if (!nameVal) {
                if (nameErr) nameErr.style.display = 'block';
                nameInput?.classList.add('is-invalid');
                isValid = false;
            } else {
                if (nameErr) nameErr.style.display = 'none';
                nameInput?.classList.remove('is-invalid');
            }

            // 2. فحص الرقم القومي (14 رقماً بالضبط)
            const natIdVal = natIdInput ? natIdInput.value.trim() : '';
            if (!natIdVal || natIdVal.length !== 14 || !/^\d{14}$/.test(natIdVal)) {
                if (natIdErr) natIdErr.style.display = 'block';
                natIdInput?.classList.add('is-invalid');
                isValid = false;
            } else {
                if (natIdErr) natIdErr.style.display = 'none';
                natIdInput?.classList.remove('is-invalid');
            }

            // 3. فحص رقم الهاتف المصري (11 رقماً يبدأ بـ 010 أو 011 أو 012 أو 015)
            const phoneVal = phoneInput ? phoneInput.value.trim() : '';
            if (!phoneVal || !/^01[0125][0-9]{8}$/.test(phoneVal)) {
                if (phoneErr) phoneErr.style.display = 'block';
                phoneInput?.classList.add('is-invalid');
                isValid = false;
            } else {
                if (phoneErr) phoneErr.style.display = 'none';
                phoneInput?.classList.remove('is-invalid');
            }

            if (!isValid) return;

            // حفظ بيانات السائق الجديد في حقول hidden داخل الفورم
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

            // تحديث العرض على الشاشة
            const driverSelectedSpan = document.querySelector('#customDriverDropdown .selected-value');
            if (driverSelectedSpan) {
                driverSelectedSpan.textContent = nameInput.value.trim();
                driverSelectedSpan.classList.remove('placeholder-color');
            }

            pendingTruckInfo = {
                company: getSelectedCompanyName(),
                truckPlate: getFullPlateString(),
                driverName: nameInput.value.trim()
            };

            // الانتقال لمودال اختيار القسم
            selectedDepartmentValue = null;
            if (deptSelectedValue) {
                deptSelectedValue.textContent = 'اختر القسم';
                deptSelectedValue.classList.add('text-muted');
            }
            document.querySelectorAll('#dept-dropdown-list .dropdown-item').forEach(i => i.classList.remove('selected'));

            showConfirmationModal(deptModal);
        });
    }

    // ==========================================
    // 9. زر الإرسال الأساسي للفورم (إضافة شاحنة جديدة)
    // ==========================================
    if (form) {
        form.addEventListener('submit', (event) => {
            event.preventDefault();

            // التحقق من بيانات الشاحنة
            if (!validateTruckFields()) {
                event.stopPropagation();
                return;
            }

            // التحقق من اختيار السائق
            const hasNewDriver = document.getElementById('hiddenNewDriverName')?.value;
            const hasSelectedDriver = driverSelect && driverSelect.value && driverSelect.value !== '0';

            if (!hasNewDriver && !hasSelectedDriver) {
                const driverContainer = document.getElementById('driverSelectView');
                const feedback = driverContainer ? driverContainer.querySelector('.invalid-feedback') : null;
                if (feedback) feedback.style.display = 'block';
                return;
            }

            const driverSelectedSpan = document.querySelector('#customDriverDropdown .selected-value');
            const driverNameText = (driverSelectedSpan && !driverSelectedSpan.classList.contains('placeholder-color'))
                ? driverSelectedSpan.textContent.trim()
                : "غير محدد";

            pendingTruckInfo = {
                company: getSelectedCompanyName(),
                truckPlate: getFullPlateString(),
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
    // 10. أحداث المودالات وتأكيد الإرسال
    // ==========================================
    if (btnEdit) {
        btnEdit.addEventListener('click', () => {
            closeModal();
        });
    }

    if (btnConfirm) {
        btnConfirm.addEventListener('click', () => {
            btnConfirm.disabled = true;
            btnConfirm.innerText = "جاري الحفظ وإصدار الدور...";

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

    if (btnPrint) {
        btnPrint.addEventListener('click', () => {
            if (window.routes && window.routes.truckIndex) {
                window.location.href = window.routes.truckIndex;
            }
        });
    }

    if (modalOverlay) {
        modalOverlay.addEventListener('click', (e) => {
            if (e.target === modalOverlay) {
                if (!deptModal?.classList.contains('d-none') || !confirmModal?.classList.contains('d-none') || !addDriverModal?.classList.contains('d-none')) {
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
                closeModal();
            }
        }
    });

    // ==========================================
    // 11. Plate Inputs Formatting
    // ==========================================
    const setupPlateFormatting = (inputElement, allowedCharactersRegex) => {
        if (!inputElement) return;

        inputElement.addEventListener('input', () => {
            const currentValue = inputElement.value;
            const cursorPosition = inputElement.selectionStart ?? currentValue.length;

            const textBeforeCursor = currentValue.slice(0, cursorPosition);
            const validCharactersBeforeCursor = [...textBeforeCursor].filter(char => allowedCharactersRegex.test(char)).length;

            const rawValue = [...currentValue].filter(char => allowedCharactersRegex.test(char));
            const formattedValue = rawValue.join(' ');

            if (inputElement.value !== formattedValue) {
                inputElement.value = formattedValue;
            }

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

            newCursorPosition = Math.min(newCursorPosition, formattedValue.length);

            requestAnimationFrame(() => {
                try {
                    inputElement.setSelectionRange(newCursorPosition, newCursorPosition);
                } catch (error) {
                    console.warn('Could not restore cursor position:', error);
                }
            });
        });
    };

    setupPlateFormatting(plateLettersInput, /[\u0600-\u06FF]/);
    setupPlateFormatting(plateNumbersInput, /[0-9]/);
});