// TaoGEST Professional Guides Scripts

document.addEventListener('DOMContentLoaded', function() {
    // Aggiungi highlight ai link attivi
    highlightActiveLink();

    // Gestisci smooth scrolling
    enableSmoothScrolling();
});

function highlightActiveLink() {
    const currentPage = window.location.pathname.split('/').pop();
    const links = document.querySelectorAll('.guide-menu a');

    links.forEach(link => {
        if (link.getAttribute('href') === currentPage) {
            link.style.backgroundColor = '#e3f2fd';
            link.style.fontWeight = 'bold';
        }
    });
}

function enableSmoothScrolling() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}

// Funzione per tornare alla home
function goToHome() {
    window.location.href = 'index.html';
}
