document.addEventListener('DOMContentLoaded', () => {

    // DOM Elements
    const tableBody = document.getElementById('employeeTableBody');
    const searchInput = document.getElementById('employeeSearch');
    const paginationContainer = document.getElementById('paginationContainer');
    const filterDropdownBtn = document.getElementById('filterDropdown');
    const sortBtnText = document.getElementById('sortBtnText');
    const sortMenu = document.getElementById('sortMenu');
    const addEmployeeBtn = document.getElementById('addEmployeeBtn');
    const exportBtn = document.getElementById('exportBtn');
    const noDataModal = document.getElementById('noDataModal');
    const closeModalBtn = document.getElementById('closeModalBtn');

    // Global State with Real Database Employees Data passed from MVC
    const state = {
        currentPage: 1,
        itemsPerPage: 10,
        searchTerm: "",
        sortBy: "name",
        sortLabels: {
            "name": "الإسم", "phone": "رقم الهاتف", "role": "الدور",
            "code": "الكود", "email": "البريد الإلكتروني", "nationalId": "الرقم القومي"
        },
        allData: Array.isArray(window.initialEmployeesData) ? window.initialEmployeesData : []
    };

    // 1. Filter & Sort Logic
    function filterEmployees() {
        let filtered = [...state.allData];

        // Apply Search (Real-time)
        if (state.searchTerm) {
            const term = state.searchTerm.toLowerCase();
            filtered = filtered.filter(emp =>
                (emp.name && emp.name.toLowerCase().includes(term)) ||
                (emp.phone && emp.phone.includes(term)) ||
                (emp.email && emp.email.toLowerCase().includes(term)) ||
                (emp.role && emp.role.toLowerCase().includes(term)) ||
                (emp.nationalId && emp.nationalId.includes(term)) ||
                (emp.code && emp.code.toLowerCase().includes(term))
            );
        }

        // Apply Sorting
        filtered.sort((a, b) => {
            const valA = String(a[state.sortBy] || "");
            const valB = String(b[state.sortBy] || "");
            return valA.localeCompare(valB, 'ar');
        });

        return filtered;
    }

    // 2. Render Table
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
                const detailsUrl = window.mvcRoutes?.detailsEmployeeUrl 
                    ? `${window.mvcRoutes.detailsEmployeeUrl}/${emp.id}` 
                    : `/Administration/Details/${emp.id}`;

                tr.innerHTML = `
                    <td>${emp.name || '--'}</td>
                    <td>${emp.phone || '--'}</td>
                    <td class="cell-email">${emp.email || '--'}</td>
                    <td>${emp.role || '--'}</td>
                    <td>${emp.nationalId || '--'}</td>
                    <td class="cell-code">${emp.code || '--'}</td>
                    <td>
                        <a href="${detailsUrl}" class="btn-table-details" style="text-decoration: none;">
                            <span>تفاصيل</span>
                            <span>&larr;</span>
                        </a>
                    </td>
                `;
                tableBody.appendChild(tr);
            });
        }

        renderPagination(totalPages);
    }

    // 3. Render Dynamic Pagination UI
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

    // 4. Event Listeners
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

    if (exportBtn) {
        exportBtn.addEventListener('click', () => {
            const filteredData = filterEmployees();

            // لو مفيش بيانات، نظهر المودال
            if (filteredData.length === 0) {
                noDataModal.style.display = 'flex';
                return;
            }

            // تجهيز البيانات للإكسل
            const excelData = filteredData.map(emp => ({
                "الإسم": emp.name || '--',
                "رقم الهاتف": emp.phone || '--',
                "البريد الإلكتروني": emp.email || '--',
                "الدور": emp.role || '--',
                "الرقم القومي": emp.nationalId || '--',
                "الكود": emp.code || '--'
            }));

            // إنشاء وتنزيل ملف الإكسل
            const worksheet = XLSX.utils.json_to_sheet(excelData);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "الموظفين");
            XLSX.writeFile(workbook, "Employees_Export.xlsx");
        });
    }

    // قفل المودال
    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', () => {
            noDataModal.style.display = 'none';
        });
    }

    // Render Initial State
    renderTable();
});