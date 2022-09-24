import {HubConnectionBuilder} from "@microsoft/signalr";
import { onDestroy } from "svelte";
import { writable } from "svelte/store";

export function usePresence(group) {
  const connection = new HubConnectionBuilder()
    .withUrl("/presences")
    .withAutomaticReconnect()
    .build();

  const presences = writable([]);
  connection.on("UpdatePresences", (forGroup, userIds) => {
    if (forGroup !== group) return;
    presences.set(userIds);
  });

  (async function() {
    await connection.start();
    await connection.invoke("Join", group)
  })();

  onDestroy(() => connection.stop());

  return presences;
}