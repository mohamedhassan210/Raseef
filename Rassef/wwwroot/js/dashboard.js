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
    //const noDataModal = document.getElementById('noDataModal');
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

document.addEventListener('DOMContentLoaded', () => {

    const tableBody = document.getElementById('truckTableBody');
    const searchInput = document.getElementById('truckSearch');
    const paginationContainer = document.getElementById('paginationContainer');
    const filterDropdownBtn = document.getElementById('filterDropdown');
    const sortBtnText = document.getElementById('sortBtnText');
    const sortMenu = document.getElementById('sortMenu');
    const exportBtn = document.getElementById('exportBtn');
    //const noDataModal = document.getElementById('noDataModal');
    const closeModalBtn = document.getElementById('closeModalBtn');

    const state = {
        currentPage: 1,
        itemsPerPage: 10,
        searchTerm: "",
        sortBy: "number",
        sortLabels: {
            "number": "الرقم", "type": "النوع", "company": "الشركة",
            "capacity": "سعة التخزين", "host": "الموظف المضيف"
        },
        allData: Array.isArray(window.initialTrucksData) ? window.initialTrucksData : []
    };

    function filterTrucks() {
        let filtered = [...state.allData];

        if (state.searchTerm) {
            const term = state.searchTerm.toLowerCase();
            filtered = filtered.filter(t =>
                (t.number && t.number.toLowerCase().includes(term)) ||
                (t.type && t.type.toLowerCase().includes(term)) ||
                (t.company && t.company.toLowerCase().includes(term)) ||
                (t.host && t.host.toLowerCase().includes(term))
            );
        }

        filtered.sort((a, b) => {
            const valA = String(a[state.sortBy] || "");
            const valB = String(b[state.sortBy] || "");
            return valA.localeCompare(valB, 'ar', { numeric: true }); // numeric: true عشان يرتب الأرقام صح
        });

        return filtered;
    }

    function renderTable() {
        if (!tableBody) return;
        const filteredData = filterTrucks();
        const totalPages = Math.ceil(filteredData.length / state.itemsPerPage);

        if (state.currentPage > totalPages && totalPages > 0) state.currentPage = 1;

        const startIndex = (state.currentPage - 1) * state.itemsPerPage;
        const pageData = filteredData.slice(startIndex, startIndex + state.itemsPerPage);

        tableBody.innerHTML = '';

        if (pageData.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="6" class="empty-state">لا توجد نتائج مطابقة للبحث</td></tr>`;
        } else {
            pageData.forEach((truck) => {
                const tr = document.createElement('tr');
                tr.className = 'table-row';
                const detailsUrl = window.mvcRoutes?.detailsUrl
                    ? `${window.mvcRoutes.detailsUrl}/${truck.id}`
                    : `/SupplierRequest/Details/${truck.id}`;

                tr.innerHTML = `
                    <td class="col-number">${truck.number || '--'}</td>
                    <td class="col-type">${truck.type || '--'}</td>
                    <td class="col-company">${truck.company || '--'}</td>
                    <td class="col-capacity">${truck.capacity ? truck.capacity + ' kg' : '--'}</td>
                    <td class="col-host">${truck.host || '--'}</td>
                    <td class="col-action">
                        <a href="${detailsUrl}" class="btn-table-details" style="text-decoration: none;">
                            <span>تفاصيل</span>
                            <span class="arrow">&larr;</span>
                        </a>
                    </td>
                `;
                tableBody.appendChild(tr);
            });
        }
        renderPagination(totalPages);
    }

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
            const totalPages = Math.ceil(filterTrucks().length / state.itemsPerPage);

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
            const filteredData = filterTrucks();

            if (filteredData.length === 0) {
                noDataModal.style.display = 'flex';
                return;
            }

            const excelData = filteredData.map(t => ({
                "الرقم": t.number || '--',
                "النوع": t.type || '--',
                "الشركة": t.company || '--',
                "سعة التخزين": t.capacity ? t.capacity + ' kg' : '--',
                "الموظف المضيف": t.host || '--'
            }));

            const worksheet = XLSX.utils.json_to_sheet(excelData);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "الشاحنات");
            XLSX.writeFile(workbook, "Trucks_Export.xlsx");
        });
    }

    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', () => {
            noDataModal.style.display = 'none';
        });
    }

    renderTable();
});

document.addEventListener('DOMContentLoaded', () => {

    const tableBody = document.getElementById('truckTableBody');
    const searchInput = document.getElementById('truckSearch');
    const paginationContainer = document.getElementById('paginationContainer');
    const filterDropdownBtn = document.getElementById('filterDropdown');
    const sortBtnText = document.getElementById('sortBtnText');
    const sortMenu = document.getElementById('sortMenu');
    const exportBtn = document.getElementById('exportBtn');
    //const noDataModal = document.getElementById('noDataModal');
    const closeModalBtn = document.getElementById('closeModalBtn');

    const state = {
        currentPage: 1,
        itemsPerPage: 10,
        searchTerm: "",
        sortBy: "number",
        sortDesc: false, // حالة الترتيب (تصاعدي ولا تنازلي)
        sortLabels: {
            "number": "الرقم", "type": "النوع", "company": "الشركة",
            "capacity": "سعة التخزين", "host": "الموظف المضيف"
        },
        allData: Array.isArray(window.initialTrucksData) ? window.initialTrucksData : []
    };

    function filterTrucks() {
        let filtered = [...state.allData];

        // 1. الفلترة بالبحث
        if (state.searchTerm) {
            const term = state.searchTerm.toLowerCase();
            filtered = filtered.filter(t =>
                (t.number && String(t.number).toLowerCase().includes(term)) ||
                (t.type && String(t.type).toLowerCase().includes(term)) ||
                (t.company && String(t.company).toLowerCase().includes(term)) ||
                (t.host && String(t.host).toLowerCase().includes(term)) ||
                (t.capacity && String(t.capacity).toLowerCase().includes(term))
            );
        }

        // 2. الترتيب من خلال الدروب داون
        filtered.sort((a, b) => {
            let valA = a[state.sortBy];
            let valB = b[state.sortBy];

            // معالجة القيم لو فاضية
            valA = (valA !== null && valA !== undefined) ? valA : "";
            valB = (valB !== null && valB !== undefined) ? valB : "";

            // الترتيب الرقمي المخصص لسعة التخزين
            if (state.sortBy === 'capacity') {
                const numA = parseFloat(valA) || 0;
                const numB = parseFloat(valB) || 0;
                return state.sortDesc ? numB - numA : numA - numB;
            }

            // الترتيب النصي لباقي العواميد
            valA = String(valA);
            valB = String(valB);

            const comparison = valA.localeCompare(valB, 'ar', { numeric: true });
            return state.sortDesc ? -comparison : comparison;
        });

        return filtered;
    }

    function renderTable() {
        if (!tableBody) return;
        const filteredData = filterTrucks();
        const totalPages = Math.ceil(filteredData.length / state.itemsPerPage);

        if (state.currentPage > totalPages && totalPages > 0) state.currentPage = 1;

        const startIndex = (state.currentPage - 1) * state.itemsPerPage;
        const pageData = filteredData.slice(startIndex, startIndex + state.itemsPerPage);

        tableBody.innerHTML = '';

        if (pageData.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="6" class="empty-state">لا توجد نتائج مطابقة للبحث</td></tr>`;
        } else {
            pageData.forEach((truck) => {
                const tr = document.createElement('tr');
                tr.className = 'table-row';
                const detailsUrl = window.mvcRoutes?.detailsUrl
                    ? `${window.mvcRoutes.detailsUrl}/${truck.id}`
                    : `/SupplierRequest/Details/${truck.id}`;

                // معالجة نوع الشاحنة (لو راجعة Boolean)
                let truckTypeDisplay = truck.type;
                if (typeof truck.type === "boolean") {
                    truckTypeDisplay = truck.type ? "تبريد" : "لا تبريد";
                }

                tr.innerHTML = `
                    <td class="col-number">${truck.number || '--'}</td>
                    <td class="col-type">${truckTypeDisplay || '--'}</td>
                    <td class="col-company">${truck.company || '--'}</td>
                    <td class="col-capacity">${truck.capacity ? truck.capacity + ' kg' : '--'}</td>
                    <td class="col-host">${truck.host || '--'}</td>
                    <td class="col-action">
                        <a href="${detailsUrl}" class="btn-table-details" style="text-decoration: none;">
                            <span>تفاصيل</span>
                            <span class="arrow">&larr;</span>
                        </a>
                    </td>
                `;
                tableBody.appendChild(tr);
            });
        }
        renderPagination(totalPages);
    }

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
            const totalPages = Math.ceil(filterTrucks().length / state.itemsPerPage);

            if (target.id === 'prevPage' && state.currentPage > 1) {
                goToPage(state.currentPage - 1);
            } else if (target.id === 'nextPage' && state.currentPage < totalPages) {
                goToPage(state.currentPage + 1);
            } else if (target.dataset.page) {
                goToPage(parseInt(target.dataset.page));
            }
        });
    }

    // =====================================
    // 💡 تعديلات الدروب داون (تصنيف عبر) 
    // =====================================
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

                // لو ضغط على نفس الخيار تاني، نعكس الترتيب (تصاعدي/تنازلي)
                if (state.sortBy === selectedSort) {
                    state.sortDesc = !state.sortDesc;
                } else {
                    state.sortBy = selectedSort;
                    state.sortDesc = false; // نرجعه تصاعدي لو اختار عمود جديد
                }

                const arrow = state.sortDesc ? "↓" : "↑";
                sortBtnText.textContent = `تصنيف عبر : ${state.sortLabels[selectedSort]} ${arrow}`;

                state.currentPage = 1;
                renderTable();
            }
        });
    }

    if (exportBtn) {
        exportBtn.addEventListener('click', () => {
            const filteredData = filterTrucks();

            if (filteredData.length === 0) {
                noDataModal.style.display = 'flex';
                return;
            }

            const excelData = filteredData.map(t => {
                let typeText = t.type;
                if (typeof t.type === "boolean") typeText = t.type ? "تبريد" : "لا تبريد";

                return {
                    "الرقم": t.number || '--',
                    "النوع": typeText || '--',
                    "الشركة": t.company || '--',
                    "سعة التخزين": t.capacity ? t.capacity + ' kg' : '--',
                    "الموظف المضيف": t.host || '--'
                };
            });

            const worksheet = XLSX.utils.json_to_sheet(excelData);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "الشاحنات");
            XLSX.writeFile(workbook, "Trucks_Export.xlsx");
        });
    }

    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', () => {
            noDataModal.style.display = 'none';
        });
    }

    renderTable();
});
document.addEventListener('DOMContentLoaded', () => {

    // ====================================================================
    // 1. كود صفحة "الشاحنات" (بيشتغل بس لو جدول الشاحنات موجود في الصفحة)
    // ====================================================================
    if (document.getElementById('truckTableBody')) {
        const tableBody = document.getElementById('truckTableBody');
        const searchInput = document.getElementById('truckSearch');
        const paginationContainer = document.getElementById('paginationContainer');
        const filterDropdownBtn = document.getElementById('filterDropdown');
        const sortBtnText = document.getElementById('sortBtnText');
        const sortMenu = document.getElementById('sortMenu');
        const exportBtn = document.getElementById('exportBtn');
        //const noDataModal = document.getElementById('noDataModal');
        const closeModalBtn = document.getElementById('closeModalBtn');

        const state = {
            currentPage: 1,
            itemsPerPage: 10,
            searchTerm: "",
            sortBy: "number",
            sortDesc: false,
            sortLabels: {
                "number": "الرقم", "type": "النوع", "company": "الشركة",
                "capacity": "سعة التخزين", "host": "الموظف المضيف"
            },
            allData: Array.isArray(window.initialTrucksData) ? window.initialTrucksData : []
        };

        function filterTrucks() {
            let filtered = [...state.allData];

            if (state.searchTerm) {
                const term = state.searchTerm.toLowerCase();
                filtered = filtered.filter(t =>
                    (t.number && String(t.number).toLowerCase().includes(term)) ||
                    (t.type && String(t.type).toLowerCase().includes(term)) ||
                    (t.company && String(t.company).toLowerCase().includes(term)) ||
                    (t.host && String(t.host).toLowerCase().includes(term)) ||
                    (t.capacity && String(t.capacity).toLowerCase().includes(term))
                );
            }

            filtered.sort((a, b) => {
                let valA = a[state.sortBy];
                let valB = b[state.sortBy];

                valA = (valA !== null && valA !== undefined) ? valA : "";
                valB = (valB !== null && valB !== undefined) ? valB : "";

                if (state.sortBy === 'capacity') {
                    const numA = parseFloat(valA) || 0;
                    const numB = parseFloat(valB) || 0;
                    return state.sortDesc ? numB - numA : numA - numB;
                }

                valA = String(valA);
                valB = String(valB);

                const comparison = valA.localeCompare(valB, 'ar', { numeric: true });
                return state.sortDesc ? -comparison : comparison;
            });

            return filtered;
        }

        function renderTable() {
            if (!tableBody) return;
            const filteredData = filterTrucks();
            const totalPages = Math.ceil(filteredData.length / state.itemsPerPage);

            if (state.currentPage > totalPages && totalPages > 0) state.currentPage = 1;

            const startIndex = (state.currentPage - 1) * state.itemsPerPage;
            const pageData = filteredData.slice(startIndex, startIndex + state.itemsPerPage);

            tableBody.innerHTML = '';

            if (pageData.length === 0) {
                tableBody.innerHTML = `<tr><td colspan="6" class="empty-state">لا توجد نتائج مطابقة للبحث</td></tr>`;
            } else {
                pageData.forEach((truck) => {
                    const tr = document.createElement('tr');
                    tr.className = 'table-row';
                    const detailsUrl = window.mvcRoutes?.detailsUrl
                        ? `${window.mvcRoutes.detailsUrl}/${truck.id}`
                        : `/SupplierRequest/Details/${truck.id}`;

                    let truckTypeDisplay = truck.type;
                    if (typeof truck.type === "boolean") {
                        truckTypeDisplay = truck.type ? "تبريد" : "لا تبريد";
                    }

                    tr.innerHTML = `
                        <td class="col-number">${truck.number || '--'}</td>
                        <td class="col-type">${truckTypeDisplay || '--'}</td>
                        <td class="col-company">${truck.company || '--'}</td>
                        <td class="col-capacity">${truck.capacity ? truck.capacity + ' kg' : '--'}</td>
                        <td class="col-host">${truck.host || '--'}</td>
                        <td class="col-action">
                            <a href="${detailsUrl}" class="btn-table-details" style="text-decoration: none;">
                                <span>تفاصيل</span>
                                <span class="arrow">&larr;</span>
                            </a>
                        </td>
                    `;
                    tableBody.appendChild(tr);
                });
            }
            renderPagination(totalPages);
        }

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
                const totalPages = Math.ceil(filterTrucks().length / state.itemsPerPage);

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
                    if (state.sortBy === selectedSort) {
                        state.sortDesc = !state.sortDesc;
                    } else {
                        state.sortBy = selectedSort;
                        state.sortDesc = false;
                    }
                    const arrow = state.sortDesc ? "↓" : "↑";
                    sortBtnText.textContent = `تصنيف عبر : ${state.sortLabels[selectedSort]} ${arrow}`;
                    state.currentPage = 1;
                    renderTable();
                }
            });
        }

        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                const filteredData = filterTrucks();

                if (filteredData.length === 0) {
                    noDataModal.style.display = 'flex';
                    return;
                }

                const excelData = filteredData.map(t => {
                    let typeText = t.type;
                    if (typeof t.type === "boolean") typeText = t.type ? "تبريد" : "لا تبريد";

                    return {
                        "الرقم": t.number || '--',
                        "النوع": typeText || '--',
                        "الشركة": t.company || '--',
                        "سعة التخزين": t.capacity ? t.capacity + ' kg' : '--',
                        "الموظف المضيف": t.host || '--'
                    };
                });

                const worksheet = XLSX.utils.json_to_sheet(excelData);
                const workbook = XLSX.utils.book_new();
                XLSX.utils.book_append_sheet(workbook, worksheet, "الشاحنات");
                XLSX.writeFile(workbook, "Trucks_Export.xlsx");
            });
        }

        if (closeModalBtn) {
            closeModalBtn.addEventListener('click', () => {
                noDataModal.style.display = 'none';
            });
        }

        renderTable();
    }


    // ====================================================================
    // 2. كود صفحة "الموظفين" (بيشتغل بس لو جدول الموظفين موجود في الصفحة)
    // ====================================================================
    if (document.getElementById('employeeTableBody')) {
        const tableBody = document.getElementById('employeeTableBody');
        const searchInput = document.getElementById('employeeSearch');
        const paginationContainer = document.getElementById('paginationContainer');
        const filterDropdownBtn = document.getElementById('filterDropdown');
        const sortBtnText = document.getElementById('sortBtnText');
        const sortMenu = document.getElementById('sortMenu');
        const exportBtn = document.getElementById('exportBtn');
        //const noDataModal = document.getElementById('noDataModal');
        const closeModalBtn = document.getElementById('closeModalBtn');

        const state = {
            currentPage: 1,
            itemsPerPage: 10,
            searchTerm: "",
            sortBy: "name",
            sortDesc: false,
            sortLabels: {
                "name": "الإسم", "phone": "رقم الهاتف", "role": "الدور",
                "code": "الكود", "email": "البريد الإلكتروني", "nationalId": "الرقم القومي"
            },
            allData: Array.isArray(window.initialEmployeesData) ? window.initialEmployeesData : []
        };

        function filterEmployees() {
            let filtered = [...state.allData];

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

            filtered.sort((a, b) => {
                const valA = String(a[state.sortBy] || "");
                const valB = String(b[state.sortBy] || "");
                const comparison = valA.localeCompare(valB, 'ar', { numeric: true });
                return state.sortDesc ? -comparison : comparison;
            });

            return filtered;
        }

        function renderTable() {
            if (!tableBody) return;

            const filteredData = filterEmployees();
            const totalPages = Math.ceil(filteredData.length / state.itemsPerPage);

            if (state.currentPage > totalPages && totalPages > 0) state.currentPage = 1;

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
                        <td class="col-name">${emp.name || '--'}</td>
                        <td class="col-phone">${emp.phone || '--'}</td>
                        <td class="col-email">${emp.email || '--'}</td>
                        <td class="col-role">${emp.role || '--'}</td>
                        <td class="col-nid">${emp.nationalId || '--'}</td>
                        <td class="col-code">${emp.code || '--'}</td>
                        <td class="col-action">
                            <a href="${detailsUrl}" class="btn-table-details" style="text-decoration: none;">
                                <span>تفاصيل</span>
                                <span class="arrow">&larr;</span>
                            </a>
                        </td>
                    `;
                    tableBody.appendChild(tr);
                });
            }
            renderPagination(totalPages);
        }

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
                    if (state.sortBy === selectedSort) {
                        state.sortDesc = !state.sortDesc;
                    } else {
                        state.sortBy = selectedSort;
                        state.sortDesc = false;
                    }
                    const arrow = state.sortDesc ? "↓" : "↑";
                    sortBtnText.textContent = `تصنيف عبر : ${state.sortLabels[selectedSort]} ${arrow}`;
                    state.currentPage = 1;
                    renderTable();
                }
            });
        }

        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                const filteredData = filterEmployees();

                if (filteredData.length === 0) {
                    noDataModal.style.display = 'flex';
                    return;
                }

                const excelData = filteredData.map(emp => ({
                    "الإسم": emp.name || '--',
                    "رقم الهاتف": emp.phone || '--',
                    "البريد الإلكتروني": emp.email || '--',
                    "الدور": emp.role || '--',
                    "الرقم القومي": emp.nationalId || '--',
                    "الكود": emp.code || '--'
                }));

                const worksheet = XLSX.utils.json_to_sheet(excelData);
                const workbook = XLSX.utils.book_new();
                XLSX.utils.book_append_sheet(workbook, worksheet, "الموظفين");
                XLSX.writeFile(workbook, "Employees_Export.xlsx");
            });
        }

        if (closeModalBtn) {
            closeModalBtn.addEventListener('click', () => {
                noDataModal.style.display = 'none';
            });
        }

        renderTable();
    }
});

// ====================================================================
// 3. كود صفحة "السائقين" (بيشتغل بس لو جدول السائقين موجود في الصفحة)
// ====================================================================
if (document.getElementById('driverTableBody')) {
    const tableBody = document.getElementById('driverTableBody');
    const searchInput = document.getElementById('driverSearch');
    const paginationContainer = document.getElementById('paginationContainer');
    const filterDropdownBtn = document.getElementById('filterDropdown');
    const sortBtnText = document.getElementById('sortBtnText');
    const sortMenu = document.getElementById('sortMenu');
    const exportBtn = document.getElementById('exportBtn');
    const noDataModal = document.getElementById('noDataModal');
    const closeModalBtn = document.getElementById('closeModalBtn');

    const state = {
        currentPage: 1,
        itemsPerPage: 10,
        searchTerm: "",
        sortBy: "name",
        sortDesc: false,
        sortLabels: {
            "name": "الإسم", "phone": "رقم الهاتف", "nid": "الرقم القومي",
            "type": "النوع", "host": "الموظف المضيف"
        },
        allData: Array.isArray(window.initialDriversData) ? window.initialDriversData : []
    };

    function filterDrivers() {
        let filtered = [...state.allData];

        if (state.searchTerm) {
            const term = state.searchTerm.toLowerCase();
            filtered = filtered.filter(d =>
                (d.name && String(d.name).toLowerCase().includes(term)) ||
                (d.phone && String(d.phone).toLowerCase().includes(term)) ||
                (d.nid && String(d.nid).toLowerCase().includes(term)) ||
                (d.type && String(d.type).toLowerCase().includes(term)) ||
                (d.host && String(d.host).toLowerCase().includes(term))
            );
        }

        filtered.sort((a, b) => {
            let valA = a[state.sortBy];
            let valB = b[state.sortBy];

            valA = (valA !== null && valA !== undefined) ? valA : "";
            valB = (valB !== null && valB !== undefined) ? valB : "";

            valA = String(valA);
            valB = String(valB);

            const comparison = valA.localeCompare(valB, 'ar', { numeric: true });
            return state.sortDesc ? -comparison : comparison;
        });

        return filtered;
    }

    function renderTable() {
        if (!tableBody) return;
        const filteredData = filterDrivers();
        const totalPages = Math.ceil(filteredData.length / state.itemsPerPage);

        if (state.currentPage > totalPages && totalPages > 0) state.currentPage = 1;

        const startIndex = (state.currentPage - 1) * state.itemsPerPage;
        const pageData = filteredData.slice(startIndex, startIndex + state.itemsPerPage);

        tableBody.innerHTML = '';

        if (pageData.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="6" class="empty-state">لا توجد نتائج مطابقة للبحث</td></tr>`;
        } else {
            pageData.forEach((driver) => {
                const tr = document.createElement('tr');
                tr.className = 'table-row';
                const detailsUrl = window.mvcRoutes?.detailsUrl
                    ? `${window.mvcRoutes.detailsUrl}/${driver.id}`
                    : `/Driver/Details/${driver.id}`;

                tr.innerHTML = `
                        <td class="col-name">${driver.name || '--'}</td>
                        <td class="col-phone">${driver.phone || '--'}</td>
                        <td class="col-nid">${driver.nid || '--'}</td>
                        <td class="col-type">${driver.type || '--'}</td>
                        <td class="col-host">${driver.host || '--'}</td>
                        <td class="col-action">
                            <a href="${detailsUrl}" class="btn-table-details" style="text-decoration: none;">
                                <span>تفاصيل</span>
                                <span class="arrow">&larr;</span>
                            </a>
                        </td>
                    `;
                tableBody.appendChild(tr);
            });
        }
        renderPagination(totalPages);
    }

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
            const totalPages = Math.ceil(filterDrivers().length / state.itemsPerPage);

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
                if (state.sortBy === selectedSort) {
                    state.sortDesc = !state.sortDesc;
                } else {
                    state.sortBy = selectedSort;
                    state.sortDesc = false;
                }
                const arrow = state.sortDesc ? "↓" : "↑";
                sortBtnText.textContent = `تصنيف عبر : ${state.sortLabels[selectedSort]} ${arrow}`;
                state.currentPage = 1;
                renderTable();
            }
        });
    }

    if (exportBtn) {
        exportBtn.addEventListener('click', () => {
            const filteredData = filterDrivers();

            if (filteredData.length === 0) {
                noDataModal.style.display = 'flex';
                return;
            }

            const excelData = filteredData.map(d => {
                return {
                    "الإسم": d.name || '--',
                    "رقم الهاتف": d.phone || '--',
                    "الرقم القومي": d.nid || '--',
                    "النوع": d.type || '--',
                    "الموظف المضيف": d.host || '--'
                };
            });

            const worksheet = XLSX.utils.json_to_sheet(excelData);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "السائقين");
            XLSX.writeFile(workbook, "Drivers_Export.xlsx");
        });
    }

    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', () => {
            noDataModal.style.display = 'none';
        });
    }

    renderTable();
}

