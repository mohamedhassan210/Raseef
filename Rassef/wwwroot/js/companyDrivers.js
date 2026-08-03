document.addEventListener('DOMContentLoaded', () => {

    // ==========================================
    // 1. DOM Elements
    // ==========================================
    const searchInput = document.getElementById('search-input');
    const cardsContainer = document.getElementById('cards-container');

    // Modal Elements
    const modalOverlay = document.getElementById('custom-modal-overlay');
    const confirmModal = document.getElementById('confirmation-modal');
    const successModal = document.getElementById('success-modal');

    const btnEdit = document.getElementById('btn-edit');
    const btnConfirm = document.getElementById('btn-confirm');
    const btnBack = document.getElementById('btn-back');
    const btnPrint = document.getElementById('btn-print');

    const confirmCompany = document.getElementById('confirm-company');
    const confirmTruck = document.getElementById('confirm-truck');
    const confirmDriverName = document.getElementById('confirm-driver-name');
    const ticketNumberDisplay = document.getElementById('ticket-number');

    // ==========================================
    // 2. Search & Filter Logic 
    // ==========================================
    const filterDrivers = () => {
        const query = searchInput.value.trim().toLowerCase();
        const cards = cardsContainer.querySelectorAll('.driver-card');

        cards.forEach(card => {
            const name = (card.getAttribute('data-name') || "").toLowerCase();
            const nationalId = (card.getAttribute('data-id') || "").toLowerCase();

            if (name.includes(query) || nationalId.includes(query)) {
                card.style.display = 'flex';
                card.style.animation = 'fadeIn 0.3s ease-in-out';
            } else {
                card.style.display = 'none';
            }
        });
    };

    if (searchInput) {
        searchInput.addEventListener('input', filterDrivers);
    }

    // ==========================================
    // 3. Modal System & Selection Workflow
    // ==========================================
    const showConfirmationModal = (modalElement) => {
        confirmModal.classList.remove('active-modal');
        confirmModal.classList.add('d-none');
        successModal.classList.remove('active-modal');
        successModal.classList.add('d-none');

        modalOverlay.classList.add('active');
        modalElement.classList.remove('d-none');

        setTimeout(() => {
            modalElement.classList.add('active-modal');
        }, 10);

        document.body.style.overflow = 'hidden';
    };

    const closeModal = () => {
        modalOverlay.classList.remove('active');
        confirmModal.classList.remove('active-modal');
        successModal.classList.remove('active-modal');
        document.body.style.overflow = '';
    };

    const populateConfirmationData = (driverName) => {
        // البيانات الوهمية الثابتة من الصفحة
        const company = window.pageData.companyName || "جهينة";
        const truck = window.pageData.truckName || "سيارة تبريد";

        confirmCompany.textContent = company;
        confirmTruck.textContent = truck;
        confirmDriverName.textContent = driverName;
    };

    // تفعيل زر "اختيار السائق" لكل كارت
    const attachSelectionEvents = () => {
        const selectButtons = document.querySelectorAll('.select-btn');
        selectButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                const card = e.target.closest('.driver-card');
                const driverName = card.getAttribute('data-name');

                populateConfirmationData(driverName);
                showConfirmationModal(confirmModal);
            });
        });
    };
    attachSelectionEvents();

    // ==========================================
    // 4. Ticket Badge Visual Counter Logic (Shift Reset)
    // ==========================================
    let shiftTicketCounter = 0;

    const generateTicketNumber = () => {
        shiftTicketCounter++;
        let rawDockName = window.pageData.dockName;
        let dockInitial = rawDockName && rawDockName.length > 0
            ? rawDockName.charAt(0).toUpperCase()
            : 'A';

        return `${dockInitial}${shiftTicketCounter}`;
    };

    const printTicket = () => {
        btnPrint.textContent = "جاري الطباعة...";
        btnPrint.disabled = true;

        setTimeout(() => {
            window.location.href = window.routes.authSupOrTra;
        }, 1500);
    };

    // ==========================================
    // 5. Modal Button Events & Routing
    // ==========================================
    const initializeModals = () => {
        // Step 1: Edit Button 
        btnEdit.addEventListener('click', () => {
            window.location.href = window.routes.supplierIndex;
        });

        // Step 1: Confirm Button -> Progress to Step 2 (Ticket)
        btnConfirm.addEventListener('click', () => {
            const ticketNum = generateTicketNumber();
            ticketNumberDisplay.textContent = ticketNum;

            showConfirmationModal(successModal);
        });

        // Step 2: Back Button 
        btnBack.addEventListener('click', () => {
            window.location.href = window.routes.supplierIndex;
        });

        // Step 2: Print Button 
        btnPrint.addEventListener('click', () => {
            printTicket();
        });

        // Overlay Click -> Close (Only if in Step 1)
        modalOverlay.addEventListener('click', (e) => {
            if (e.target === modalOverlay && !confirmModal.classList.contains('d-none')) {
                closeModal();
            }
        });

        // Keyboard ESC -> Close (Only if in Step 1)
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && modalOverlay.classList.contains('active')) {
                if (!confirmModal.classList.contains('d-none')) {
                    closeModal();
                }
            }
        });
    };

    initializeModals();
});