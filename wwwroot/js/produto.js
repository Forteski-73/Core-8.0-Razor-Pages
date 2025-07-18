// Variáveis globais
let currentImageIndex = 0;

/* ======================= ZOOM ======================= */
document.addEventListener("DOMContentLoaded", function () {
    const zoomContainer = document.querySelector('.zoom-container');
    const zoomImg = document.getElementById('mainImage');

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

    if (window.imagens && window.imagens.length > 0) {
        currentImageIndex = window.imagens.findIndex(i =>
            document.getElementById('mainImage').src.includes(i)
        );
        atualizarThumbnails();
        filtrarPorFinalidade('Embalagem');
    }
});

/* ======================= TROCA IMAGEM ======================= */
/*
function setMainImage(url) {
    const zoomImg = document.getElementById('mainImage');
    zoomImg.src = url;
    document.querySelectorAll('.thumbnail-img').forEach(img => {
        img.classList.remove("selected");
        if (img.src === url || img.src.includes(url)) {
            img.classList.add("selected");
        }
    });
    currentImageIndex = window.imagens.findIndex(i => i === url);
}
*/
function setMainImage(src, finalidade) {
    document.getElementById("mainImage_" + finalidade).src = src;

    const index = window.imagens[finalidade].indexOf(src);
    if (index !== -1) {
        window["currentImageIndex_" + finalidade] = index;
    }
}

/* ======================= SCROLL MINIATURAS ======================= */
function scrollThumbnail(direction) {
    const container = document.getElementById("thumbContainer");
    container.scrollBy({ left: direction * 150, behavior: 'smooth' });
}

/* ======================= TABS ======================= */
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

/* ======================= INPUT DE IMAGEM ======================= */
/*
function triggerImageInput(event) {
    event.stopPropagation();

    if (/Mobi|Android|iPhone/i.test(navigator.userAgent)) {
        const choice = confirm("Deseja abrir a câmera?");
        if (choice) {
            document.getElementById('imageInput_Decoration').click();
        } else {
            document.getElementById('imageInput_Decoration').click();
        }
    } else {
        document.getElementById('imageInputGallery_Decoration').click();
    }
}
*/
function triggerImageInput(event, finalidade) {
    event.stopPropagation();

    const cameraInputId = `imageInputCamera_${finalidade}`;
    const galleryInputId = `imageInputGallery_${finalidade}`;

    if (/Mobi|Android|iPhone/i.test(navigator.userAgent)) {
        const choice = confirm("Deseja abrir a câmera?");
        if (choice) {
            document.getElementById(cameraInputId).click();
        } else {
            document.getElementById(galleryInputId).click();
        }
    } else {
        document.getElementById(galleryInputId).click();
    }
}

/* ======================= IMAGEM ESCOLHIDA ======================= */
/*
function handleImageSelected(event) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function (e) {
        const dataUrl = e.target.result;
        window.imagens.push(dataUrl);
        setMainImage(dataUrl);
        atualizarThumbnails();
        currentImageIndex = window.imagens.length - 1;
    };
    reader.readAsDataURL(file);
}
*/
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
        setMainImage(dataUrl, finalidade);
        atualizarThumbnails(finalidade);
        window["currentImageIndex_" + finalidade] = window.imagens[finalidade].length - 1;
    };
    reader.readAsDataURL(file);
}

/* ======================= REMOVER IMAGEM ======================= */
/*
function removeCurrentImage() {
    if (window.imagens.length === 0) return;

    window.imagens.splice(currentImageIndex, 1);

    if (window.imagens.length === 0) {
        document.getElementById('mainImage').src = "";
        document.getElementById('thumbContainer').innerHTML = "";
        return;
    }

    const novaPrincipal = window.imagens[0];
    setMainImage(novaPrincipal);
    atualizarThumbnails();
}
*/
function removeCurrentImage(finalidade) {
    const imagens = window.imagens[finalidade];
    const currentIndexKey = "currentImageIndex_" + finalidade;
    const currentIndex = window[currentIndexKey];

    if (!imagens || imagens.length === 0) return;

    imagens.splice(currentIndex, 1);

    // Se não há mais imagens, limpa tela
    if (imagens.length === 0) {
        document.getElementById('mainImage_' + finalidade).src = "";
        document.getElementById('thumbContainer_' + finalidade).innerHTML = "";
        return;
    }

    // Define nova imagem principal
    const novaPrincipal = imagens[0];
    window[currentIndexKey] = 0;

    setMainImage(novaPrincipal, finalidade);
    atualizarThumbnails(finalidade);
}

