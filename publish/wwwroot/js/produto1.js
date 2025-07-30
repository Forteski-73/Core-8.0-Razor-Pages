// ======================= VARIÁVEIS GLOBAIS =======================
window.imagens = {
    PRODUTO: [],
    EMBALAGEM: [],
    PALETIZACAO: []
};

// =========================== ZOOM ===========================
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
        el.classList.toggle("selected", el.src === src || el.getAttribute("src") === src);
    });
}

// ======================= IMAGEM ESCOLHIDA =======================
function handleImageSelected(event, finalidade) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function (e) {
        const dataUrl = e.target.result;

        if (!window.imagens[finalidade]) window.imagens[finalidade] = [];

        window.imagens[finalidade].push(dataUrl);
        const novoIndex = window.imagens[finalidade].length - 1;
        window["currentImageIndex_" + finalidade] = novoIndex;
        setMainImage(dataUrl, finalidade);
        atualizarThumbnails(finalidade);
        enviarImagens(finalidade);
    };
    reader.readAsDataURL(file);
}

// ======================= INPUT DE IMAGEM =======================
function triggerImageInput(finalidade) {
    const cameraInputId = `imageInputCamera_${finalidade}`;
    const galleryInputId = `imageInputGallery_${finalidade}`;

    if (/Mobi|Android|iPhone/i.test(navigator.userAgent)) {
        const choice = confirm("Deseja abrir a câmera?");
        document.getElementById(choice ? cameraInputId : galleryInputId).click();
    } else {
        document.getElementById(galleryInputId).click();
    }
}

// ======================= REMOVER IMAGEM =======================
function removeCurrentImage(finalidade) {
    const imagens = window.imagens[finalidade];
    const mainImgElement = document.getElementById('mainImage_' + finalidade);
    const currentIndex = window["currentImageIndex_" + finalidade];

    if (!imagens || imagens.length === 0 || currentIndex === -1) return;

    imagens.splice(currentIndex, 1);

    if (imagens.length === 0) {
        mainImgElement.src = "";
        document.getElementById('thumbContainer_' + finalidade).innerHTML = "";
        window["currentImageIndex_" + finalidade] = -1;
        enviarImagens(finalidade);
        return;
    }

    const novoIndex = Math.min(currentIndex, imagens.length - 1);
    const novaPrincipal = imagens[novoIndex];
    window["currentImageIndex_" + finalidade] = novoIndex;
    setMainImage(novaPrincipal, finalidade);
    atualizarThumbnails(finalidade);
    enviarImagens(finalidade);
}

// ======================= ATUALIZAR MINIATURAS =======================
function atualizarThumbnails(finalidade) {
    const container = document.getElementById("thumbContainer_" + finalidade);
    container.innerHTML = "";

    window.imagens[finalidade].forEach((img, index) => {
        const thumb = document.createElement("img");
        thumb.src = img;
        thumb.dataset.url = img;
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
        alert("Nenhuma imagem para salvar.");
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
        const nomeArquivo = `${numero}.${extensao}`;
        formData.append('files', blob, nomeArquivo);
    });

    const produtoId = window.produtoId;
    fetch(`?handler=UploadBase64&product=${produtoId}&finalidade=${finalidade}`, {
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
            if (!data.success) {
                alert("Erro ao salvar imagens.");
            }
        })
        .catch(error => {
            console.error("Erro na requisição:", error);
            alert("Falha na comunicação com o servidor.");
        });
}

// ======================= SCROLL MINIATURAS =======================
function scrollThumbnail(direction, finalidade) {
    const container = document.getElementById("thumbContainer_" + finalidade);
    container.scrollBy({ left: direction * 150, behavior: 'smooth' });
}

// ======================= TABS =======================
function toggleTab(header, idTab) {
    const tab = document.getElementById(idTab);
    tab.classList.toggle('expanded');
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

// ======================= INICIALIZAÇÃO SORTABLE E CLIQUES =======================
const tiposImagem = ['PRODUTO', 'EMBALAGEM', 'PALETIZACAO'];
tiposImagem.forEach(tipo => {
    window.imagens[tipo] = window.imagens[tipo] || [];
    const container = document.getElementById(`thumbContainer_${tipo}`);
    if (!container) return;

    new Sortable(container, {
        animation: 150,
        ghostClass: 'sortable-ghost',
        onEnd: function () {
            const newOrder = Array.from(container.querySelectorAll('img')).map(img => img.src).filter(Boolean);
            window.imagens[tipo] = newOrder;

            const atualSrc = document.getElementById("mainImage_" + tipo)?.src;
            const novoIndex = newOrder.indexOf(atualSrc);
            window["currentImageIndex_" + tipo] = novoIndex;

            atualizarThumbnails(tipo);
            enviarImagens(tipo);
        }
    });

    container.querySelectorAll('.thumbnail-img').forEach(img => {
        img.addEventListener('click', function (e) {
            e.stopPropagation();
            const src = this.getAttribute('src');
            setMainImage(src, tipo);
        });
    });
});


// ======================= ATIVA/DESATIVA DRAG AND DROP  =======================

let sortableInstances = {};
let reordenacaoAtiva = {}; // controle por finalidade

function toggleReordenacao(finalidade) {
    const btn = document.getElementById("toggleSort_" + finalidade);

    if (!reordenacaoAtiva[finalidade]) {
        ativarSortable(finalidade);
        reordenacaoAtiva[finalidade] = true;
        btn.classList.remove("btn-outline-secondary");
        btn.classList.add("btn-success");
        btn.innerHTML = "✅ Reordenando...";
    } else {
        desativarSortable(finalidade);
        reordenacaoAtiva[finalidade] = false;
        btn.classList.remove("btn-success");
        btn.classList.add("btn-outline-secondary");
        btn.innerHTML = "🔒 Reordenar";
    }
}

function ativarSortable(finalidade) {
    const container = document.getElementById("thumbContainer_" + finalidade);

    if (!sortableInstances[finalidade]) {
        sortableInstances[finalidade] = new Sortable(container, {
            animation: 150,
            onEnd: function (evt) {
                const oldIndex = evt.oldIndex;
                const newIndex = evt.newIndex;
                const imagens = window.imagens[finalidade];

                if (!imagens || imagens.length === 0) return;

                const movedItem = imagens.splice(oldIndex, 1)[0];
                imagens.splice(newIndex, 0, movedItem);

                window["currentImageIndex_" + finalidade] = newIndex;
                setMainImage(imagens[newIndex], finalidade);
                enviarImagens(finalidade);
            }
        });
    } else {
        sortableInstances[finalidade].option("disabled", false);
    }
}

function desativarSortable(finalidade) {
    if (sortableInstances[finalidade]) {
        sortableInstances[finalidade].option("disabled", true);
    }
}
// ======================= ATIVA/DESATIVA DRAG AND DROP  =======================