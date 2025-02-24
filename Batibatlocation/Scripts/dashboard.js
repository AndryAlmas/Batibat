document.addEventListener("DOMContentLoaded", function () {
    document.querySelector('.dashboard-container').classList.add('loaded');

    var leftSection = document.querySelector('.left-section');
    var rightSection = document.querySelector('.right-section');
    var closeIcon = document.getElementById('closeIcon');
    var hamburgerIconRight = document.getElementById('hamburgerIconRight');

    // Gestisci il click sulla X nella sezione sinistra
    closeIcon.addEventListener('click', function () {
        leftSection.classList.add('hidden');
        rightSection.classList.add('full-width');
        closeIcon.classList.remove('visible');
        closeIcon.classList.add('hidden');
        hamburgerIconRight.classList.add('visible');
    });

    // Gestisci il click sull'icona hamburger nella sezione destra
    hamburgerIconRight.addEventListener('click', function () {
        leftSection.classList.remove('hidden');
        rightSection.classList.remove('full-width');
        closeIcon.classList.remove('hidden');
        closeIcon.classList.add('visible');
        hamburgerIconRight.classList.remove('visible');
    });
});