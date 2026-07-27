document.addEventListener('DOMContentLoaded', () => {
    // Select DOM Elements
    const searchInput = document.getElementById('search-input');
    const cardsContainer = document.getElementById('cards-container');
    const cards = cardsContainer.querySelectorAll('.truck-card');
  
    /**
     * Filter cards based on user search query
     */
    const handleSearch = () => {
      const query = searchInput.value.trim().toLowerCase();
  
      cards.forEach(card => {
        const plateText = card.getAttribute('data-plate').toLowerCase();
        const vehicleTitle = card.querySelector('.vehicle-title').textContent.toLowerCase();
        
        // Matches license plate or title
        if (plateText.includes(query) || vehicleTitle.includes(query)) {
          card.style.display = 'flex';
          card.style.animation = 'fadeIn 0.3s ease-in-out';
        } else {
          card.style.display = 'none';
        }
      });
    };
  
    // Event Listeners for real-time search
    searchInput.addEventListener('input', handleSearch);
  
    // Optional button interactions
    const selectButtons = document.querySelectorAll('.select-btn');
    selectButtons.forEach(button => {
      button.addEventListener('click', (e) => {
        const card = e.target.closest('.truck-card');
        const plate = card.getAttribute('data-plate');
        console.log(`Car selected: ${plate}`);
      });
    });
  });