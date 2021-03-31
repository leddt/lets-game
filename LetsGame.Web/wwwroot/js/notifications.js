initPushNotifications();

async function initPushNotifications() {
    const {pushManager} = await navigator.serviceWorker.ready;
    const subscription = await pushManager.getSubscription();
    
    const isActive = (subscription && window.activePushSubscriptions && window.activePushSubscriptions.indexOf(JSON.stringify(subscription)) > -1);
    updatePushNotificationsUI(isActive);
}

async function enablePushNotifications() {
    const subscription = await getSubscription();

    document.getElementById("AddPushSubscription").value = JSON.stringify(subscription);
    document.getElementById("RemovePushSubscription").value = "";
    updatePushNotificationsUI(true);
    
    async function getSubscription() {
        const {pushManager} = await navigator.serviceWorker.ready;
        const subscription = await pushManager.getSubscription();
        if (subscription) return subscription;
    
        const publicKey = window.vapidPublicKey;
    
        return pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(publicKey)
        })
    }
    
    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/\-/g, '+')
            .replace(/_/g, '/');
    
        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);
    
        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
    
        return outputArray;
    }
}

async function disablePushNotifications() {
    const {pushManager} = await navigator.serviceWorker.ready;
    const subscription = await pushManager.getSubscription();
    
    document.getElementById("AddPushSubscription").value = "";
    document.getElementById("RemovePushSubscription").value = JSON.stringify(subscription);
    
    updatePushNotificationsUI(false);
}

async function testPushNotifications() {
    const {pushManager} = await navigator.serviceWorker.ready;
    const subscription = await pushManager.getSubscription();
    
    await fetch("/api/push/test", {
        method: "post",
        headers: {
            "Content-type": "application/json"
        },
        body: JSON.stringify({
            subscription: JSON.stringify(subscription)
        })
    });
}

function updatePushNotificationsUI(active) {
    if (active) {
        document.querySelectorAll("[data-push=enable]").forEach(x => x.removeAttribute("disabled"));
        document.querySelectorAll("[data-push=show]").forEach(x => x.style.display = '');
        document.querySelectorAll("[data-push=hide]").forEach(x => x.style.display = 'none');
    } else {
        document.querySelectorAll("[data-push=enable]").forEach(x => x.setAttribute("disabled", ""));
        document.querySelectorAll("[data-push=show]").forEach(x => x.style.display = 'none');
        document.querySelectorAll("[data-push=hide]").forEach(x => x.style.display = '');
    }
}