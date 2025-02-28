<script context="module">
  import gql from "graphql-tag";

  export const sidebarMembersFragment = gql`
    fragment sidebarMembers on GroupGraphType {
      self {
        id
        role
        displayName
      }
      members {
        id
        userId
        displayName
        availableUntil
        role
      }
    }
  `;
</script>

<script>
  import { fade } from "svelte/transition";
  import { time } from "@/lib/date-helpers";
  import { me } from "@/lib/store";
  import client from "@/lib/apollo";
  import Avatar from "@/components/ui/avatar.svelte";
  import Button from "@/components/ui/button.svelte";

  export let group;

  $: isOwner = group?.self.role === "OWNER";

  function removeMember(member) {
    if (!confirm(`Remove ${member.displayName} from the group?`)) return;

    client.mutate({
      mutation: gql`
        ${sidebarMembersFragment}
        mutation RemoveMember($groupId: ID!, $memberId: ID!) {
          removeGroupMember(groupId: $groupId, memberId: $memberId) {
            group {
              id
              ...sidebarMembers
            }
          }
        }
      `,
      variables: {
        groupId: group.id,
        memberId: member.id,
      },
    });
  }

  function editDisplayName() {
    const newName = prompt("Enter your new name", group.self.displayName);
    if (!newName || newName === group.self.displayName) return;

    client.mutate({
      mutation: gql`
        mutation EditDisplayName($groupId: ID!, $newName: String!) {
          editDisplayName(groupId: $groupId, newName: $newName) {
            membership {
              id
              displayName
            }
          }
        }
      `,
      variables: {
        groupId: group.id,
        newName,
      },
    });
  }
</script>

{#if group?.members}
  <div class="flex flex-col gap-1">
    {#each group.members as member (member.id)}
      <div class="flex items-center gap-2">
        <Avatar
          id={member.id}
          name={member.displayName}
          active={!!member.availableUntil}
        />
        <div class="flex-grow">
          <div>
            <span class:font-bold={member.userId === $me.id}>
              {member.displayName}
            </span>
            {#if member.role === "OWNER"}
              <span class="text-sm">(owner)</span>
            {/if}
          </div>
          {#if member.availableUntil}
            <div class="text-xs" transition:fade|local>
              Available until
              {time(member.availableUntil)}!
            </div>
          {/if}
        </div>
        {#if member.id === group.self.id}
          <Button on:click={editDisplayName} tip="Edit display name">✎</Button>
        {:else if isOwner}
          <Button
            color="red"
            on:click={() => removeMember(member)}
            tip="Remove from group"
          >
            &times;
          </Button>
        {/if}
      </div>
    {/each}

    {#if group.members.length < 2}
      <p>It's lonely in here... You should invite your friends to join you!</p>
    {/if}
  </div>
{/if}