document.addEventListener('DOMContentLoaded', () => {

    // ====================================================================
    // 1. كود صفحة "الموظفين"
    // ====================================================================
    if (document.getElementById('employeeTableBody')) {
        const tableBody = document.getElementById('employeeTableBody');
        const searchInput = document.getElementById('employeeSearch');
        const paginationContainer = document.getElementById('paginationContainer');
        const filterDropdownBtn = document.getElementById('filterDropdown');
        const sortBtnText = document.getElementById('sortBtnText');
        const sortMenu = document.getElementById('sortMenu');
        const exportBtn = document.getElementById('exportBtn');
        const noDataModal = document.getElementById('noDataModal');
        const closeModalBtn = document.getElementById('closeModalBtn');

        const state = {
            currentPage: 1,
            itemsPerPage: 10,
            searchTerm: "",
            sortBy: "name",
            sortDesc: false,
            sortLabels: {
                "name": "الإسم", "phone": "رقم الهاتف", "role": "الدور",
                "code": "الكود", "email": "البريد الإلكتروني", "nationalId": "الرقم القومي"
            },
            allData: Array.isArray(window.initialEmployeesData) ? window.initialEmployeesData : []
        };

        function filterEmployees() {
            let filtered = [...state.allData];

            if (state.searchTerm) {
                const term = state.searchTerm.toLowerCase();
                filtered = filtered.filter(emp =>
                    (emp.name && String(emp.name).toLowerCase().includes(term)) ||
                    (emp.phone && String(emp.phone).includes(term)) ||
                    (emp.email && String(emp.email).toLowerCase().includes(term)) ||
                    (emp.role && String(emp.role).toLowerCase().includes(term)) ||
                    (emp.nationalId && String(emp.nationalId).includes(term)) ||
                    (emp.code && String(emp.code).toLowerCase().includes(term))
                );
            }

            filtered.sort((a, b) => {
                const valA = String(a[state.sortBy] || "");
                const valB = String(b[state.sortBy] || "");
                const comparison = valA.localeCompare(valB, 'ar', { numeric: true });
                return state.sortDesc ? -comparison : comparison;
            });

            return filtered;
        }

        function renderTable() {
            if (!tableBody) return;
            const filteredData = filterEmployees();
            const totalPages = Math.ceil(filteredData.length / state.itemsPerPage);

            if (state.currentPage > totalPages && totalPages > 0) state.currentPage = 1;

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
                        <td class="col-name">${emp.name || '--'}</td>
                        <td class="col-phone">${emp.phone || '--'}</td>
                        <td class="col-email">${emp.email || '--'}</td>
                        <td class="col-role">${emp.role || '--'}</td>
                        <td class="col-nid">${emp.nationalId || '--'}</td>
                        <td class="col-code">${emp.code || '--'}</td>
                        <td class="col-action">
                            <a href="${detailsUrl}" class="btn-table-details" style="text-decoration: none;">
                                <span>تفاصيل</span>
                                <span class="arrow">&larr;</span>
                            </a>
                        </td>
                    `;
                    tableBody.appendChild(tr);
                });
            }
            renderPagination(totalPages);
        }

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
                    state.currentPage -= 1;
                    renderTable();
                } else if (target.id === 'nextPage' && state.currentPage < totalPages) {
                    state.currentPage += 1;
                    renderTable();
                } else if (target.dataset.page) {
                    state.currentPage = parseInt(target.dataset.page);
                    renderTable();
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
                    if (state.sortBy === selectedSort) {
                        state.sortDesc = !state.sortDesc;
                    } else {
                        state.sortBy = selectedSort;
                        state.sortDesc = false;
                    }
                    const arrow = state.sortDesc ? "↓" : "↑";
                    sortBtnText.textContent = `تصنيف عبر : ${state.sortLabels[selectedSort]} ${arrow}`;
                    state.currentPage = 1;
                    renderTable();
                }
            });
        }

        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                const filteredData = filterEmployees();
                if (filteredData.length === 0) {
                    noDataModal.style.display = 'flex';
                    return;
                }
                const excelData = filteredData.map(emp => ({
                    "الإسم": emp.name || '--',
                    "رقم الهاتف": emp.phone || '--',
                    "البريد الإلكتروني": emp.email || '--',
                    "الدور": emp.role || '--',
                    "الرقم القومي": emp.nationalId || '--',
                    "الكود": emp.code || '--'
                }));
                const worksheet = XLSX.utils.json_to_sheet(excelData);
                const workbook = XLSX.utils.book_new();
                XLSX.utils.book_append_sheet(workbook, worksheet, "الموظفين");
                XLSX.writeFile(workbook, "Employees_Export.xlsx");
            });
        }

        if (closeModalBtn) closeModalBtn.addEventListener('click', () => noDataModal.style.display = 'none');
        renderTable();
    }


    // ====================================================================
    // 2. كود صفحة "الشاحنات"
    // ====================================================================
    else if (document.getElementById('truckTableBody')) {
        const tableBody = document.getElementById('truckTableBody');
        const searchInput = document.getElementById('truckSearch');
        const paginationContainer = document.getElementById('paginationContainer');
        const filterDropdownBtn = document.getElementById('filterDropdown');
        const sortBtnText = document.getElementById('sortBtnText');
        const sortMenu = document.getElementById('sortMenu');
        const exportBtn = document.getElementById('exportBtn');
        const noDataModal = document.getElementById('noDataModal');
        const closeModalBtn = document.getElementById('closeModalBtn');

        const state = {
            currentPage: 1,
            itemsPerPage: 10,
            searchTerm: "",
            sortBy: "number",
            sortDesc: false,
            sortLabels: {
                "number": "الرقم", "type": "النوع", "company": "الشركة",
                "capacity": "سعة التخزين", "host": "الموظف المضيف"
            },
            allData: Array.isArray(window.initialTrucksData) ? window.initialTrucksData : []
        };

        function filterTrucks() {
            let filtered = [...state.allData];
            if (state.searchTerm) {
                const term = state.searchTerm.toLowerCase();
                filtered = filtered.filter(t =>
                    (t.number && String(t.number).toLowerCase().includes(term)) ||
                    (t.type && String(t.type).toLowerCase().includes(term)) ||
                    (t.company && String(t.company).toLowerCase().includes(term)) ||
                    (t.host && String(t.host).toLowerCase().includes(term)) ||
                    (t.capacity && String(t.capacity).toLowerCase().includes(term))
                );
            }

            filtered.sort((a, b) => {
                let valA = a[state.sortBy] ?? "";
                let valB = b[state.sortBy] ?? "";

                if (state.sortBy === 'capacity') {
                    const numA = parseFloat(valA) || 0;
                    const numB = parseFloat(valB) || 0;
                    return state.sortDesc ? numB - numA : numA - numB;
                }

                valA = String(valA);
                valB = String(valB);
                const comparison = valA.localeCompare(valB, 'ar', { numeric: true });
                return state.sortDesc ? -comparison : comparison;
            });
            return filtered;
        }

        function renderTable() {
            if (!tableBody) return;
            const filteredData = filterTrucks();
            const totalPages = Math.ceil(filteredData.length / state.itemsPerPage);

            if (state.currentPage > totalPages && totalPages > 0) state.currentPage = 1;
            const startIndex = (state.currentPage - 1) * state.itemsPerPage;
            const pageData = filteredData.slice(startIndex, startIndex + state.itemsPerPage);

            tableBody.innerHTML = '';
            if (pageData.length === 0) {
                tableBody.innerHTML = `<tr><td colspan="6" class="empty-state">لا توجد نتائج مطابقة للبحث</td></tr>`;
            } else {
                pageData.forEach((truck) => {
                    const tr = document.createElement('tr');
                    tr.className = 'table-row';
                    const detailsUrl = window.mvcRoutes?.detailsUrl
                        ? `${window.mvcRoutes.detailsUrl}/${truck.id}`
                        : `/SupplierRequest/Details/${truck.id}`;

                    let truckTypeDisplay = truck.type;
                    if (typeof truck.type === "boolean") {
                        truckTypeDisplay = truck.type ? "تبريد" : "لا تبريد";
                    }

                    tr.innerHTML = `
                        <td class="col-number">${truck.number || '--'}</td>
                        <td class="col-type">${truckTypeDisplay || '--'}</td>
                        <td class="col-company">${truck.company || '--'}</td>
                        <td class="col-capacity">${truck.capacity ? truck.capacity + ' kg' : '--'}</td>
                        <td class="col-host">${truck.host || '--'}</td>
                        <td class="col-action">
                            <a href="${detailsUrl}" class="btn-table-details" style="text-decoration: none;">
                                <span>تفاصيل</span>
                                <span class="arrow">&larr;</span>
                            </a>
                        </td>
                    `;
                    tableBody.appendChild(tr);
                });
            }
            renderPagination(totalPages);
        }

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
                const totalPages = Math.ceil(filterTrucks().length / state.itemsPerPage);

                if (target.id === 'prevPage' && state.currentPage > 1) {
                    state.currentPage -= 1;
                    renderTable();
                } else if (target.id === 'nextPage' && state.currentPage < totalPages) {
                    state.currentPage += 1;
                    renderTable();
                } else if (target.dataset.page) {
                    state.currentPage = parseInt(target.dataset.page);
                    renderTable();
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
                    if (state.sortBy === selectedSort) {
                        state.sortDesc = !state.sortDesc;
                    } else {
                        state.sortBy = selectedSort;
                        state.sortDesc = false;
                    }
                    const arrow = state.sortDesc ? "↓" : "↑";
                    sortBtnText.textContent = `تصنيف عبر : ${state.sortLabels[selectedSort]} ${arrow}`;
                    state.currentPage = 1;
                    renderTable();
                }
            });
        }

        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                const filteredData = filterTrucks();
                if (filteredData.length === 0) {
                    noDataModal.style.display = 'flex';
                    return;
                }
                const excelData = filteredData.map(t => {
                    let typeText = t.type;
                    if (typeof t.type === "boolean") typeText = t.type ? "تبريد" : "لا تبريد";

                    return {
                        "الرقم": t.number || '--',
                        "النوع": typeText || '--',
                        "الشركة": t.company || '--',
                        "سعة التخزين": t.capacity ? t.capacity + ' kg' : '--',
                        "الموظف المضيف": t.host || '--'
                    };
                });
                const worksheet = XLSX.utils.json_to_sheet(excelData);
                const workbook = XLSX.utils.book_new();
                XLSX.utils.book_append_sheet(workbook, worksheet, "الشاحنات");
                XLSX.writeFile(workbook, "Trucks_Export.xlsx");
            });
        }

        if (closeModalBtn) closeModalBtn.addEventListener('click', () => noDataModal.style.display = 'none');
        renderTable();
    }


    // ====================================================================
    // 3. كود صفحة "السائقين"
    // ====================================================================
    else if (document.getElementById('driverTableBody')) {
        const tableBody = document.getElementById('driverTableBody');
        const searchInput = document.getElementById('driverSearch');
        const paginationContainer = document.getElementById('paginationContainer');
        const filterDropdownBtn = document.getElementById('filterDropdown');
        const sortBtnText = document.getElementById('sortBtnText');
        const sortMenu = document.getElementById('sortMenu');
        const exportBtn = document.getElementById('exportBtn');
        const noDataModal = document.getElementById('noDataModal');
        const closeModalBtn = document.getElementById('closeModalBtn');

        const state = {
            currentPage: 1,
            itemsPerPage: 10,
            searchTerm: "",
            sortBy: "name",
            sortDesc: false,
            sortLabels: {
                "name": "الإسم", "phone": "رقم الهاتف", "nid": "الرقم القومي",
                "type": "النوع", "host": "الموظف المضيف"
            },
            allData: Array.isArray(window.initialDriversData) ? window.initialDriversData : []
        };

        function filterDrivers() {
            let filtered = [...state.allData];
            if (state.searchTerm) {
                const term = state.searchTerm.toLowerCase();
                filtered = filtered.filter(d =>
                    (d.name && String(d.name).toLowerCase().includes(term)) ||
                    (d.phone && String(d.phone).toLowerCase().includes(term)) ||
                    (d.nid && String(d.nid).toLowerCase().includes(term)) ||
                    (d.type && String(d.type).toLowerCase().includes(term)) ||
                    (d.host && String(d.host).toLowerCase().includes(term))
                );
            }

            filtered.sort((a, b) => {
                let valA = a[state.sortBy] ?? "";
                let valB = b[state.sortBy] ?? "";

                valA = String(valA);
                valB = String(valB);

                const comparison = valA.localeCompare(valB, 'ar', { numeric: true });
                return state.sortDesc ? -comparison : comparison;
            });

            return filtered;
        }

        function renderTable() {
            if (!tableBody) return;
            const filteredData = filterDrivers();
            const totalPages = Math.ceil(filteredData.length / state.itemsPerPage);

            if (state.currentPage > totalPages && totalPages > 0) state.currentPage = 1;
            const startIndex = (state.currentPage - 1) * state.itemsPerPage;
            const pageData = filteredData.slice(startIndex, startIndex + state.itemsPerPage);

            tableBody.innerHTML = '';
            if (pageData.length === 0) {
                tableBody.innerHTML = `<tr><td colspan="6" class="empty-state">لا توجد نتائج مطابقة للبحث</td></tr>`;
            } else {
                pageData.forEach((driver) => {
                    const tr = document.createElement('tr');
                    tr.className = 'table-row';
                    const detailsUrl = window.mvcRoutes?.detailsUrl
                        ? `${window.mvcRoutes.detailsUrl}/${driver.id}`
                        : `/Driver/Details/${driver.id}`;

                    tr.innerHTML = `
                        <td class="col-name">${driver.name || '--'}</td>
                        <td class="col-phone">${driver.phone || '--'}</td>
                        <td class="col-nid">${driver.nid || '--'}</td>
                        <td class="col-type">${driver.type || '--'}</td>
                        <td class="col-host">${driver.host || '--'}</td>
                        <td class="col-action">
                            <a href="${detailsUrl}" class="btn-table-details" style="text-decoration: none;">
                                <span>تفاصيل</span>
                                <span class="arrow">&larr;</span>
                            </a>
                        </td>
                    `;
                    tableBody.appendChild(tr);
                });
            }
            renderPagination(totalPages);
        }

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
                const totalPages = Math.ceil(filterDrivers().length / state.itemsPerPage);

                if (target.id === 'prevPage' && state.currentPage > 1) {
                    state.currentPage -= 1;
                    renderTable();
                } else if (target.id === 'nextPage' && state.currentPage < totalPages) {
                    state.currentPage += 1;
                    renderTable();
                } else if (target.dataset.page) {
                    state.currentPage = parseInt(target.dataset.page);
                    renderTable();
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
                    if (state.sortBy === selectedSort) {
                        state.sortDesc = !state.sortDesc;
                    } else {
                        state.sortBy = selectedSort;
                        state.sortDesc = false;
                    }
                    const arrow = state.sortDesc ? "↓" : "↑";
                    sortBtnText.textContent = `تصنيف عبر : ${state.sortLabels[selectedSort]} ${arrow}`;
                    state.currentPage = 1;
                    renderTable();
                }
            });
        }

        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                const filteredData = filterDrivers();
                if (filteredData.length === 0) {
                    noDataModal.style.display = 'flex';
                    return;
                }
                const excelData = filteredData.map(d => ({
                    "الإسم": d.name || '--',
                    "رقم الهاتف": d.phone || '--',
                    "الرقم القومي": d.nid || '--',
                    "النوع": d.type || '--',
                    "الموظف المضيف": d.host || '--'
                }));

                const worksheet = XLSX.utils.json_to_sheet(excelData);
                const workbook = XLSX.utils.book_new();
                XLSX.utils.book_append_sheet(workbook, worksheet, "السائقين");
                XLSX.writeFile(workbook, "Drivers_Export.xlsx");
            });
        }

        if (closeModalBtn) closeModalBtn.addEventListener('click', () => noDataModal.style.display = 'none');
        renderTable();
    }
});