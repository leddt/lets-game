<script context="module">
  import gql from "graphql-tag";

  export const sidebarAvailabilityFragment = gql`
    fragment sidebarAvailability on GroupGraphType {
      id
      self {
        id
        availableUntil
      }
    }
  `;
</script>

<script>
  import { fade } from "svelte/transition";
  
  import Button from "@/components/ui/button.svelte";
  import client from "@/lib/apollo";
  import { time } from "@/lib/date-helpers";

  export let group;

  $: isAvailable = !!group?.self.availableUntil;

  function setAvailable(seconds) {
    return client.mutate({
      mutation: gql`
        ${sidebarAvailabilityFragment}
        mutation SetAvailable($groupId: ID!, $seconds: Int!) {
          setAvailable(groupId: $groupId, lengthInSeconds: $seconds) {
            group {
              ...sidebarAvailability
            }
          }
        }
      `,
      variables: {
        groupId: group.id,
        seconds
      }
    });
  }

  function setUnavailable() {
    return client.mutate({
      mutation: gql`
        ${sidebarAvailabilityFragment}
        mutation SetUnavailable($groupId: ID!) {
          setUnavailable(groupId: $groupId) {
            group {
              ...sidebarAvailability
            }
          }
        }
      `,
      variables: {
        groupId: group.id
      }
    });
  }
</script>

<div class="grid grid-cols-1">
  {#if isAvailable}
    <div style="grid-area: 1/1/1/1;" out:fade in:fade|local={{ delay: 400 }}>
      <p class="mb-2">You are available until {time(group.self.availableUntil)}</p>
      <Button on:click={setUnavailable}>I'm no longer available</Button>
    </div>
  {:else}
    <div style="grid-area: 1/1/1/1" out:fade in:fade|local={{ delay: 400 }}>
      <p class="mb-2">
        Use these buttons to tell other members you are available to game right now.
      </p>
      <p class="font-bold">I'm available for...</p>
      <div class="grid grid-cols-3 gap-1">
        <Button on:click={() => setAvailable(3600 * 1)}>1 hour</Button>
        <Button on:click={() => setAvailable(3600 * 2)}>2 hours</Button>
        <Button on:click={() => setAvailable(3600 * 3)}>3 hours</Button>
        <Button on:click={() => setAvailable(3600 * 4)}>4 hours</Button>
        <Button on:click={() => setAvailable(3600 * 5)}>5 hours</Button>
        <Button on:click={() => setAvailable(3600 * 6)}>6 hours</Button>
      </div>
    </div>
  {/if}
</div>
