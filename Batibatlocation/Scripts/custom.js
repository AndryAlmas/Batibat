// JavaScript code for form submission and interaction
// document.getElementById('contactForm').addEventListener('submit', function(event) {
//     event.preventDefault();
    
//     // Esempio: logica per invio del form (puoi aggiungere una chiamata AJAX qui)
//     alert('Grazie per averci contattato! Ti risponderemo presto.');
    
//     // Resetta i campi del form
//     this.reset();
// });

// Variabile per tracciare l'ultima posizione di scorrimento
let lastScrollTop = 0;
const navbar = document.getElementById('navbar');
const topbar = document.getElementById('topbar');
const sideMenu = document.getElementById('sideMenu');
const menuToggle = document.getElementById('menuToggle');
const closeMenu = document.getElementById('closeMenu');
const heroSection = document.getElementById('hero'); // Assicurati che l'ID sia corretto

// Aggiungi un listener per l'evento scroll
window.addEventListener('scroll', function () {
    let scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    let marginIOS = 25;
    const heroHeight = heroSection.offsetHeight; // Ottieni l'altezza della sezione hero
    const topbarHeight = topbar.offsetHeight; // Ottieni l'altezza della sezione hero
        if (scrollTop > lastScrollTop && scrollTop > marginIOS) {
            // Se l'utente scorre verso il basso, la top-bar scompare
            topbar.style.marginTop = "-" + topbarHeight + "px";
            topbar.classList.add('topbar-hidden');
        } else {
            // Se l'utente scorre verso l'alto, la navbar riappare
            if (heroHeight >= (scrollTop+heroHeight-marginIOS)) { // Solo se si è in cima al sito
                topbar.style.marginTop = "0px";
                topbar.classList.remove('topbar-hidden');
                //console.log(heroHeight, scrollTop,heroHeight,marginIOS, scrollTop+heroHeight-marginIOS)
            }
        }    
    // Aggiorna l'ultima posizione di scorrimento
    lastScrollTop = scrollTop;
});

window.addEventListener('load', function() {
    // Aspetta un istante prima di forzare lo scroll in cima
    setTimeout(function() {
        window.scrollTo(window.pageYOffset || document.documentElement.scrollTop, 0);
    }, 25); // Piccolo ritardo di 25 millisecondi
});


// Mostra il menu laterale
menuToggle.addEventListener('click', function() {
    sideMenu.classList.add('active');
});

// Nascondi il menu laterale
closeMenu.addEventListener('click', function() {
    sideMenu.classList.remove('active');
});

// Aggiungi un listener di eventi ai link nel menu laterale
const sideMenuLinks = document.querySelectorAll('.side-menu-nav a');

sideMenuLinks.forEach(link => {
    link.addEventListener('click', function() {
        sideMenu.classList.remove('active'); // Nascondi il menu quando un link viene cliccato
    });
});

document.addEventListener("DOMContentLoaded", function() {

    function setHeroHeight() {
        // const topbarHeight = topbar.offsetHeight;
        const navbarHeight = navbar.offsetHeight; // Ottiene l'altezza della navbar
        heroSection.style.height = `calc(100vh - ${(navbarHeight)}px)`; // Imposta l'altezza della hero section
        heroSection.style.marginTop = navbarHeight + "px";
    }

    // Imposta l'altezza iniziale
    setHeroHeight();
    window.addEventListener("resize", setHeroHeight);
});




// Pulsante "To Top"
const toTopBtn = document.getElementById('toTopBtn');

// Mostra il pulsante "To Top" quando si scorre giù oltre la sezione hero
window.addEventListener('scroll', function() {
    if (window.scrollY >= window.innerHeight) { // Quando si scorre oltre l'altezza della finestra
        toTopBtn.classList.remove('to-top-hidden'); // Rimuove la classe per disabilitare il pulsante
        toTopBtn.classList.add('show'); // Aggiungi la classe "show" per farlo apparire
    } else {
        toTopBtn.classList.remove('show'); // Rimuovi la classe "show" per farlo scomparire
        toTopBtn.classList.add('to-top-hidden'); // Aggiungi la classe per disabilitare il pulsante
    }
});

// Scorri verso l'alto quando il pulsante viene cliccato
toTopBtn.addEventListener('click', function() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth' // Scorrimento fluido
    });
});


// Pulsante "Home"
const toHomeBtn = document.getElementById('homeBtn');
// Scorri verso l'alto quando il pulsante viene cliccato
toHomeBtn.addEventListener('click', function() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth' // Scorrimento fluido
    });
});

// Pulsante "Home 2"
const toHomeBtn2 = document.getElementById('homeBtn2');
// Scorri verso l'alto quando il pulsante viene cliccato
toHomeBtn2.addEventListener('click', function() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth' // Scorrimento fluido
    });
});


document.querySelector('.email-button').addEventListener('click', function() {
    window.location.href = 'mailto:batibatlocation@gmail.com';
});