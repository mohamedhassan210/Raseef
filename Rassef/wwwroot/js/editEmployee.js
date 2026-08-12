document.addEventListener('DOMContentLoaded', async () => {

    // 1. Elements Definition
    const form = document.getElementById('editEmployeeForm');
    const loadingState = document.getElementById('loadingState');
    const errorState = document.getElementById('errorState');
    const formSection = document.getElementById('formSection');
    const btnSubmitEdit = document.getElementById('btnSubmitEdit');
    const btnBackToDetails = document.getElementById('btnBackToDetails');
    const btnErrorBack = document.getElementById('btnErrorBack');
    const customDropdowns = document.querySelectorAll('.custom-dropdown-container');

    // Inputs
    const inpName = document.getElementById('empName');
    const inpPhone = document.getElementById('empPhone');
    const inpEmail = document.getElementById('empEmail');
    const inpNationalId = document.getElementById('empNationalId');
    const selectRole = document.getElementById('empRoleSelect');
    const selectDept = document.getElementById('empDeptSelect');

    let currentEmployeeId = null;
    let employeeCodeBackup = null; // للاحتفاظ بالكود الداخلي للموظف

    // 2. Read ID from URL
    const params = new URLSearchParams(window.location.search);
    currentEmployeeId = Number(params.get('id'));

    if (!currentEmployeeId) {
        showError();
        return;
    }

    // 3. Init Data & UI
    try {
        const employee = await EmployeesAPI.getById(currentEmployeeId);
        if (employee) {
            populateForm(employee);
            initializeDropdowns();
            initializeValidation();
            showForm();
        } else {
            showError();
        }
    } catch (error) {
        console.error("Error fetching employee:", error);
        showError();
    }

    // 4. Populate Form
    function populateForm(emp) {
        inpName.value = emp.name || "";
        inpPhone.value = emp.phone || "";
        inpEmail.value = emp.email || "";
        inpNationalId.value = emp.nationalId || "";
        employeeCodeBackup = emp.code; // نحتفظ بالكود لأنه غير قابل للتعديل بالواجهة

        // تعبئة Dropdowns
        setDropdownValue(selectRole, 'roleDropdown', 'roleSelectedText', emp.role);
        setDropdownValue(selectDept, 'deptDropdown', 'deptSelectedText', emp.department || "المخازن"); // Fallback
    }

    function setDropdownValue(hiddenSelect, dropdownId, textId, value) {
        if (!value) return;

        hiddenSelect.value = value;
        const textSpan = document.getElementById(textId);
        textSpan.textContent = value;
        textSpan.classList.remove('text-muted');

        const dropdown = document.getElementById(dropdownId);
        const items = dropdown.querySelectorAll('.dropdown-item');
        items.forEach(item => {
            if (item.getAttribute('data-value') === value) {
                item.classList.add('selected');
            } else {
                item.classList.remove('selected');
            }
        });
    }

    // 5. State Management UI
    function showForm() {
        loadingState.classList.add('d-none');
        errorState.classList.add('d-none');
        formSection.classList.remove('d-none');
    }

    function showError() {
        loadingState.classList.add('d-none');
        formSection.classList.add('d-none');
        errorState.classList.remove('d-none');
    }

    // 6. Navigation
    function goBack(targetId = currentEmployeeId) {
        if (window.mvcRoutes && window.mvcRoutes.employeeDetailsUrl) {
            window.location.href = `${window.mvcRoutes.employeeIndexUrl}?id=${targetId || ''}`;
        } else {
            window.location.href = `Index.cshtml?id=${targetId || ''}`;
        }
    }

    btnBackToDetails.addEventListener('click', goBack);
    btnErrorBack.addEventListener('click', goBack);

    // 7. Initialize Custom Dropdowns (Logic Matched with AddEmployee)
    function initializeDropdowns() {
        customDropdowns.forEach(container => {
            const dropdown = container.querySelector('.custom-dropdown');
            const header = dropdown.querySelector('.dropdown-header');
            const list = dropdown.querySelector('.dropdown-list');
            const items = dropdown.querySelectorAll('.dropdown-item');
            const selectedValueText = dropdown.querySelector('.selected-value');
            const hiddenSelect = container.querySelector('.visually-hidden-select');

            header.addEventListener('click', (e) => {
                e.stopPropagation();
                const isOpen = dropdown.classList.contains('open');
                closeAllDropdowns();

                if (!isOpen) {
                    dropdown.classList.add('open');
                    dropdown.setAttribute('aria-expanded', 'true');
                    adjustDropdownPosition(dropdown, list);
                }
            });

            items.forEach(item => {
                item.addEventListener('click', (e) => {
                    e.stopPropagation();
                    const value = item.getAttribute('data-value');

                    selectedValueText.textContent = value;
                    selectedValueText.classList.remove('text-muted');
                    hiddenSelect.value = value;

                    items.forEach(el => el.classList.remove('selected'));
                    item.classList.add('selected');

                    if (form.classList.contains('was-validated')) {
                        dropdown.classList.remove('is-invalid-dropdown');
                    }

                    dropdown.classList.remove('open');
                    dropdown.setAttribute('aria-expanded', 'false');
                });
            });
        });

        document.addEventListener('click', closeAllDropdowns);
    }

    function closeAllDropdowns() {
        document.querySelectorAll('.custom-dropdown.open').forEach(dropdown => {
            dropdown.classList.remove('open');
            dropdown.setAttribute('aria-expanded', 'false');
        });
    }

    function adjustDropdownPosition(dropdown, list) {
        const rect = dropdown.getBoundingClientRect();
        const listHeight = 220;
        const spaceBelow = window.innerHeight - rect.bottom;

        if (spaceBelow < listHeight && rect.top > listHeight) {
            list.style.top = 'auto';
            list.style.bottom = 'calc(100% + 6px)';
            list.style.transformOrigin = 'bottom center';
        } else {
            list.style.top = 'calc(100% + 6px)';
            list.style.bottom = 'auto';
            list.style.transformOrigin = 'top center';
        }
    }

    // 8. Validation Logic
    function initializeValidation() {
        const inputs = form.querySelectorAll('input');
        inputs.forEach(input => {
            // Live validation on blur and input if already validated
            ['blur', 'input'].forEach(evt => {
                input.addEventListener(evt, () => {
                    if (form.classList.contains('was-validated')) {
                        input.checkValidity();
                    }
                });
            });
        });
    }

    // 9. Form Submit
    form.addEventListener('submit', async (event) => {
        event.preventDefault();

        let isValid = true;

        // Custom dropdown validation
        customDropdowns.forEach(container => {
            const hiddenSelect = container.querySelector('.visually-hidden-select');
            const dropdown = container.querySelector('.custom-dropdown');

            if (!hiddenSelect.value) {
                dropdown.classList.add('is-invalid-dropdown');
                isValid = false;
            } else {
                dropdown.classList.remove('is-invalid-dropdown');
            }
        });

        if (!form.checkValidity() || !isValid) {
            event.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        // All Valid -> Prepare Update
        const updatedEmployee = {
            id: currentEmployeeId,
            name: inpName.value.trim(),
            phone: inpPhone.value.trim(),
            email: inpEmail.value.trim(),
            nationalId: inpNationalId.value.trim(),
            role: selectRole.value,
            department: selectDept.value,
            code: employeeCodeBackup // الحفاظ على الكود القديم
        };

        // Disable button & show loading state on button
        btnSubmitEdit.disabled = true;
        const originalBtnText = btnSubmitEdit.textContent;
        btnSubmitEdit.textContent = "جاري حفظ التعديلات...";

        try {
            await EmployeesAPI.update(currentEmployeeId, updatedEmployee);
            if (typeof showToast === 'function') {
                showToast('تم تعديل بيانات الموظف بنجاح', 'success');
            }
            // هيستنى ثانية ويروح لصفحة التفاصيل الخاصة بنفس الموظف
            setTimeout(() => goBack(currentEmployeeId), 1000);
        } catch (error) {
            console.error("Error updating employee:", error);
            if (typeof showToast === 'function') {
                showToast('حدث خطأ أثناء تعديل بيانات الموظف', 'error');
            }
            btnSubmitEdit.disabled = false;
            btnSubmitEdit.textContent = originalBtnText;
        }
    });

});