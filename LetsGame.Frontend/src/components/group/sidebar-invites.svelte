<script context="module">
  import gql from "graphql-tag";

  export const sidebarInvitesFragment = gql`
    fragment sidebarInvites on GroupGraphType {
      invites {
        id
        inviteCode
        isSingleUse
      }
    }
  `;
</script>

<script>
  import { fade } from "svelte/transition";
  import { flip } from "svelte/animate";
  
  import client from "@/lib/apollo";
  import Button from "@/components/ui/button.svelte";

  export let group;

  function create(singleUse) {
    return client.mutate({
      mutation: gql`
        ${sidebarInvitesFragment}
        mutation CreateInvite($groupId: ID!, $singleUse: Boolean!) {
          createInvite(groupId: $groupId, singleUse: $singleUse) {
            group {
              id
              ...sidebarInvites
            }
          }
        }
      `,
      variables: {
        groupId: group.id,
        singleUse
      }
    });
  }

  function deleteInvite(invite) {
    return client.mutate({
      mutation: gql`
        ${sidebarInvitesFragment}
        mutation DeleteInvite($groupId: ID!, $inviteCode: String!) {
          deleteInvite(groupId: $groupId, inviteCode: $inviteCode) {
            group {
              id
              ...sidebarInvites
            }
          }
        }
      `,
      variables: {
        groupId: group.id,
        inviteCode: invite.inviteCode
      }
    });
  }
</script>

<div class="flex flex-col gap-2">
  {#if group.invites.length === 0}
    <p>Create links to invite your friends to the group!</p>
  {/if}
  {#each group.invites as invite (invite.id)}
    <div class="flex gap-2" animate:flip in:fade|local>
      <Button color="red" tip="Revoke this invite" on:click={() => deleteInvite(invite)}>
        &times;
      </Button>
      <div class="flex-grow flex flex-col">
        <input
          class="w-full text-sm bg-transparent"
          readonly
          value="{location.origin}/i/{invite.inviteCode}"
          on:click={(ev) => ev.target.select()}
        />
        <span class="text-xs text-gray-500">
          {#if invite.isSingleUse}
            (single use)
          {:else}
            (unlimited)
          {/if}
        </span>
      </div>
    </div>
  {/each}
  <div>
    <p class="font-bold">Create new invite:</p>
    <div class="flex gap-2">
      <Button on:click={() => create(false)}>Unlimited</Button>
      <Button on:click={() => create(true)}>Single use</Button>
    </div>
  </div>
</div>
