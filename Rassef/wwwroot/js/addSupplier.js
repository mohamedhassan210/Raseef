document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('addSupplierForm');
    const nameInput = document.getElementById('supplierName');
    const phoneInput = document.getElementById('supplierPhone');
    const nameError = document.getElementById('nameError');
    const phoneError = document.getElementById('phoneError');
    const submitBtn = document.getElementById('submitBtn');
    
    // Logo Upload Logic
    const logoTrigger = document.getElementById('logoUploadTrigger');
    const logoInput = document.getElementById('supplierLogoInput');

    logoTrigger.addEventListener('click', () => {
        logoInput.click();
    });

    logoInput.addEventListener('change', (e) => {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (event) => {
                logoTrigger.innerHTML = `<img src="${event.target.result}" alt="شعار المورد المعاين">`;
            };
            reader.readAsDataURL(file);
        }
    });

    // Real-time Validation for Name
    nameInput.addEventListener('input', () => {
        validateName();
    });

    // Real-time Validation for Phone (Numbers Only)
    phoneInput.addEventListener('input', (e) => {
        e.target.value = e.target.value.replace(/\D/g, '');
        validatePhone();
    });

    const validateName = () => {
        const val = nameInput.value.trim();
        if (val === '') {
            showError(nameInput, nameError, 'يرجى إدخال اسم المورد');
            return false;
        } else if (val.length < 3) {
            showError(nameInput, nameError, 'اسم المورد يجب ألا يقل عن 3 أحرف');
            return false;
        } else {
            showSuccess(nameInput, nameError);
            return true;
        }
    };

    const validatePhone = () => {
        const val = phoneInput.value.trim();
        if (val === '') {
            showError(phoneInput, phoneError, 'يرجى إدخال رقم الهاتف');
            return false;
        } else if (val.length !== 11) {
            showError(phoneInput, phoneError, 'رقم الهاتف غير صحيح (يجب أن يكون 11 رقماً)');
            return false;
        } else {
            showSuccess(phoneInput, phoneError);
            return true;
        }
    };

    const showError = (input, errorElement, message) => {
        input.classList.remove('is-valid');
        input.classList.add('is-invalid');
        errorElement.textContent = message;
        errorElement.style.display = 'block';
    };

    const showSuccess = (input, errorElement) => {
        input.classList.remove('is-invalid');
        input.classList.add('is-valid');
        errorElement.textContent = '';
        errorElement.style.display = 'none';
    };

    // Form Submission Handling
    form.addEventListener('submit', (e) => {
        e.preventDefault();

        const isNameValid = validateName();
        const isPhoneValid = validatePhone();

        if (isNameValid && isPhoneValid) {
            // Show Loading State Inside Button
            const btnText = submitBtn.querySelector('.btn-text');
            const spinner = submitBtn.querySelector('.spinner-border');

            btnText.textContent = 'جاري الحفظ...';
            spinner.classList.remove('d-none');
            submitBtn.disabled = true;

            // Simulate Network Request Delay
            setTimeout(() => {
                window.location.href = 'suppliers.html';
            }, 1000);
        }
    });
});