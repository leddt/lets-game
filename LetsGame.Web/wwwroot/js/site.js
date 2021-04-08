// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

if (navigator.serviceWorker) {
    navigator.serviceWorker.register("/service-worker.js")
}

$(() => initUi(document));

function copyToClipboard(text) {
    const elem = document.createElement('textarea');
    elem.value = text;
    document.body.appendChild(elem);
    elem.select();
    document.execCommand('copy');
    document.body.removeChild(elem);
}

// Update Panels
let ignoreUpdates = null;
document.addEventListener("submit", async (ev) => {
    if (!ev.target.matches("form[data-update]")) return;
    
    const containerId = ev.target.dataset.update;
    const container = document.getElementById(containerId);
    if (!container) return;
    
    ev.preventDefault();
        
    ignoreUpdates = containerId;
    setTimeout(() => ignoreUpdates = false, 500)
    
    const response = await fetch(ev.target.action, {
        credentials: "same-origin",
        method: ev.target.method,
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: serializeForm(ev.target, ev.submitter)
    });
    
    const resultHtml = await response.text();
    replaceElement(resultHtml, container);
});

async function refreshPanel(containerId) {
    if (ignoreUpdates === containerId) return;
    
    const container = document.getElementById(containerId);
    if (!container) return;

    const response = await fetch(window.location.href, {
        credentials: "same-origin",
        method: "GET"
    });

    const resultHtml = await response.text();
    replaceElement(resultHtml, container);
}

function replaceElement(resultHtml, container) {
    const resultElement = getElementByIdFromHtml(container.id, resultHtml);

    if (!resultElement) {
        window.location.reload();
        return;
    }

    cleanupUi(container);
    initUi(resultElement);

    $(container).replaceWith(resultElement);
}

function initUi(container) {
    $(container).find("[data-tooltip]").tooltip();
}

function cleanupUi(container) {
    $(container).find("[data-tooltip]").tooltip("hide");
}

function getElementByIdFromHtml(elementId, html) {
    const tempContainer = document.createElement("div")
    tempContainer.innerHTML = html;
    return tempContainer.querySelector(`#${elementId}`);
}

function serializeForm(form, submitter) {
    const formData = new FormData(form);
    if (submitter && submitter.name) {
        formData.append(submitter.name, submitter.value)
    }
    
    return new URLSearchParams(Array.from(formData)).toString();
}