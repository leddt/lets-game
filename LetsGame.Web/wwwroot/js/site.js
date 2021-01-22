// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(() => initUi(document));

function initUi(container) {
    $(container).find("[data-tooltip]").tooltip();
}
function cleanupUi(container) {
    $(container).find("[data-tooltip]").tooltip("hide");
}

function copyToClipboard(text) {
    const elem = document.createElement('textarea');
    elem.value = text;
    document.body.appendChild(elem);
    elem.select();
    document.execCommand('copy');
    document.body.removeChild(elem);
}

// Update Panels
document.addEventListener("submit", async (ev) => {
    if (!ev.target.matches("form[data-update]")) return;
    
    const containerId = ev.target.dataset.update;
    const container = document.getElementById(containerId);
    if (!container) return;
    
    ev.preventDefault();
    
    new FormData(ev.target)
    
    const response = await fetch(ev.target.action, {
        credentials: "same-origin",
        method: ev.target.method,
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: serializeForm(ev.target)
    });
    
    const resultHtml = await response.text();
    const resultElement = getElementByIdFromHtml(containerId, resultHtml);

    if (!resultElement) {
        window.location.reload();
        return;
    }

    cleanupUi(container);
    initUi(resultElement);
    
    $(container).replaceWith(resultElement);
    
    function serializeForm(form) {
        return new URLSearchParams(Array.from(new FormData(form))).toString();
    }

    function getElementByIdFromHtml(elementId, html) {
        const tempContainer = document.createElement("div")
        tempContainer.innerHTML = html;
        return tempContainer.querySelector(`#${elementId}`);
    }
});