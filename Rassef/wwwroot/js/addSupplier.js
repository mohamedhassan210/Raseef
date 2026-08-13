document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('addSupplierForm');
    const nameInput = document.getElementById('supplierName');
    const phoneInput = document.getElementById('supplierPhone');
    const nameError = document.getElementById('nameError');
    const phoneError = document.getElementById('phoneError');
    const submitBtn = document.getElementById('submitBtn');

    // Logo Upload Logic (Exact exact behavior preserved)
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
    // منع كتابة أية أرقام غير صحيحة أثناء الكتابة فوراً

    const validatePhone = () => {
        const val = phoneInput.value.trim();

        // الـ Regex اللي بيفحص بداية الرقم (010, 011, 012, 015) والطول 11 رقم
        const phoneRegex = /^01[0125][0-9]{8}$/;

        if (val === '') {
            showError(phoneInput, phoneError, 'يرجى إدخال رقم الهاتف');
            return false;
        }
        else if (!phoneRegex.test(val)) {
            showError(phoneInput, phoneError, 'رقم الهاتف يجب أن يبدأ بـ (010 أو 011 أو 012 أو 015) ومكون من 11 رقماً');
            return false;
        }
        else {
            showSuccess(phoneInput, phoneError);
            return true;
        }
    };
    //const validatePhone = () => {
    //    const val = phoneInput.value.trim();
    //    if (val === '') {
    //        showError(phoneInput, phoneError, 'يرجى إدخال رقم الهاتف');
    //        return false;
    //    } else if (val.length !== 11) {
    //        showError(phoneInput, phoneError, 'رقم الهاتف غير صحيح (يجب أن يكون 11 رقماً)');
    //        return false;
    //    } else {
    //        showSuccess(phoneInput, phoneError);
    //        return true;
    //    }
    //};

    const showError = (input, errorElement, message) => {
        input.classList.remove('is-valid');
        input.classList.add('is-invalid');

        // Handling ASP.NET Core MVC Span integration seamlessly
        const mvcSpan = errorElement.querySelector('span');
        if (mvcSpan) {
            mvcSpan.textContent = message;
        } else {
            errorElement.textContent = message;
        }
        errorElement.style.display = 'block';
    };

    const showSuccess = (input, errorElement) => {
        input.classList.remove('is-invalid');
        input.classList.add('is-valid');

        const mvcSpan = errorElement.querySelector('span');
        if (mvcSpan) {
            mvcSpan.textContent = '';
        } else {
            errorElement.textContent = '';
        }
        errorElement.style.display = 'none';
    };

    // Check Server-Side Validation Errors on page load
    const checkServerErrors = () => {
        const nameMvcSpan = nameError.querySelector('span');
        if (nameMvcSpan && nameMvcSpan.textContent.trim() !== '') {
            nameInput.classList.add('is-invalid');
            nameError.style.display = 'block';
        }

        const phoneMvcSpan = phoneError.querySelector('span');
        if (phoneMvcSpan && phoneMvcSpan.textContent.trim() !== '') {
            phoneInput.classList.add('is-invalid');
            phoneError.style.display = 'block';
        }
    };

    checkServerErrors();

    // Form Submission Handling
    form.addEventListener('submit', (e) => {
        const isNameValid = validateName();
        const isPhoneValid = validatePhone();

        // Prevent submission ONLY if invalid
        if (!isNameValid || !isPhoneValid) {
            e.preventDefault();
            return;
        }

        // Form is Valid -> Show Loading State & Submit to MVC Controller
        const btnText = submitBtn.querySelector('.btn-text');
        const spinner = submitBtn.querySelector('.spinner-border');

        btnText.textContent = 'جاري الحفظ...';
        spinner.classList.remove('d-none');

        // Prevent double clicking while form is natively submitted to the backend
        submitBtn.style.pointerEvents = 'none';

        // MVC Controller will automatically process and return RedirectToAction(nameof(Create))
    });
});