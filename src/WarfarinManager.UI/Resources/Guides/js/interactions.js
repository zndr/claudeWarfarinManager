// --- DATA STORE ---
// Comprehensive list of interactions categorized by risk and type
const interactionData = [
    // ANTIBIOTICS (High Risk)
    {
        id: 1,
        name: "Ciprofloxacina",
        category: "Antibiotici",
        risk: "Alto",
        icon: "💊",
        mechanism: "Inibisce il metabolismo del Warfarin.",
        effect: "Aumento INR 📈 (Rischio Emorragia)",
        advice: "Interazione molto comune. Solitamente richiede una riduzione della dose di Warfarin durante la terapia antibiotica. Monitorare INR ogni 2-3 giorni."
    },
    {
        id: 2,
        name: "Eritromicina / Claritromicina",
        category: "Antibiotici",
        risk: "Alto",
        icon: "💊",
        mechanism: "Potente inibitore enzimatico epatico.",
        effect: "Aumento INR 📈 (Rischio Emorragia)",
        advice: "Evitare se possibile o ridurre preventivamente il Warfarin del 20-30% sotto consiglio medico. Controllo INR stretto."
    },
    {
        id: 3,
        name: "Bactrim (Sulfametoxazolo)",
        category: "Antibiotici",
        risk: "Alto",
        icon: "💊",
        mechanism: "Sposta il Warfarin dalle proteine plasmatiche e ne inibisce il metabolismo.",
        effect: "Aumento INR Drastico 📈",
        advice: "Una delle interazioni più pericolose. Il rischio di emorragia è molto alto. Necessario aggiustamento dosaggio immediato."
    },
    // NSAIDS / PAIN (High/Med Risk)
    {
        id: 4,
        name: "Aspirina / FANS",
        category: "Antinfiammatori",
        risk: "Alto",
        icon: "🦴",
        mechanism: "Danneggia mucosa gastrica e inibisce funzione piastrinica.",
        effect: "Rischio Sanguinamento 🩸 (INR può restare invariato)",
        advice: "Pericoloso perché aumenta il rischio di emorragia (specialmente gastrica) anche senza alzare l'INR. Usare solo se prescritto da cardiologo (es. prevenzione secondaria)."
    },
    {
        id: 5,
        name: "Ibuprofene / Ketoprofene",
        category: "Antinfiammatori",
        risk: "Medio",
        icon: "🦴",
        mechanism: "Effetto antiaggregante e gastrolesivo.",
        effect: "Rischio Sanguinamento 🩸",
        advice: "Evitare l'uso cronico. Per il dolore occasionale, preferire il Paracetamolo."
    },
    {
        id: 6,
        name: "Paracetamolo (Tachipirina)",
        category: "Analgesici",
        risk: "Medio",
        icon: "🤒",
        mechanism: "Interazione metabolica ad alti dosaggi.",
        effect: "Aumento INR 📈 (se dose > 2g/die)",
        advice: "Sicuro a basse dosi. Se assunto a pieno dosaggio (es. 3g al giorno) per più giorni consecutivi, può alzare l'INR."
    },
    // CARDIO
    {
        id: 7,
        name: "Amiodarone",
        category: "Cardiovascolari",
        risk: "Alto",
        icon: "🫀",
        mechanism: "Blocca potentemente l'eliminazione del Warfarin.",
        effect: "Aumento INR 📈",
        advice: "L'effetto persiste anche settimane dopo la sospensione dell'Amiodarone (emivita lunghissima). Dose Warfarin spesso va dimezzata."
    },
    // FOOD & HERBS
    {
        id: 8,
        name: "Verdure a Foglia Larga",
        category: "Cibo",
        risk: "Cibo",
        icon: "🥦",
        mechanism: "Ricche di Vitamina K (antagonista del Warfarin).",
        effect: "Diminuzione INR 📉 (Rischio Trombosi)",
        advice: "Spinaci, broccoli, cavoli. NON vanno eliminati, ma mangiati in quantità costante. Evitare le 'abbuffate' occasionali."
    },
    {
        id: 9,
        name: "Succo di Mirtillo",
        category: "Cibo",
        risk: "Cibo",
        icon: "🫐",
        mechanism: "Inibizione enzimatica dei flavonoidi.",
        effect: "Aumento INR 📈",
        advice: "Grandi quantità di succo concentrato possono destabilizzare la terapia. Consumo moderato è accettabile."
    },
    {
        id: 10,
        name: "Alcool",
        category: "Cibo",
        risk: "Medio",
        icon: "🍷",
        mechanism: "Acuto: inibisce metabolismo. Cronico: induce metabolismo.",
        effect: "Variabile 📉📈",
        advice: "L'intossicazione acuta (ubriacatura) alza l'INR (rischio emorragia). L'alcolismo cronico può abbassarlo. Consigliata moderazione (1 bicchiere)."
    },
    {
        id: 11,
        name: "Ginkgo Biloba",
        category: "Erbe",
        risk: "Alto",
        icon: "🌿",
        mechanism: "Attività antiaggregante piastrinica.",
        effect: "Rischio Sanguinamento 🩸",
        advice: "Evitare. Aumenta il rischio di emorragie spontanee anche con INR normale."
    },
    // OTHERS
    {
        id: 12,
        name: "Fluconazolo",
        category: "Antifungini",
        risk: "Alto",
        icon: "🍄",
        mechanism: "Inibitore CYP2C9.",
        effect: "Aumento INR 📈",
        advice: "Interazione forte. Monitorare strettamente INR se prescritta terapia per candida o micosi."
    },
    {
        id: 13,
        name: "Carbamazepina",
        category: "Antiepilettici",
        risk: "Medio",
        icon: "🧠",
        mechanism: "Induttore enzimatico (accelera eliminazione Warfarin).",
        effect: "Diminuzione INR 📉",
        advice: "Potrebbe essere necessario aumentare la dose di Warfarin per mantenere l'effetto terapeutico."
    }
];

