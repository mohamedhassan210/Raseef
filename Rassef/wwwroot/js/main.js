document.addEventListener('DOMContentLoaded', function () {
const form = document.getElementById('loginForm');
const usernameInput = document.getElementById('username');
const passwordInput = document.getElementById('password');
 
const usernameError = document.getElementById('usernameError');
const passwordError = document.getElementById('passwordError');
const formAlert = document.getElementById('formAlert');

// وظيفة إظهار الخطأ لحقل معين
function showError(input, errorElement, message) {
  input.classList.add('is-invalid');
  errorElement.textContent = message;
}

// وظيفة مسح الخطأ عن حقل معين
function clearError(input, errorElement) {
  input.classList.remove('is-invalid');
  errorElement.textContent = '';
}

// إزالة الأخطاء فوراً عند بدء الكتابة
usernameInput.addEventListener('input', () => clearError(usernameInput, usernameError));
passwordInput.addEventListener('input', () => clearError(passwordInput, passwordError));

// معالجة الضغط على زر التسجيل
form.addEventListener('submit', function (event) {
  event.preventDefault(); // منع إعادة تحميل الصفحة

  let isValid = true;

  // 1. الفحص الخاص باسم المستخدم
  const usernameValue = usernameInput.value.trim();
  if (usernameValue === '') {
    showError(usernameInput, usernameError, 'يرجى إدخال اسم المستخدم.');
    isValid = false;
  } else if (usernameValue.length < 3) {
    showError(usernameInput, usernameError, 'يجب أن يكون اسم المستخدم 3 حروف على الأقل.');
    isValid = false;
  } else {
    clearError(usernameInput, usernameError);
  }

  // 2. الفحص الخاص بكلمة المرور
  const passwordValue = passwordInput.value.trim();
  if (passwordValue === '') {
    showError(passwordInput, passwordError, 'يرجى إدخال كلمة المرور.');
    isValid = false;
  } else if (passwordValue.length < 6) {
    showError(passwordInput, passwordError, 'كلمة المرور يجب ألا تقل عن 6 رموز.');
    isValid = false;
  } else {
    clearError(passwordInput, passwordError);
  }

  // 3. التفاعل مع النتيجة النهائية
  if (!isValid) {
    // إظهار التنبيه أسفل الزر عند وجود أخطاء
    formAlert.classList.remove('d-none');
  } else {
    // إخفاء التنبيه والتجهيز للإرسال
    formAlert.classList.add('d-none');
    
    // هنا يمكنك الربط مع الـ API أو السيرفر
    alert('تمت العملية بنجاح! جاري تسجيل الدخول...');
    // form.submit(); // لاستخدام الإرسال العادي
  }
});
});
