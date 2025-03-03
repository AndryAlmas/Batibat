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

function updateImagePreview() {
    var input = document.getElementById('imageFile');
    var preview = document.getElementById('imagePreview');
    var label = document.querySelector('label[for="imageFile"]');

    // Controlla se un file è stato selezionato
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        // Funzione di callback quando il file è caricato
        reader.onload = function (e) {
            // Aggiorna l'anteprima dell'immagine
            preview.src = e.target.result;
            // Aggiorna il testo dell'etichetta con il nome del file
            label.textContent = input.files[0].name;
        }

        // Leggi il file come URL di dati
        reader.readAsDataURL(input.files[0]);
    } else {
        // Ripristina l'anteprima e l'etichetta se non è stato selezionato alcun file
        preview.src = '@Model.ImageUrl'; // Ripristina l'anteprima originale
        label.textContent = "Choisir une image";
    }
}

function removeImage(src) {
    // Rimuovi l'immagine e resetta il campo input
    var preview = document.getElementById('imagePreview');
    var input = document.getElementById('imageFile');
    var label = document.querySelector('label[for="imageFile"]');

    // Reset dell'anteprima dell'immagine
    preview.src = src; // Ripristina l'anteprima originale
    label.textContent = "Choisir une image"; // Ripristina il testo dell'etichetta

    // Reset del campo input file
    input.value = ''; // Resetta il valore del file
}

function updateQuadrantImage(index) {
    var input = document.getElementById('fileInput-' + index);
    var preview = document.getElementById('preview-' + index);
    var quadrant = document.getElementById('quadrant-' + index);
    var removeBtn = document.getElementById('remove-' + index);
    var suprimeBtn = document.getElementById('btnSuprimeAutreImg-' + index);

    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            preview.src = e.target.result; // Aggiorna l'anteprima dell'immagine nel quadrante
            preview.classList.remove('image-removed'); // Rimuove la classe che mostra l'immagine
            preview.classList.add('image-filled'); // Aggiungi classe per indicare che c'è un'immagine
            removeBtn.style.display = "block"; // Mostra il pulsante di rimozione
            suprimeBtn.classList.add('hideBtnSuprime');

        }

        reader.readAsDataURL(input.files[0]);
    }
}

function removeQuadrantImage(src,index) {
/*    var quadrant = document.getElementById('quadrant-' + index);*/
    var preview = document.getElementById('preview-' + index);
    var input = document.getElementById('fileInput-' + index);
    var removeBtn = document.getElementById('remove-' + index);
    var suprimeBtn = document.getElementById('btnSuprimeAutreImg-' + index);


    // Reset dell'anteprima dell'immagine
    preview.src = src; // Ripristina l'anteprima originale
    //quadrant.classList.remove('image-removed'); // Rimuove la classe che mostra l'immagine
    //quadrant.classList.add('image-filled'); // Aggiungi classe per indicare che c'è un'immagine
    if (suprimeBtn == undefined) {
        preview.classList.remove('image-filled'); // 
        preview.classList.add('image-removed'); // nasconde l'immagine
    }


    removeBtn.style.display = "none"; // Nasconde il pulsante di rimozione
    suprimeBtn.classList.remove('hideBtnSuprime'); // mostra il bottone


    // Reset del campo input file
    input.value = ''; // Resetta il valore del file
}

function deleteImage(event, imagePath, index) {
    event.preventDefault(); // Previene il refresh della pagina
    if (!imagePath) {
        alert("Aucune image à supprimer.");
        return;
    }

    if (!confirm("Êtes-vous sûr de vouloir supprimer cette image ?")) {
        return;
    }

    fetch('/Admin/DeleteImage', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ imagePath: imagePath })
    })
        .then(response => response.json())
        .then(data => {
            console.log("Response:", data); // Debug console
            if (data.success) {
                alert("Image supprimée avec succès!");

                // Reset quadrante
                document.getElementById('preview-' + index).src = "";
                document.getElementById('preview-' + index).classList.remove('image-filled');
                document.getElementById('preview-' + index).classList.add('image-removed');
                document.getElementById('remove-' + index).style.display = "none";
                var btn = document.getElementById('btnSuprimeAutreImg-' + index);
                btn.parentNode.removeChild(btn);
                
            } else {
                alert("Erreur lors de la suppression de l'image: " + (data.message || ''));
            }
        })
        .catch(error => {
            console.error("Erreur AJAX:", error);
            alert("Une erreur est survenue lors de la suppression de l'image.");
        });

    return false;
}

function toggleVisibility(icon) {
    var id = icon.getAttribute("data-id");
    var isVisible = icon.getAttribute("data-visible") === "True"; // Converti in boolean

    $.ajax({
        url: "/Admin/ToggleVisibility", // Cambia con la tua URL corretta
        type: "POST",
        data: { id: id },
        success: function (response) {
            if (response.success) {
                // Cambia l'icona visibile/non visibile
                if (isVisible) {
                    icon.classList.remove("fa-eye");
                    icon.classList.add("fa-eye-slash");
                } else {
                    icon.classList.remove("fa-eye-slash");
                    icon.classList.add("fa-eye");
                }
                // Aggiorna il valore data-visible
                icon.setAttribute("data-visible", isVisible ? "False" : "True");
            } else {
                alert("Errore durante l'aggiornamento.");
            }
        },
        error: function () {
            alert("Errore di connessione con il server.");
        }
    });
}