// --- UI & LOGIC ---

const grid = document.getElementById('cardsGrid');
const searchInput = document.getElementById('searchInput');
const filterBtns = document.querySelectorAll('.filter-btn');
const modal = document.getElementById('detailModal');
const modalContent = document.getElementById('modalContent');
const noResults = document.getElementById('noResults');

let currentFilter = 'all';

// Color Mapping
const riskColors = {
    'Alto': { bg: 'bg-red-50', border: 'border-red-200', text: 'text-red-800', badge: 'bg-red-100 text-red-700', hover: 'hover:border-red-300' },
    'Medio': { bg: 'bg-amber-50', border: 'border-amber-200', text: 'text-amber-800', badge: 'bg-amber-100 text-amber-700', hover: 'hover:border-amber-300' },
    'Cibo': { bg: 'bg-emerald-50', border: 'border-emerald-200', text: 'text-emerald-800', badge: 'bg-emerald-100 text-emerald-700', hover: 'hover:border-emerald-300' }
};

// Initialize Chart
function initChart() {
    const ctx = document.getElementById('riskChart').getContext('2d');

    // Calculate stats
    const counts = {
        'Alto Rischio': interactionData.filter(d => d.risk === 'Alto').length,
        'Moderato': interactionData.filter(d => d.risk === 'Medio').length,
        'Cibo/Erbe': interactionData.filter(d => d.risk === 'Cibo').length
    };

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: Object.keys(counts),
            datasets: [{
                data: Object.values(counts),
                backgroundColor: [
                    'rgba(239, 68, 68, 0.8)', // Red
                    'rgba(245, 158, 11, 0.8)', // Amber
                    'rgba(16, 185, 129, 0.8)'  // Emerald
                ],
                borderWidth: 0,
                hoverOffset: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        font: {
                            family: "'Inter', sans-serif",
                            size: 12
                        },
                        boxWidth: 12
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return ` ${context.label}: ${context.raw} elementi`;
                        }
                    }
                }
            }
        }
    });
}

