document.addEventListener('DOMContentLoaded', async () => {

    // DOM Elements
    const tableBody = document.getElementById('employeeTableBody');
    const searchInput = document.getElementById('employeeSearch');
    const paginationContainer = document.getElementById('paginationContainer');
    const filterDropdownBtn = document.getElementById('filterDropdown');
    const sortBtnText = document.getElementById('sortBtnText');
    const sortMenu = document.getElementById('sortMenu');
    const exportBtn = document.getElementById('exportBtn');
    const addEmployeeBtn = document.getElementById('addEmployeeBtn');

    // Global State
    const state = {
        currentPage: 1,
        itemsPerPage: 10,
        searchTerm: "",
        sortBy: "name",
        sortLabels: {
            "name": "الإسم", "phone": "رقم الهاتف", "role": "الدور",
            "code": "الكود", "email": "البريد الإلكتروني", "nationalId": "الرقم القومي"
        },
        allData: [] // Store fetched data
    };

    // 1. Fetch Data centrally from API
    async function loadData() {
        try {
            state.allData = await EmployeesAPI.getAll();
            renderTable();
        } catch (error) {
            console.error("Failed to load employees", error);
        }
    }

    // 2. Filter & Sort Logic
    function filterEmployees() {
        let filtered = state.allData;

        // Apply Search (Real-time)
        if (state.searchTerm) {
            const term = state.searchTerm.toLowerCase();
            filtered = filtered.filter(emp =>
                emp.name.toLowerCase().includes(term) ||
                emp.phone.includes(term) ||
                emp.email.toLowerCase().includes(term) ||
                emp.role.toLowerCase().includes(term) ||
                emp.nationalId.includes(term) ||
                emp.code.toLowerCase().includes(term)
            );
        }

        // Apply Sorting
        filtered.sort((a, b) => {
            const valA = String(a[state.sortBy]);
            const valB = String(b[state.sortBy]);
            return valA.localeCompare(valB, 'ar');
        });

        return filtered;
    }

    // 3. Render Table
    function renderTable() {
        if (!tableBody) return;

        const filteredData = filterEmployees();
        const totalPages = Math.ceil(filteredData.length / state.itemsPerPage);

        if (state.currentPage > totalPages && totalPages > 0) {
            state.currentPage = 1;
        } else if (totalPages === 0) {
            state.currentPage = 1;
        }

        const startIndex = (state.currentPage - 1) * state.itemsPerPage;
        const pageData = filteredData.slice(startIndex, startIndex + state.itemsPerPage);

        tableBody.innerHTML = '';

        if (pageData.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="7" class="empty-state">لا توجد نتائج مطابقة للبحث</td></tr>`;
        } else {
            pageData.forEach((emp) => {
                const tr = document.createElement('tr');
                tr.className = 'table-row';
                tr.innerHTML = `
                    <td>${emp.name}</td>
                    <td>${emp.phone}</td>
                    <td class="cell-email">${emp.email}</td>
                    <td>${emp.role}</td>
                    <td>${emp.nationalId}</td>
                    <td class="cell-code">${emp.code}</td>
                    <td>
                        <button class="btn-table-details" data-id="${emp.id}">
                            <span>تفاصيل</span>
                            <span>&larr;</span>
                        </button>
                    </td>
                `;
                tableBody.appendChild(tr);
            });
        }

        renderPagination(totalPages);
    }

    // 4. Render Dynamic Pagination UI
    function renderPagination(totalPages) {
        if (!paginationContainer) return;
        paginationContainer.innerHTML = '';

        if (totalPages <= 1) return;

        const prevBtn = document.createElement('button');
        prevBtn.className = 'page-item';
        prevBtn.id = 'prevPage';
        prevBtn.innerHTML = '&lt;';
        prevBtn.disabled = state.currentPage === 1;
        paginationContainer.appendChild(prevBtn);

        const pages = generatePageNumbers(state.currentPage, totalPages);

        pages.forEach(p => {
            if (p === '...') {
                const span = document.createElement('span');
                span.className = 'page-dots';
                span.textContent = '...';
                paginationContainer.appendChild(span);
            } else {
                const btn = document.createElement('button');
                btn.className = `page-item ${p === state.currentPage ? 'active' : ''}`;
                btn.textContent = p;
                btn.dataset.page = p;
                paginationContainer.appendChild(btn);
            }
        });

        const nextBtn = document.createElement('button');
        nextBtn.className = 'page-item';
        nextBtn.id = 'nextPage';
        nextBtn.innerHTML = '&gt;';
        nextBtn.disabled = state.currentPage === totalPages || totalPages === 0;
        paginationContainer.appendChild(nextBtn);
    }

    function generatePageNumbers(current, total) {
        if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
        if (current <= 4) return [1, 2, 3, 4, 5, '...', total];
        if (current >= total - 3) return [1, '...', total - 4, total - 3, total - 2, total - 1, total];
        return [1, '...', current - 1, current, current + 1, '...', total];
    }

    function goToPage(pageNumber) {
        state.currentPage = pageNumber;
        renderTable();
    }

    // ==================================================
    // EVENT LISTENERS
    // ==================================================

    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            state.searchTerm = e.target.value.trim();
            state.currentPage = 1;
            renderTable();
        });
    }

    if (paginationContainer) {
        paginationContainer.addEventListener('click', (e) => {
            const target = e.target;
            const totalPages = Math.ceil(filterEmployees().length / state.itemsPerPage);

            if (target.id === 'prevPage' && state.currentPage > 1) {
                goToPage(state.currentPage - 1);
            } else if (target.id === 'nextPage' && state.currentPage < totalPages) {
                goToPage(state.currentPage + 1);
            } else if (target.dataset.page) {
                goToPage(parseInt(target.dataset.page));
            }
        });
    }

    if (filterDropdownBtn && sortMenu) {
        filterDropdownBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            const isExpanded = sortMenu.classList.contains('show');
            sortMenu.classList.toggle('show');
            filterDropdownBtn.classList.toggle('active', !isExpanded);
        });

        document.addEventListener('click', () => {
            sortMenu.classList.remove('show');
            filterDropdownBtn.classList.remove('active');
        });

        sortMenu.addEventListener('click', (e) => {
            if (e.target.tagName === 'LI') {
                const selectedSort = e.target.dataset.sort;
                state.sortBy = selectedSort;
                sortBtnText.textContent = `تصنيف عبر : ${state.sortLabels[selectedSort]}`;
                state.currentPage = 1;
                renderTable();
            }
        });
    }

    // الانتقال لصفحة التفاصيل (يشتغل على MVC أو HTML عادي)
    if (tableBody) {
        tableBody.addEventListener('click', (e) => {
            const detailsBtn = e.target.closest('.btn-table-details');
            if (detailsBtn) {
                const empId = detailsBtn.dataset.id;
                if (empId) {
                    // لو شغالين على MVC
                    if (window.mvcRoutes && window.mvcRoutes.detailsEmployeeUrl) {
                        window.location.href = window.mvcRoutes.detailsEmployeeUrl + '?id=' + empId;
                    }
                    // لو بنجرب على HTML عادي
                    else {
                        window.location.href = `Details.cshtml?id=${empId}`;
                    }
                }
            }
        });
    }

    // زر إضافة موظف (يشتغل على MVC أو HTML عادي)
    if (addEmployeeBtn) {
        addEmployeeBtn.addEventListener('click', () => {
            // لو شغالين على MVC
            if (window.mvcRoutes && window.mvcRoutes.createEmployeeUrl) {
                window.location.href = window.mvcRoutes.createEmployeeUrl;
            }
            // لو بنجرب على HTML عادي
            else {
                window.location.href = 'Create.cshtml';
            }
        });
    }

    if (exportBtn) {
        exportBtn.addEventListener('click', () => {
            const filteredData = filterEmployees();
            if (filteredData.length === 0) {
                if (typeof showToast === 'function') showToast('لا توجد بيانات للتصدير', 'error');
                return;
            }

            let csvContent = "الاسم,رقم الهاتف,البريد الإلكتروني,الدور,الرقم القومي,الكود\n";
            filteredData.forEach(row => {
                csvContent += `${row.name},${row.phone},${row.email},${row.role},${row.nationalId},${row.code}\n`;
            });

            const blob = new Blob(["\uFEFF" + csvContent], { type: 'text/csv;charset=utf-8;' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");

            link.setAttribute("href", url);
            link.setAttribute("download", "employees_export.csv");
            link.style.visibility = 'hidden';

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            if (typeof showToast === 'function') showToast('تم تصدير البيانات بنجاح', 'success');
        });
    }

    // زر إضافة موظف باستخدام MVC Route
    if (addEmployeeBtn) {
        addEmployeeBtn.addEventListener('click', () => {
            if (window.mvcRoutes) {
                window.location.href = window.mvcRoutes.createEmployeeUrl;
            }
        });
    }

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

    // Initialize fetching and rendering
    await loadData();
});