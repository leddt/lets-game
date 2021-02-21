﻿(async function () {
    let presences = [];
    
    const {currentGroup} = window;
    const connection = new signalR.HubConnectionBuilder().withUrl("/grouphub").build();

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
    
    await connection.start();
    await connection.invoke("join", currentGroup);

    addPresence(window.ownUserId);
    
    function addPresence(userId) {
        if (presences.indexOf(userId) >= 0) return;
        presences.push(userId);
        
        $(`[data-presence-id='${userId}']`).addClass("active");
    }
    function removePresence(userId) {
        presences = presences.filter(x => x !== userId);
        
        $(`[data-presence-id='${userId}']`).removeClass("active");
    }
})();