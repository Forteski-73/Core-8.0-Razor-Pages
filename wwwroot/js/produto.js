// ======================= VARIÁVEIS GLOBAIS =======================
window.imagens = {
    DECORACAO: [],
    EMBALAGEM: [],
    PALETIZACAO: []
};

// ======================= ZOOM =======================
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".zoom-container").forEach(zoomContainer => {
        const finalidade = zoomContainer.dataset.finalidade;
        const zoomImg = document.getElementById("mainImage_" + finalidade);

        if (window.innerWidth > 768 && zoomContainer && zoomImg) {
            zoomContainer.addEventListener('mousemove', function (e) {
                const bounds = zoomContainer.getBoundingClientRect();
                const x = e.clientX - bounds.left;
                const y = e.clientY - bounds.top;
                const percentX = x / bounds.width;
                const percentY = y / bounds.height;
                const offsetX = (percentX - 0.5) * 100;
                const offsetY = (percentY - 0.5) * 100;
                zoomImg.style.transform = `scale(2) translate(${-offsetX}%, ${-offsetY}%)`;
            });

            zoomContainer.addEventListener('mouseleave', function () {
                zoomImg.style.transform = 'scale(1)';
            });
        }
    });
});

// ======================= SETAR IMAGEM PRINCIPAL =======================
function setMainImage(src, finalidade) {
    const img = document.getElementById("mainImage_" + finalidade);
    if (img) img.src = src;

    const index = window.imagens[finalidade].indexOf(src);
    if (index !== -1) {
        window["currentImageIndex_" + finalidade] = index;
    }

    document.querySelectorAll(`#thumbContainer_${finalidade} .thumbnail-img`).forEach(el => {
        el.classList.toggle("selected", el.src === src);
    });
}

// ======================= INPUT DE IMAGEM =======================
function triggerImageInput(event, finalidade) {
    event.stopPropagation();

    const cameraInputId = `imageInputCamera_${finalidade}`;
    const galleryInputId = `imageInputGallery_${finalidade}`;

    if (/Mobi|Android|iPhone/i.test(navigator.userAgent)) {
        const choice = confirm("Deseja abrir a câmera?");
        document.getElementById(choice ? cameraInputId : galleryInputId).click();
    } else {
        document.getElementById(galleryInputId).click();
    }
}

// ======================= IMAGEM ESCOLHIDA =======================
function handleImageSelected(event, finalidade) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function (e) {
        const dataUrl = e.target.result;

        if (!window.imagens[finalidade]) {
            window.imagens[finalidade] = [];
        }

        window.imagens[finalidade].push(dataUrl);
        window["currentImageIndex_" + finalidade] = window.imagens[finalidade].length - 1;

        setMainImage(dataUrl, finalidade);
        atualizarThumbnails(finalidade);
    };
    reader.readAsDataURL(file);
}

// ======================= REMOVER IMAGEM =======================
function removeCurrentImage(finalidade) {
    const imagens = window.imagens[finalidade];
    const currentIndex = window["currentImageIndex_" + finalidade];

    if (!imagens || imagens.length === 0) return;

    imagens.splice(currentIndex, 1);

    if (imagens.length === 0) {
        document.getElementById('mainImage_' + finalidade).src = "";
        document.getElementById('thumbContainer_' + finalidade).innerHTML = "";
        return;
    }

    window["currentImageIndex_" + finalidade] = 0;
    const novaPrincipal = imagens[0];

    setMainImage(novaPrincipal, finalidade);
    atualizarThumbnails(finalidade);
}

// ======================= ATUALIZAR MINIATURAS =======================
function atualizarThumbnails(finalidade) {
    const container = document.getElementById("thumbContainer_" + finalidade);
    container.innerHTML = "";

    window.imagens[finalidade].forEach((img, index) => {
        const thumb = document.createElement("img");
        thumb.src = img;
        thumb.className = "img-thumbnail thumbnail-img";
        thumb.style.cursor = "pointer";
        thumb.onclick = () => {
            setMainImage(img, finalidade);
            window["currentImageIndex_" + finalidade] = index;
        };
        container.appendChild(thumb);
    });
}

// ======================= SALVAR IMAGENS =======================
function enviarImagens(finalidade) {
    const imagens = window.imagens[finalidade];

    if (!imagens || imagens.length === 0) {
        alert(`Nenhuma imagem para salvar em ${finalidade}.`);
        return;
    }

    const formData = new FormData();

    imagens.forEach((dataUrl, index) => {
        const base64Data = dataUrl.split(',')[1];
        const mimeString = dataUrl.split(',')[0].split(':')[1].split(';')[0];
        const byteString = atob(base64Data);
        const ab = new ArrayBuffer(byteString.length);
        const ia = new Uint8Array(ab);
        for (let i = 0; i < byteString.length; i++) ia[i] = byteString.charCodeAt(i);

        const blob = new Blob([ab], { type: mimeString });
        const numero = String(index + 1).padStart(4, '0');
        const extensao = mimeString.split('/')[1] || 'jpg';
        formData.append('files', blob, `${numero}.${extensao}`);
    });

    fetch(`?handler=UploadBase64&product=${window.produtoId}&finalidade=${finalidade}`, {
        method: 'POST',
        headers: { 'RequestToken': window.token },
        body: formData
    })
        .then(async response => {
            if (!response.ok) {
                const text = await response.text();
                alert(`Erro ao salvar imagens de ${finalidade}:\n` + text);
                return;
            }

            const data = await response.json();
            if (data.success) {
                alert(`Imagens de ${finalidade} salvas com sucesso!`);
                window.location.reload();
            } else {
                alert(`Erro ao salvar imagens de ${finalidade}.`);
            }
        })
        .catch(error => {
            console.error("Erro na requisição:", error);
            alert(`Falha na comunicação com o servidor para ${finalidade}.`);
        });
}

// ======================= SCROLL MINIATURAS =======================
function scrollThumbnail(direction, finalidade) {
    const container = document.getElementById("thumbContainer_" + finalidade);
    container.scrollBy({ left: direction * 150, behavior: 'smooth' });
}

// ======================= TABS =======================
function toggleTab(header) {
    const tab = header.parentElement;
    const isExpanded = tab.classList.contains('expanded');
    if (isExpanded) {
        tab.classList.remove('expanded');
    } else {
        document.querySelectorAll('.info-tab').forEach(t => t.classList.remove('expanded'));
        tab.classList.add('expanded');
    }
}

// ======================= GET COOKIE =======================
function getCookie(name) {
    const cookies = document.cookie.split(';');
    for (const cookie of cookies) {
        const [key, value] = cookie.trim().split('=');
        if (key === name) return decodeURIComponent(value);
    }
    return null;
}
