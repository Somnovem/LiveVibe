let currentPage = 1;
const pageSize = 6;
let totalPages = 1;

// Elements
const eventsGrid = document.getElementById('eventsGrid');
const prevButton = document.getElementById('prevPage');
const nextButton = document.getElementById('nextPage');
const pageInfo = document.getElementById('pageInfo');

// Fetch events from the API
async function fetchEvents(page) {
    try {
        const response = await fetch(`/api/events/all?pageNumber=${page}&pageSize=${pageSize}`);
        if (!response.ok) throw new Error('Failed to fetch events');
        const data = await response.json();
        console.log(data);
        totalPages = Math.ceil(data.totalCount / pageSize);
        return data;
    } catch (error) {
        console.error('Error fetching events:', error);
        return null;
    }
}

// Format date
function formatDate(dateString) {
    const options = { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    };
    return new Date(dateString).toLocaleDateString('en-US', options);
}

// Format price range
function formatPriceRange(seatTypes) {
    if (!seatTypes || seatTypes.length === 0) return 'Price not available';
    
    const prices = seatTypes.map(st => st.price);
    const minPrice = Math.min(...prices);
    const maxPrice = Math.max(...prices);
    
    if (minPrice === maxPrice) {
        return `${minPrice.toFixed(2)} UAH`;
    }
    return `${minPrice.toFixed(2)} - ${maxPrice.toFixed(2)} UAH`;
}

// Create seat types HTML
function createSeatTypesHtml(seatTypes) {
    if (!seatTypes || seatTypes.length === 0) return '';
    
    return `
        <div class="seat-types">
            <h3>Available Tickets:</h3>
            ${seatTypes.map(st => `
                <div class="seat-type">
                    <span class="seat-name">${st.name}</span>
                    <span class="seat-price">${st.price.toFixed(2)} UAH</span>
                </div>
            `).join('')}
        </div>
    `;
}

// Create event card HTML
function createEventCard(event) {
    const defaultImage = '/images/events/default-event.jpg';
    return `
        <article class="event-card">
            <img 
                src="${event.imageUrl || defaultImage}" 
                alt="${event.title}"
                class="event-image"
                onerror="this.src='${defaultImage}'"
            >
            <div class="event-info">
                <h2 class="event-title">${event.title}</h2>
                <div class="price-range">Tickets from ${formatPriceRange(event.seatTypes)}</div>
                <p class="event-description">${event.description || 'No description available'}</p>
                <div class="event-details">
                    <div class="event-detail">
                        <span>When:</span>
                        <span>${formatDate(event.time)}</span>
                    </div>
                    <div class="event-detail">
                        <span>Where:</span>
                        <span>${event.location}, ${event.city}</span>
                    </div>
                    <div class="event-detail">
                        <span>Category:</span>
                        <span>${event.category}</span>
                    </div>
                    <div class="event-detail">
                        <span>Organizer:</span>
                        <span>${event.organizer}</span>
                    </div>
                </div>
                ${createSeatTypesHtml(event.seatTypes)}
            </div>
        </article>
    `;
}

// Update pagination controls
function updatePagination() {
    prevButton.disabled = currentPage === 1;
    nextButton.disabled = currentPage === totalPages;
    pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;
}

// Load and display events
async function loadEvents(page) {
    const data = await fetchEvents(page);
    if (!data) {
        eventsGrid.innerHTML = '<p>Failed to load events. Please try again later.</p>';
        return;
    }

    eventsGrid.innerHTML = data.items
        .map(event => createEventCard(event))
        .join('');

    updatePagination();
}

// Event listeners
prevButton.addEventListener('click', () => {
    if (currentPage > 1) {
        currentPage--;
        loadEvents(currentPage);
    }
});

nextButton.addEventListener('click', () => {
    if (currentPage < totalPages) {
        currentPage++;
        loadEvents(currentPage);
    }
});

// Initial load
loadEvents(currentPage); 