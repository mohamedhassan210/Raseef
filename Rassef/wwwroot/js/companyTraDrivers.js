document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.getElementById('search-input');
    const cardsContainer = document.getElementById('cards-container');

    const filterTrucks = () => {
        if (!searchInput || !cardsContainer) return;
        const query = searchInput.value.trim().toLowerCase();
        const cards = cardsContainer.querySelectorAll('.truck-card');

        cards.forEach(card => {
            const plate = (card.getAttribute('data-plate') || "").toLowerCase();
            card.style.display = plate.replace(/\s+/g, '').includes(query.replace(/\s+/g, '')) ? 'flex' : 'none';
        });
    };

    if (searchInput) searchInput.addEventListener('input', filterTrucks);

    document.querySelectorAll('.select-btn').forEach(button => {
        button.addEventListener('click', (e) => {
            const card = e.target.closest('.truck-card');
            const truckId = card ? card.getAttribute('data-truck-id') : null;
            if (!truckId) return;

            const base = window.routes?.transferDriversPage || '/Driver/TransferDrivers';
            window.location.href = `${base}?id=${truckId}`;
        });
    });
});