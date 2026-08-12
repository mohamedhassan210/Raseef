document.addEventListener('DOMContentLoaded', async () => {
    // 1. Elements Definition
    const employeeContentContainer = document.getElementById('employeeContentContainer');
    const errorStateContainer = document.getElementById('errorStateContainer');
    const btnBackToDashboard = document.getElementById('btnBackToDashboard');
    const btnEditEmployee = document.getElementById('btnEditEmployee');
    const btnDeleteEmployee = document.getElementById('btnDeleteEmployee');
    const deleteModal = document.getElementById('deleteModal');
    const btnCancelDelete = document.getElementById('btnCancelDelete');
    const btnConfirmDelete = document.getElementById('btnConfirmDelete');

    // UI Fields
    const elName = document.getElementById('employeeName');
    const elRoleBadge = document.getElementById('employeeRoleBadge');
    const elCodeHeader = document.getElementById('employeeCodeHeader');
    const elAvatarLetter = document.getElementById('employeeAvatarLetter');
    const elCardName = document.getElementById('cardName');
    const elCardCode = document.getElementById('cardCode');
    const elCardRole = document.getElementById('cardRole');
    const elCardPhone = document.getElementById('cardPhone');
    const elCardEmail = document.getElementById('cardEmail');
    const elCardNationalId = document.getElementById('cardNationalId');

    // Sidebar Mobile Toggle
    const mobileToggle = document.getElementById('mobileToggle');
    const sidebar = document.querySelector('.dashboard-sidebar');
    if (mobileToggle && sidebar) {
        mobileToggle.addEventListener('click', () => {
            sidebar.classList.toggle('active');
        });
        document.addEventListener('click', (e) => {
            if (window.innerWidth <= 992 && !sidebar.contains(e.target) && !mobileToggle.contains(e.target)) {
                sidebar.classList.remove('active');
            }
        });
    }

    // 2. Read ID from URL
    const params = new URLSearchParams(window.location.search);
    const employeeId = Number(params.get('id'));

    if (!employeeId) {
        showErrorState();
        return;
    }

    // 3. Fetch Employee Data
    try {
        const employee = await EmployeesAPI.getById(employeeId);

        if (employee) {
            renderEmployeeDetails(employee);
        } else {
            showErrorState();
        }
    } catch (error) {
        console.error("Error fetching employee:", error);
        showErrorState();
    }

    // 4. Render Logic
    function renderEmployeeDetails(employee) {
        elName.textContent = employee.name;
        elRoleBadge.textContent = `موظف ${employee.role}`;
        elCodeHeader.textContent = employee.code;

        const firstLetter = employee.name ? employee.name.trim().charAt(0) : "م";
        elAvatarLetter.textContent = firstLetter;

        elCardName.textContent = employee.name;
        elCardCode.textContent = employee.code;
        elCardRole.textContent = employee.role;
        elCardPhone.textContent = employee.phone;
        elCardEmail.textContent = employee.email;
        elCardNationalId.textContent = employee.nationalId;
    }

    function showErrorState() {
        if (employeeContentContainer) employeeContentContainer.style.display = 'none';
        if (errorStateContainer) errorStateContainer.style.display = 'block';
    }

    function goToDashboard() {
        if (window.mvcRoutes && window.mvcRoutes.dashboardUrl) {
            window.location.href = window.mvcRoutes.dashboardUrl;
        } else {
            // Fallback يرجعك لصفحة الـ Index مباشرة
            window.location.href = '/Administration/Index';
        }
    }

    // 5. Actions Logic
    if (btnBackToDashboard) {
        btnBackToDashboard.addEventListener('click', goToDashboard);
    }

    if (btnEditEmployee) {
        btnEditEmployee.addEventListener('click', () => {
            if (window.mvcRoutes && window.mvcRoutes.editEmployeeUrl) {
                window.location.href = `${window.mvcRoutes.editEmployeeUrl}?id=${employeeId}`;
            } else {
                // Fallback للـ HTML العادي
                window.location.href = `Edit?id=${employeeId}`;
            }
        });
    }

    // Delete Modal Logic
    if (btnDeleteEmployee && deleteModal) {
        btnDeleteEmployee.addEventListener('click', () => {
            deleteModal.classList.add('active');
        });
    }

    if (btnCancelDelete && deleteModal) {
        btnCancelDelete.addEventListener('click', () => {
            deleteModal.classList.remove('active');
        });
    }

    if (btnConfirmDelete) {
        btnConfirmDelete.addEventListener('click', async () => {
            try {
                await EmployeesAPI.delete(employeeId);
                deleteModal.classList.remove('active');

                if (typeof showToast === 'function') {
                    showToast('تم حذف الموظف بنجاح', 'success');
                }

                setTimeout(() => goToDashboard(), 1000);
            } catch (error) {
                console.error("Error deleting employee:", error);
                if (typeof showToast === 'function') {
                    showToast('حدث خطأ أثناء محاولة الحذف', 'error');
                }
            }
        });
    }

});