document.addEventListener('DOMContentLoaded', () => {

    const form = document.getElementById('finishSetupForm');

    const deptDropdownContainer = document.getElementById('deptDropdownContainer');
    const deptDropdownHeader = document.getElementById('deptDropdownHeader');
    const deptSelectedValue = document.getElementById('deptSelectedValue');
    const deptDropdownList = document.getElementById('deptDropdownList');
    const departmentInput = document.getElementById('departmentInput');
    const deptErrorMsg = document.getElementById('deptErrorMsg');

    // ==============================
    // Department Dropdown
    // ==============================

    const initDepartmentDropdown = () => {

        const deptItems =
            deptDropdownList.querySelectorAll('.dropdown-item');

        deptDropdownHeader.addEventListener('click', (e) => {

            e.stopPropagation();

            deptDropdownContainer.classList.toggle('open');

            deptDropdownHeader.classList.remove('border-danger');
            deptErrorMsg.style.display = 'none';
        });

        deptItems.forEach(item => {

            item.addEventListener('click', (e) => {

                e.stopPropagation();

                const value = item.getAttribute('data-value');

                departmentInput.value = value;

                deptSelectedValue.textContent = value;
                deptSelectedValue.classList.remove('text-muted');

                deptItems.forEach(el =>
                    el.classList.remove('selected')
                );

                item.classList.add('selected');

                deptDropdownContainer.classList.remove('open');

                deptDropdownHeader.classList.remove('border-danger');
                deptErrorMsg.style.display = 'none';
            });

        });

        document.addEventListener('click', (e) => {

            if (!deptDropdownContainer.contains(e.target)) {
                deptDropdownContainer.classList.remove('open');
            }

        });
    };

    initDepartmentDropdown();


    // ==============================
    // Form Validation
    // ==============================

    form.addEventListener('submit', (e) => {

        let isValid = true;

        // Validation القسم
        if (!departmentInput.value.trim()) {

            deptDropdownHeader.classList.add('border-danger');

            deptErrorMsg.style.display = 'block';

            isValid = false;

        } else {

            deptDropdownHeader.classList.remove('border-danger');

            deptErrorMsg.style.display = 'none';
        }


        // Validation باقي الـ inputs
        if (!form.checkValidity()) {

            e.preventDefault();

            e.stopPropagation();

            isValid = false;
        }


        // ==============================
        // Validation Failed
        // ==============================

        if (!isValid) {

            e.preventDefault();

            form.classList.add('was-validated');

            return;
        }


        // ==============================
        // Validation Successful
        // ==============================

        e.preventDefault();

        window.location.href =
            window.appRoutes.transferDrivers;
    });


    // ==============================
    // Remove Input Errors
    // ==============================

    const textInputs =
        form.querySelectorAll('input[type="text"]');

    textInputs.forEach(input => {

        input.addEventListener('input', () => {

            if (input.value.trim() !== '') {

                input.classList.remove('is-invalid');

                input.setCustomValidity('');
            }

        });

    });

});