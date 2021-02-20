(async function () {
    let presences = [];
    
    addPresence(window.ownUserId);
    
    const {currentGroup} = window;
    const connection = new signalR.HubConnectionBuilder().withUrl("/grouphub").build();

    connection.on("here", function(userId) {
        addPresence(userId);
        connection.invoke("hereReply", userId);
    });
    connection.on("hereToo", function(userId) {
        addPresence(userId);
    });
    connection.on("left", function(userId) {
        removePresence(userId);
    });
    
    await connection.start();
    await connection.invoke("join", currentGroup);
    
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