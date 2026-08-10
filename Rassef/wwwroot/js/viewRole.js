// بيانات وهمية للأدوار لتجربة النظام
const mockRoles = [
    {
        roleNumber: 22,
        status: "تم",
        employeeName: "محمد السيد الشناوي",
        truckNumber: "أ س ق 2356",
        yardNumber: 5,
        department: "المخزونات",
        time: "02:50 AM"
    },
    {
        roleNumber: 23,
        status: "جاري",
        employeeName: "أحمد محمود علي",
        truckNumber: "س ب ع 1234",
        yardNumber: 3,
        department: "المخزونات",
        time: "03:15 AM"
    },
    {
        roleNumber: 24,
        status: "إنتظار",
        employeeName: "كريم حسن عبد الله",
        truckNumber: "ط ق ص 9876",
        yardNumber: 2,
        department: "العمليات",
        time: "03:40 AM"
    },
    {
        roleNumber: 25,
        status: "إنتظار",
        employeeName: "عمر فاروق",
        truckNumber: "ر م ن 4567",
        yardNumber: 5,
        department: "المخزونات",
        time: "04:00 AM"
    },
    {
        roleNumber: 26,
        status: "تم",
        employeeName: "سعيد عبد الرحمن",
        truckNumber: "ج ح خ 3321",
        yardNumber: 1,
        department: "الصيانة",
        time: "01:20 AM"
    },
    {
        roleNumber: 27,
        status: "جاري",
        employeeName: "محمود زكريا",
        truckNumber: "ص ض ط 7654",
        yardNumber: 4,
        department: "العمليات",
        time: "04:10 AM"
    },
    {
        roleNumber: 28,
        status: "تم",
        employeeName: "مصطفى كامل",
        truckNumber: "ع غ ف 1111",
        yardNumber: 2,
        department: "المخزونات",
        time: "12:05 AM"
    },
    {
        roleNumber: 29,
        status: "إنتظار",
        employeeName: "ياسر إبراهيم",
        truckNumber: "ق ك ل 9999",
        yardNumber: 3,
        department: "التوزيع",
        time: "04:30 AM"
    }
];

// المتغيرات الأساسية
let currentFilter = "الكل";
let searchQuery = "";

// العناصر من DOM
const rolesContainer = document.getElementById('rolesContainer');
const searchInput = document.getElementById('searchInput');
const filterBtns = document.querySelectorAll('.filter-btn');

const waitCountEl = document.getElementById('waitCount');
const activeCountEl = document.getElementById('activeCount');
const doneCountEl = document.getElementById('doneCount');

// دالة لتحديد فئة CSS الخاصة بالحالة
function getStatusClass(status) {
    switch (status) {
        case "تم": return "status-done";
        case "جاري": return "status-active";
        case "إنتظار": return "status-wait";
        default: return "";
    }
}

// دالة تحديث الإحصائيات (تُحسب دائماً من البيانات المعروضة)
function updateStats(filteredData) {
    const waitCount = filteredData.filter(role => role.status === "إنتظار").length;
    const activeCount = filteredData.filter(role => role.status === "جاري").length;
    const doneCount = filteredData.filter(role => role.status === "تم").length;

    waitCountEl.textContent = waitCount;
    activeCountEl.textContent = activeCount;
    doneCountEl.textContent = doneCount;
}

// دالة لتوليد كود البطاقة الواحدة
function createRoleCard(role) {
    const statusClass = getStatusClass(role.status);

    return `
        <div class="role-card">
            <div class="role-number-group">
                <span class="role-number-val">${role.roleNumber}</span>
                <span class="role-number-label">رقم الدور</span>
            </div>

            <div class="status-badge ${statusClass}">
                <span class="status-dot-small"></span>
                ${role.status}
            </div>

            <div class="info-group">
                <i class="fas fa-user info-icon"></i>
                <div class="info-text">
                    <span class="info-value">${role.employeeName}</span>
                    <span class="info-label">اسم السائق</span>
                </div>
            </div>

            <div class="info-group">
                <i class="fas fa-truck info-icon"></i>
                <div class="info-text">
                    <span class="info-value">${role.truckNumber}</span>
                    <span class="info-label">رقم السيارة</span>
                </div>
            </div>

            <div class="info-group">
                <i class="fas fa-columns info-icon"></i>
                <div class="info-text">
                    <span class="info-value">رصيف ${role.yardNumber}</span>
                    <span class="info-label">رقم الرصيف</span>
                </div>
            </div>

            <div class="info-group">
                <i class="fas fa-sitemap info-icon"></i>
                <div class="info-text">
                    <span class="info-value">${role.department}</span>
                    <span class="info-label">القسم</span>
                </div>
            </div>

            <div class="info-group">
                <i class="far fa-clock info-icon"></i>
                <div class="info-text">
                    <span class="info-value">${role.time}</span>
                    <span class="info-label">وقت الدخول</span>
                </div>
            </div>
        </div>
    `;
}

// دالة الفلترة والريندر (الأساسية)
function renderRoles() {
    let filteredRoles = mockRoles;

    // الفلترة بالبحث النصي
    if (searchQuery) {
        filteredRoles = filteredRoles.filter(role => 
            role.employeeName.includes(searchQuery) ||
            role.truckNumber.includes(searchQuery) ||
            role.roleNumber.toString().includes(searchQuery)
        );
    }

    // فلترة الأرقام للإحصائيات قبل فلترة الحالة
    // (الإحصائيات ديماً بتعتمد على ناتج البحث)
    updateStats(filteredRoles);

    // الفلترة بحالة الدور
    if (currentFilter !== "الكل") {
        filteredRoles = filteredRoles.filter(role => role.status === currentFilter);
    }

    // عرض البطاقات
    if (filteredRoles.length === 0) {
        rolesContainer.innerHTML = `<div class="empty-state">لا توجد أدوار مطابقة لبحثك</div>`;
    } else {
        rolesContainer.innerHTML = filteredRoles.map(role => createRoleCard(role)).join('');
    }
}

// مستمع لحدث الكتابة في مربع البحث
searchInput.addEventListener('input', (e) => {
    searchQuery = e.target.value.trim();
    renderRoles();
});

// مستمع لحدث الضغط على أزرار الفلترة
filterBtns.forEach(btn => {
    btn.addEventListener('click', (e) => {
        // إزالة الكلاس active من كل الأزرار
        filterBtns.forEach(b => b.classList.remove('active'));
        
        // إضافة الكلاس للزر اللي تم الضغط عليه
        e.target.classList.add('active');
        
        // تحديث الفلتر الحالي وتحديث العرض
        currentFilter = e.target.getAttribute('data-filter');
        renderRoles();
    });
});

// التشغيل المبدئي عند فتح الصفحة
renderRoles();