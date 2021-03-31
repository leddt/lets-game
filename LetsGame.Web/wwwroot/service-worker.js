self.addEventListener("push", function(event) {
   const payload = event.data ? event.data.json() : null;
   
   event.waitUntil(
       self.registration.showNotification(payload.title, payload)
   );
});