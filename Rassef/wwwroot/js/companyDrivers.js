document.addEventListener('DOMContentLoaded', () => {
    
    // ==========================================
    // 1. DOM Elements
    // ==========================================
    
    // Page Elements
    const pageTitle = document.getElementById('page-title');
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
    // 2. SVGs for UI mapping
    // ==========================================
    const ICONS = {
        avatar: '<path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>',
        phone: '<path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>',
        idCard: '<path d="M21 3H3c-1.11 0-2 .89-2 2v14c0 1.11.89 2 2 2h18c1.11 0 2-.89 2-2V5c0-1.11-.89-2-2-2zm-9 15H4v-2h8v2zm0-4H4v-2h8v2zm0-4H4V8h8v2zm7 8h-4v-6h4v6zm0-8h-4V8h4v2z"/>'
    };

    // ==========================================
    // 3. Page Initialization & Rendering Data
    // ==========================================
    
    const initializePage = () => {
        const storedTruckData = JSON.parse(localStorage.getItem('selectedTruck'));
        
        // تغيير العنوان بناءً على الشاحنة المختارة، أو افتراضي "جهينة"
        if (storedTruckData && storedTruckData.company) {
            pageTitle.textContent = `سائقين ${storedTruckData.company}`;
        } else {
            pageTitle.textContent = 'سائقين جهينة';
        }

        loadSavedDrivers();
    };

    const saveDummyDrivers = () => {
        const dummyDrivers = [
            { id: 1, name: "محمد محمد ابوتريكة", phone: "01003526597", nationalId: "032023569810012" },
            { id: 2, name: "محمد محمود سعد غلاب", phone: "01003526597", nationalId: "032023569810012" },
            { id: 3, name: "محمد السيد بدير الشناوي", phone: "01003526597", nationalId: "032023569810012" },
            { id: 4, name: "فارس محمد عشري آمان", phone: "01003526597", nationalId: "032023569810012" },
            { id: 5, name: "احمد علاء احمد علي", phone: "01003526597", nationalId: "032023569810012" }
        ];
        localStorage.setItem('drivers', JSON.stringify(dummyDrivers));
        return dummyDrivers;
    };

    const loadSavedDrivers = () => {
        let savedDrivers = JSON.parse(localStorage.getItem('drivers'));
        
        if (!savedDrivers || savedDrivers.length === 0) {
            savedDrivers = saveDummyDrivers();
        }
        
        renderDrivers(savedDrivers);
    };

    const renderDrivers = (drivers) => {
        cardsContainer.innerHTML = '';
        const fragment = document.createDocumentFragment();

        drivers.forEach(driver => {
            const article = document.createElement('article');
            article.className = 'driver-card';
            article.setAttribute('data-name', driver.name);
            article.setAttribute('data-id', driver.nationalId);

            article.innerHTML = `
                <div class="btnAndArticle">  
                    <div class="driver-avatar" aria-hidden="true">
                        <svg viewBox="0 0 24 24" fill="currentColor">
                            ${ICONS.avatar}
                        </svg>
                    </div>
                
                    <div class="card-details">
                        <h2 class="driver-name">${driver.name}</h2>
                        <div class="driver-meta">
                            <div class="meta-item">
                                <svg class="type-icon" viewBox="0 0 24 24" fill="currentColor">${ICONS.phone}</svg>
                                <span>${driver.phone}</span>
                            </div>
                            <div class="meta-item">
                                <svg class="type-icon" viewBox="0 0 24 24" fill="currentColor">${ICONS.idCard}</svg>
                                <span>${driver.nationalId}</span>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="card-action">
                    <button class="select-btn" type="button" aria-label="اختيار السائق ${driver.name}">اختيار السائق</button>
                </div>
            `;
            
            fragment.appendChild(article);
        });

        cardsContainer.appendChild(fragment);
        attachSelectionEvents();
    };

    const attachSelectionEvents = () => {
        const selectButtons = document.querySelectorAll('.select-btn');
        selectButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                selectDriver(e.target.closest('.driver-card'));
            });
        });
    };

    const filterDrivers = () => {
        const query = searchInput.value.trim().toLowerCase();
        const cards = cardsContainer.querySelectorAll('.driver-card'); 

        cards.forEach(card => {
            const name = card.getAttribute('data-name').toLowerCase();
            const nationalId = card.getAttribute('data-id').toLowerCase();
            
            if (name.includes(query) || nationalId.includes(query)) {
                card.style.display = 'flex';
                card.style.animation = 'fadeIn 0.3s ease-in-out';
            } else {
                card.style.display = 'none';
            }
        });
    };

    // ==========================================
    // 4. Modal System & Two-Step Workflow
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
        const fallbackValue = "غير متوفر";
        
        const savedTruck = JSON.parse(localStorage.getItem('selectedTruck')) || {
            company: fallbackValue,
            plate: fallbackValue,
            type: "سيارة" 
        };

        const safeType = savedTruck.type || "سيارة";

        const truckDisplay = savedTruck.plate !== fallbackValue 
            ? `${safeType} (${savedTruck.plate})` 
            : fallbackValue;

        confirmCompany.textContent = savedTruck.company || fallbackValue;
        confirmTruck.textContent = truckDisplay;
        confirmDriverName.textContent = driverName || fallbackValue;
    };

    const selectDriver = (cardElement) => {
        const driverName = cardElement.getAttribute('data-name');
        
        populateConfirmationData(driverName);
        
        localStorage.setItem('pendingDriver', JSON.stringify({
            name: driverName,
            nationalId: cardElement.getAttribute('data-id')
        }));

        showConfirmationModal(confirmModal);
    };

    const generateTicketNumber = () => {
        let lastTicket = parseInt(localStorage.getItem('lastTicketNumber')) || 99;
        const newTicket = lastTicket + 1;
        localStorage.setItem('lastTicketNumber', newTicket.toString());
        return newTicket;
    };

    const printTicket = () => {
        btnPrint.textContent = "جاري الطباعة...";
        btnPrint.disabled = true;

        setTimeout(() => {
            window.location.href = "beforhome.html";
        }, 1500);
    };

    const initializeModals = () => {
        // Step 1: Edit Button -> Redirect
        btnEdit.addEventListener('click', () => {
            window.location.href = "suppliers.html";
        });

        // Step 1: Confirm Button -> Progress to Step 2
        btnConfirm.addEventListener('click', () => {
            const pending = localStorage.getItem('pendingDriver');
            if (pending) {
                localStorage.setItem('selectedDriver', pending);
                localStorage.removeItem('pendingDriver');
            }

            const ticketNum = generateTicketNumber();
            ticketNumberDisplay.textContent = ticketNum;
            
            showConfirmationModal(successModal);
        });

        // Step 2: Back Button -> Redirect
        btnBack.addEventListener('click', () => {
            window.location.href = "suppliers.html";
        });

        // Step 2: Print Button -> Disable -> Redirect
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

    // ==========================================
    // 5. Execution
    // ==========================================
    
    if (searchInput) {
        searchInput.addEventListener('input', filterDrivers);
    }

    initializePage();
    initializeModals();

});