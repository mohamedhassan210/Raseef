// يتحكم في نافذة "أمر توريد" على شاشة اختيار الخدمة (توريد/تحويل):
// يفتح النافذة، يبحث عن المورد بالكود اللي المستخدم دخله، ولو اتلاقى بيوديه
// على طول لصفحة اختيار الشاحنة (Truck/Index) ومعاه المورد مختار بالفعل.
document.addEventListener('DOMContentLoaded', () => {
    const openBtn = document.getElementById('supply-order-btn');
    const overlay = document.getElementById('supply-order-overlay');
    const input = document.getElementById('supply-order-input');
    const errorBox = document.getElementById('supply-order-error');
    const cancelBtn = document.getElementById('supply-order-cancel');
    const confirmBtn = document.getElementById('supply-order-confirm');

    if (!openBtn || !overlay || !input || !confirmBtn) return;

    const showError = (message) => {
        if (!errorBox) return;
        errorBox.textContent = message;
        errorBox.style.display = 'block';
    };

    const clearError = () => {
        if (!errorBox) return;
        errorBox.textContent = '';
        errorBox.style.display = 'none';
    };

    const openModal = () => {
        clearError();
        input.value = '';
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
        setTimeout(() => input.focus(), 50);
    };

    const closeModal = () => {
        overlay.classList.remove('active');
        document.body.style.overflow = '';
    };

    const setLoading = (isLoading) => {
        confirmBtn.disabled = isLoading;
        confirmBtn.textContent = isLoading ? 'جاري البحث...' : 'تأكيد';
    };

    const submitCode = async () => {
        const code = input.value.trim();
        if (!code) {
            showError('يرجى إدخال رقم أمر التوريد.');
            return;
        }

        if (!window.appRoutes || !window.appRoutes.findSupplierByCode || !window.appRoutes.truckIndex) {
            showError('حدث خطأ في إعدادات الصفحة. يرجى تحديث الصفحة والمحاولة مرة أخرى.');
            return;
        }

        clearError();
        setLoading(true);

        try {
            const response = await fetch(`${window.appRoutes.findSupplierByCode}?code=${encodeURIComponent(code)}`);
            if (!response.ok) {
                showError('حدث خطأ أثناء البحث. يرجى المحاولة مرة أخرى.');
                setLoading(false);
                return;
            }

            const result = await response.json();
            if (!result.found) {
                showError(result.message || 'لا يوجد مورد بهذا الرقم.');
                setLoading(false);
                return;
            }

            window.location.href = `${window.appRoutes.truckIndex}?supplierId=${result.supplierId}`;
        } catch (err) {
            showError('تعذر الاتصال بالخادم. يرجى المحاولة مرة أخرى.');
            setLoading(false);
        }
    };

    openBtn.addEventListener('click', openModal);
    if (cancelBtn) cancelBtn.addEventListener('click', closeModal);

    overlay.addEventListener('click', (e) => {
        if (e.target === overlay) closeModal();
    });

    confirmBtn.addEventListener('click', submitCode);
    input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            submitCode();
        }
    });
});
