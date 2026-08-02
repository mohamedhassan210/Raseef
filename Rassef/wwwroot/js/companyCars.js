document.addEventListener('DOMContentLoaded', () => {
  // 1. تحديد العناصر الأساسية من الـ DOM
  const searchInput = document.getElementById('search-input');
  const cardsContainer = document.getElementById('cards-container');

  // 2. دالة استرجاع وعرض الشاحنات المضافة حديثاً من LocalStorage
  const loadSavedTrucks = () => {
      const savedTrucks = JSON.parse(localStorage.getItem('trucks')) || [];
      
      savedTrucks.forEach(truck => {
          const isCooling = truck.type === "تبريد";
          const iconPath = isCooling 
              ? "M11 9H9V2H7v7H5V2H3v7c0 2.12 1.46 3.91 3.45 4.38L6 22h2l.55-8.62C10.54 12.91 12 11.12 12 9V2h-1v7zm7-7s-3 0-3 5v6h2v9h2V2z" 
              : "M20 8h-3V4H3c-1.1 0-2 .9-2 2v11h2c0 1.66 1.34 3 3 3s3-1.34 3-3h6c0 1.66 1.34 3 3 3s3-1.34 3-3h2v-5l-3-4zM6 18.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zm13.5-1.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zM17 12V9.5h2.47l1.88 2.5H17z";

              const cardHTML = `
              <article class="truck-card" data-plate="${truck.plateNumbers} ${truck.plateLetters}">
                <div class="btnAndArticle">  
                  <div class="license-plate" aria-label="لوحة السيارة: ${truck.plateNumbers} ${truck.plateLetters}">
                    <div class="plate-header">
                      <span class="plate-country">مصر</span>
                      <span class="plate-country-en">EGYPT</span>
                    </div>
                    <div class="plate-body">
                      <span class="plate-numbers">${truck.plateNumbers}</span>
                      <span class="plate-letters">${truck.plateLetters}</span>
                    </div>
                  </div>
              
                  <div class="card-details">
                    <h2 class="vehicle-title">سيارة ${truck.type}</h2>
                    <div class="d-flex gap-2 details">
                        <div class="vehicle-type">
                            <svg class="type-icon" viewBox="0 0 24 24" fill="currentColor">
                                <path d="${iconPath}"/>
                            </svg>
                            <span>${isCooling ? 'غذائي' : 'غير غذائي'}</span>
                        </div>
                    </div>
                  </div>
                </div>
              
                <div class="card-action">
                  <button class="select-btn" type="button">اختيار السيارة</button>
                </div>
              </article>
              `;
          // إضافة الشاحنة الجديدة في أعلى القائمة لتظهر أولاً
          cardsContainer.insertAdjacentHTML('afterbegin', cardHTML);
      });
  };

  // 3. تشغيل دالة جلب الشاحنات (يجب أن تعمل قبل تعريف كود البحث)
  loadSavedTrucks();

  // 4. تحديد الكروت والأزرار (بعد أن تم إضافة الشاحنات الجديدة للمتصفح)
  const cards = cardsContainer.querySelectorAll('.truck-card');
  const selectButtons = document.querySelectorAll('.select-btn');

  // 5. نظام البحث (Filter)
  const handleSearch = () => {
      const query = searchInput.value.trim().toLowerCase();

      cards.forEach(card => {
          const plateText = card.getAttribute('data-plate').toLowerCase();
          const vehicleTitle = card.querySelector('.vehicle-title').textContent.toLowerCase();
           
          // يبحث برقم اللوحة أو اسم الشاحنة
          if (plateText.includes(query) || vehicleTitle.includes(query)) {
              card.style.display = 'flex';
              card.style.animation = 'fadeIn 0.3s ease-in-out';
          } else {
              card.style.display = 'none';
          }
      });
  };

  // تفعيل البحث عند الكتابة
  if (searchInput) {
      searchInput.addEventListener('input', handleSearch);
  }

  // 6. تفعيل حدث الضغط على زر "اختيار السيارة"
  selectButtons.forEach(button => {
    button.addEventListener('click', (e) => {
        const card = e.target.closest('.truck-card');
        const plate = card.getAttribute('data-plate');
         
        // استخراج اسم الشركة 
        const companyElement = card.querySelectorAll('.vehicle-type span')[1];
        const companyName = companyElement ? companyElement.textContent : 'جهينة';

        // 💡 [الجزء المتعدل هنا]: استخراج نوع السيارة (مثل: سيارة تبريد)
        const typeElement = card.querySelector('.vehicle-title');
        const truckType = typeElement ? typeElement.textContent : 'سيارة';

        // حفظ البيانات كاملة في الـ LocalStorage بما فيها النوع
        const selectedTruck = {
            plate: plate,
            company: companyName,
            type: truckType // تم إضافة دي
        };
        localStorage.setItem('selectedTruck', JSON.stringify(selectedTruck));
        
        console.log(`Car selected: ${plate}, Company: ${companyName}, Type: ${truckType}`);
        
        // الانتقال الفوري لصفحة السائقين
        window.location.href = "companyDrivers.html";
    });
  });
});