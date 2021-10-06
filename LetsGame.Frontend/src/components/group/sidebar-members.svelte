<script context="module">
  import gql from "graphql-tag";

  export const sidebarMembersFragment = gql`
    fragment sidebarMembers on GroupGraphType {
      self {
        id
        role
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
  import Avatar from "@/components/ui/avatar.svelte";
  import Button from "@/components/ui/button.svelte";

  export let group;

  $: isOwner = group?.self.role === "OWNER";
</script>

{#if group?.members}
  <div class="flex flex-col gap-1">
    {#each group.members as member (member.id)}
      <div class="flex items-center gap-2">
        <Avatar name={member.displayName} active={!!member.availableUntil} />
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
        {#if member.id !== group.self.id}
          <Button color="red">&times;</Button>
        {/if}
      </div>
    {/each}
  </div>
{/if}
