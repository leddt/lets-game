(async function () {
    let presences = [];
    
    const {currentGroup} = window;
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/grouphub")
        .withAutomaticReconnect()
        .build();

    connection.on("here", function(userId) {
        addPresence(userId);
    });
    connection.on("hereToo", function(userIds) {
        for (const userId of userIds) {
            addPresence(userId);
        }
    });
    connection.on("left", function(userId) {
        removePresence(userId);
    });
    connection.on("update", function(elementId) {
        refreshPanel(elementId);
    });
    
    connection.onreconnecting(() => {
        clearPresences();
    });
    connection.onreconnected(async () => {
        await onConnected();
    });
    
    await connection.start();
    await onConnected();
    
    document.addEventListener("panelupdate", function(ev) {
        if (ev.detail?.containerId !== "members") return;
        
        for (const userId of presences) {
            $(`[data-presence-id='${userId}']`).addClass("active");
        }
    });
    
    function addPresence(userId) {
        if (presences.indexOf(userId) >= 0) return;
        presences.push(userId);
        
        $(`[data-presence-id='${userId}']`).addClass("active");
    }
    function removePresence(userId) {
        presences = presences.filter(x => x !== userId);
        
        $(`[data-presence-id='${userId}']`).removeClass("active");
    }
    function clearPresences() {
        presences = [];
        $("[data-presence-id]").removeClass("active")
    }
    
    async function onConnected() {
        await connection.invoke("join", currentGroup);
        addPresence(window.ownUserId);
    }
})();