/* ======================= ATUALIZA MINIATURAS ======================= */
/*
function atualizarThumbnails() {
    const container = document.getElementById("thumbContainer");
    container.innerHTML = "";

    window.imagens.forEach(img => {
        const thumb = document.createElement("img");
        thumb.src = img;
        thumb.className = "img-thumbnail thumbnail-img";
        thumb.style.cursor = "pointer";
        thumb.onclick = () => setMainImage(img);
        container.appendChild(thumb);
    });
}
*/
function atualizarThumbnails() {
    const container = document.getElementById("thumbContainer");
    container.innerHTML = "";

    window.imagens.forEach(img => {
        const thumb = document.createElement("img");
        thumb.src = img;
        thumb.className = "img-thumbnail thumbnail-img";
        thumb.style.cursor = "pointer";
        thumb.onclick = () => setMainImage(img);
        container.appendChild(thumb);
    });
}

/* ======================= FILTRO FINALIDADE ======================= */
function filtrarPorFinalidade(finalidade) {
    const thumbs = document.querySelectorAll('#thumbContainer img');
    let found = false;

    thumbs.forEach(img => {
        if (img.dataset.finalidade === finalidade) {
            img.style.display = 'inline-block';
            if (!found) {
                setMainImage(img.src);
                found = true;
            }
        } else {
            img.style.display = 'none';
        }
    });

    document.getElementById("btnEmbalagem").classList.toggle("btn-dark", finalidade === "Embalagem");
    document.getElementById("btnEmbalagem").classList.toggle("btn-light", finalidade !== "Embalagem");
    document.getElementById("btnPaletizacao").classList.toggle("btn-dark", finalidade === "Inventário");
    document.getElementById("btnPaletizacao").classList.toggle("btn-light", finalidade !== "Inventário");
}

/* ======================= SALVAR IMAGENS ======================= */
/*
function enviarImagens() {
    if (!window.imagens || window.imagens.length === 0) {
        alert("Nenhuma imagem para salvar.");
        return;
    }

    const formData = new FormData();

    window.imagens.forEach((dataUrl, index) => {
        const base64Data = dataUrl.split(',')[1];
        const mimeString = dataUrl.split(',')[0].split(':')[1].split(';')[0];
        const byteString = atob(base64Data);
        const ab = new ArrayBuffer(byteString.length);
        const ia = new Uint8Array(ab);

        for (let i = 0; i < byteString.length; i++) {
            ia[i] = byteString.charCodeAt(i);
        }

        const blob = new Blob([ab], { type: mimeString });
        const numero = String(index + 1).padStart(4, '0');
        const extensao = mimeString.split('/')[1] || 'jpg';
        formData.append('files', blob, `${numero}.${extensao}`);
    });

    fetch(`?handler=UploadBase64&product=${window.produtoId}`, {
        method: 'POST',
        headers: {
            'RequestToken': window.token
        },
        body: formData
    })
        .then(async response => {
            if (!response.ok) {
                const text = await response.text();
                alert("Erro ao salvar imagens:\n" + text);
                return;
            }

            const data = await response.json();
            if (data.success) {
                alert("Imagens salvas com sucesso!");
                window.location.reload();
            } else {
                alert("Erro ao salvar imagens.");
            }
        })
        .catch(error => {
            console.error("Erro na requisição:", error);
            alert("Falha na comunicação com o servidor.");
        });
}
*/

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

        for (let i = 0; i < byteString.length; i++) {
            ia[i] = byteString.charCodeAt(i);
        }

        const blob = new Blob([ab], { type: mimeString });
        const numero = String(index + 1).padStart(4, '0');
        const extensao = mimeString.split('/')[1] || 'jpg';
        formData.append('files', blob, `${numero}.${extensao}`);
    });

    fetch(`?handler=UploadBase64&product=${window.produtoId}&finalidade=${finalidade}`, {
        method: 'POST',
        headers: {
            'RequestToken': window.token
        },
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

/* ======================= TOKEN ======================= */
function getCookie(name) {
    const cookies = document.cookie.split(';');
    for (const cookie of cookies) {
        const [key, value] = cookie.trim().split('=');
        if (key === name) return decodeURIComponent(value);
    }
    return null;
}
