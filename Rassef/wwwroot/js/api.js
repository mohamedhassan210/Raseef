/**
 * RASEEF — API Layer
 * ===================
 * Mock data for development. 
 */

const BASE_URL = 'https://api.raseef.com/v1';
const MOCK_MODE = true;

function mockDelay(ms = 300) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

// ────────────────────────────────────────────────
//   EMPLOYEES MOCK GENERATOR (PERSISTENT)
// ────────────────────────────────────────────────
function getOrGenerateMockEmployees() {
    let data = localStorage.getItem('raseef_employees');
    if (data) {
        return JSON.parse(data);
    }

    const firstNames = ['أحمد', 'محمد', 'محمود', 'مصطفى', 'كريم', 'عمر', 'علي', 'حسين', 'طارق', 'زياد', 'يوسف', 'إسلام'];
    const lastNames = ['علي', 'الشناوي', 'حسن', 'إبراهيم', 'توفيق', 'عبدالله', 'سعيد', 'فاروق', 'صادق', 'عثمان', 'سعد'];
    const rolesList = ['مخازن', 'بوابة', 'مدير أحمال', 'مشرف', 'أمن', 'موارد بشرية', 'تشغيل', 'محاسب', 'مدير', 'موظف إداري'];

    const employees = [];
    for (let i = 1; i <= 400; i++) {
        employees.push({
            id: i,
            name: `${firstNames[Math.floor(Math.random() * firstNames.length)]} ${lastNames[Math.floor(Math.random() * lastNames.length)]} ${firstNames[Math.floor(Math.random() * firstNames.length)]}`,
            phone: `01${Math.floor(Math.random() * 3)}${Math.floor(Math.random() * 90000000 + 10000000)}`,
            email: `emp${i}@raseef.com`,
            role: rolesList[Math.floor(Math.random() * rolesList.length)],
            nationalId: `29${Math.floor(Math.random() * 900000000000 + 100000000000)}`,
            code: `J0${324000 + i}`
        });
    }

    localStorage.setItem('raseef_employees', JSON.stringify(employees));
    return employees;
}

// ────────────────────────────────────────────────
//   EMPLOYEES API
// ────────────────────────────────────────────────
const EmployeesAPI = {
    async getAll() {
        if (MOCK_MODE) {
            await mockDelay(300);
            return getOrGenerateMockEmployees();
        }
        return [];
    },

    async getById(id) {
        if (MOCK_MODE) {
            await mockDelay(200);
            const employees = getOrGenerateMockEmployees();
            return employees.find(emp => emp.id === Number(id)) || null;
        }
        return null;
    },

    async delete(id) {
        if (MOCK_MODE) {
            await mockDelay(400);
            let employees = getOrGenerateMockEmployees();
            employees = employees.filter(emp => emp.id !== Number(id));
            localStorage.setItem('raseef_employees', JSON.stringify(employees));
            return { success: true };
        }
        return { success: true };
    }, // <--- الفاصلة دي هي اللي كانت ناقصة وموقفة الدنيا

    // ── الدالة الجديدة المضافة للتعديل ──
    async update(id, updatedData) {
        if (MOCK_MODE) {
            await mockDelay(600); // محاكاة وقت التحميل
            let employees = getOrGenerateMockEmployees();
            const index = employees.findIndex(emp => emp.id === Number(id));

            if (index !== -1) {
                // دمج البيانات القديمة مع الجديدة
                employees[index] = { ...employees[index], ...updatedData };
                localStorage.setItem('raseef_employees', JSON.stringify(employees));
                return { success: true, data: employees[index] };
            }
            throw new Error("الموظف غير موجود");
        }
    }
};

/* ── Other Existing MOCK_DATA ── */
const MOCK_DATA = {
    suppliers: [
        { id: 1, name: 'شركة جهينة', phone: '01005568324', logo: null, logoText: 'جهينة', company: 'juhayna' },
        { id: 2, name: 'شركة حلواني اخوان', phone: '01005568324', logo: null, logoText: 'HB', company: 'halwani' },
        { id: 3, name: 'شركة اكوافينا', phone: '01005568324', logo: null, logoText: 'AQUAFINA', company: 'aquafina' },
        { id: 4, name: 'شركة المكتبة الرقمية', phone: '01005568324', logo: null, logoText: 'المكتبة الرقمية', company: 'digital-lib' }
    ],
    trucks: { /* existing trucks data */ },
    drivers: [ /* existing drivers data */],
};

async function apiRequest(endpoint, options = {}) {
    const url = `${BASE_URL}${endpoint}`;
    const defaults = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('raseef_token') || ''}`,
        },
    };
    const response = await fetch(url, { ...defaults, ...options });
    if (!response.ok) {
        throw new Error(`API Error: ${response.status}`);
    }
    return response.json();
}

const Auth = {
    async login(username, password) {
        if (MOCK_MODE) {
            await mockDelay(600);
            if (username && password) {
                const token = 'mock_token_' + Date.now();
                localStorage.setItem('raseef_token', token);
                localStorage.setItem('raseef_user', username);
                return { success: true, token, user: { name: username } };
            }
            throw new Error('بيانات خاطئة');
        }
        return apiRequest('/auth/login', { method: 'POST', body: JSON.stringify({ username, password }) });
    },
    logout() {
        localStorage.removeItem('raseef_token');
        localStorage.removeItem('raseef_user');
        window.location.href = 'login.html';
    },
    isLoggedIn() { return true; },
};

const SuppliersAPI = {
    async getAll(search = '') {
        if (MOCK_MODE) {
            await mockDelay(300);
            let list = MOCK_DATA.suppliers;
            if (search) list = list.filter(s => s.name.includes(search) || s.phone.includes(search));
            return list;
        }
        return apiRequest(`/suppliers${search ? '?search=' + encodeURIComponent(search) : ''}`);
    }
};

const TrucksAPI = { /* Your existing TrucksAPI functions here */ };
const DriversAPI = { /* Your existing DriversAPI functions here */ };