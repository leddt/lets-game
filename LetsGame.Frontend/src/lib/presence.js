import {HubConnectionBuilder} from "@microsoft/signalr";
import { onDestroy } from "svelte";
import { writable } from "svelte/store";

const connection = new HubConnectionBuilder()
  .withUrl("/presences")
  .withAutomaticReconnect()
  .build();

const startPromise = connection.start();

const presentIn = [];

export function usePresence(group) {
  if (presentIn.includes(group)) return;

  const presences = writable([]);
  connection.on("UpdatePresences", (forGroup, userIds) => {
    if (forGroup !== group) return;
    presences.set(userIds);
  });

  (async function() {
    await startPromise;
    await connection.invoke("Join", group)
    presentIn.push(group);
  })();

  onDestroy(async () => {
    await connection.invoke("Leave", group)
    presentIn.splice(presentIn.indexOf(group), 1);
  });

  return presences;
}