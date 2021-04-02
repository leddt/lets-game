self.addEventListener("push", function (event) {
    const payload = event.data ? event.data.json() : null;
    if (!payload) return;

    switch (payload.type) {
        case "simple":
            const {title, body, image, icon} = payload;
            event.waitUntil(
                self.registration.showNotification(title, {
                    body,
                    image,
                    icon,
                    data: payload
                })
            );
            break;
    }
});

self.addEventListener("notificationclick", function (event) {
    event.notification.close();

    const payload = event.notification.data;
    if (payload && payload.url) {
        event.waitUntil(
            self.clients.openWindow(payload.url)
        );
    }
})