// Render Cards
function renderCards(data) {
    grid.innerHTML = '';

    if (data.length === 0) {
        noResults.classList.remove('hidden');
        return;
    } else {
        noResults.classList.add('hidden');
    }

    data.forEach(item => {
        const colors = riskColors[item.risk] || riskColors['Medio'];

        const card = document.createElement('div');
        card.className = `bg-white border rounded-xl p-5 card-hover transition-all duration-200 cursor-pointer flex flex-col justify-between ${colors.hover}`;
        card.style.borderColor = "transparent";
        card.classList.add(colors.border);

        card.innerHTML = `
            <div class="flex justify-between items-start mb-3">
                <span class="text-3xl bg-slate-50 p-2 rounded-lg">${item.icon}</span>
                <span class="px-2 py-1 rounded text-xs font-bold uppercase tracking-wide ${colors.badge}">${item.risk}</span>
            </div>
            <div>
                <h3 class="text-lg font-bold text-slate-800 mb-1">${item.name}</h3>
                <p class="text-xs text-slate-500 uppercase tracking-wide mb-2">${item.category}</p>
                <p class="text-sm text-slate-600 line-clamp-2">${item.mechanism}</p>
            </div>
            <div class="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between">
                <span class="text-xs font-semibold ${colors.text}">Clicca per dettagli &rarr;</span>
            </div>
        `;

        card.addEventListener('click', () => openModal(item));
        grid.appendChild(card);
    });
}

// Filter Logic
function filterData() {
    const query = searchInput.value.toLowerCase();

    const filtered = interactionData.filter(item => {
        const matchesSearch = item.name.toLowerCase().includes(query) ||
                              item.category.toLowerCase().includes(query) ||
                              item.mechanism.toLowerCase().includes(query);

        const matchesCategory = currentFilter === 'all' || item.risk === currentFilter;

        return matchesSearch && matchesCategory;
    });

    renderCards(filtered);
}

// Modal Logic
function openModal(item) {
    const colors = riskColors[item.risk];
    const header = document.getElementById('modalHeader');

    // Dynamic Header Color
    header.className = `p-6 text-white transition-colors duration-300 ${item.risk === 'Alto' ? 'bg-red-600' : item.risk === 'Medio' ? 'bg-amber-500' : 'bg-emerald-600'}`;

    document.getElementById('modalIcon').textContent = item.icon;
    document.getElementById('modalTitle').textContent = item.name;
    document.getElementById('modalCategory').textContent = item.category + " • Rischio " + item.risk;
    document.getElementById('modalMechanism').textContent = item.mechanism;
    document.getElementById('modalEffect').textContent = item.effect;
    document.getElementById('modalAdvice').textContent = item.advice;

    modal.classList.remove('hidden');
    modal.classList.add('flex');

    // Animation timeout
    setTimeout(() => {
        modal.classList.remove('opacity-0');
        modalContent.classList.remove('scale-95');
        modalContent.classList.add('scale-100');
    }, 10);
}

function closeModal() {
    modal.classList.add('opacity-0');
    modalContent.classList.remove('scale-100');
    modalContent.classList.add('scale-95');

    setTimeout(() => {
        modal.classList.add('hidden');
        modal.classList.remove('flex');
    }, 300);
}

// Navigation function for back to home
function goToHome() {
    window.location.href = 'index.html';
}

// Event Listeners
searchInput.addEventListener('input', filterData);

filterBtns.forEach(btn => {
    btn.addEventListener('click', (e) => {
        // Update UI classes
        filterBtns.forEach(b => {
            b.classList.remove('bg-slate-800', 'text-white');
            b.classList.add('bg-white', 'text-slate-600');
        });
        e.target.classList.remove('bg-white', 'text-slate-600');
        e.target.classList.add('bg-slate-800', 'text-white');

        // Update Logic
        currentFilter = e.target.dataset.filter;
        filterData();
    });
});

document.getElementById('closeModal').addEventListener('click', closeModal);
document.getElementById('closeModalBtn').addEventListener('click', closeModal);
modal.addEventListener('click', (e) => {
    if (e.target === modal) closeModal();
});

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    initChart();
    renderCards(interactionData);
});
