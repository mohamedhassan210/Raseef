document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('loginForm');
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');

    const usernameError = document.getElementById('usernameError');
    const passwordError = document.getElementById('passwordError');
    const formAlert = document.getElementById('formAlert');
    const submitBtn = form.querySelector('button[type="submit"]');

    function showError(input, errorElement, message) {
        input.classList.add('is-invalid');
        errorElement.textContent = message;
    }

    function clearError(input, errorElement) {
        input.classList.remove('is-invalid');
        errorElement.textContent = '';
    }

    usernameInput.addEventListener('input', () => clearError(usernameInput, usernameError));
    passwordInput.addEventListener('input', () => clearError(passwordInput, passwordError));

    form.addEventListener('submit', function (event) {
        let isValid = true;

        const usernameValue = usernameInput.value.trim();
        if (usernameValue === '') {
            showError(usernameInput, usernameError, 'يرجى إدخال اسم المستخدم أو البريد الإلكتروني.');
            isValid = false;
        } else if (usernameValue.length < 3) {
            showError(usernameInput, usernameError, 'يجب أن يكون اسم المستخدم 3 حروف على الأقل.');
            isValid = false;
        } else {
            clearError(usernameInput, usernameError);
        }

        const passwordValue = passwordInput.value.trim();
        if (passwordValue === '') {
            showError(passwordInput, passwordError, 'يرجى إدخال كلمة المرور.');
            isValid = false;
        }
        else {
            clearError(passwordInput, passwordError);
        }

        if (!isValid) {
            event.preventDefault();
            formAlert.classList.remove('d-none');
        } else {

            formAlert.classList.add('d-none');

            submitBtn.textContent = 'جاري التحقق من البيانات...';
            submitBtn.classList.add('disabled');

        }
    });
});