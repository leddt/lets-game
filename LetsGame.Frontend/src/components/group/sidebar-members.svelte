<script context="module">
  import gql from "graphql-tag";

  export const sidebarMembersFragment = gql`
    fragment sidebarMembers on GroupGraphType {
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
  import Avatar from "@/components/ui/avatar.svelte";
  import { time } from "@/lib/date-helpers";
  import { me } from "@/lib/store";

  export let group;
</script>

{#if group?.members}
  <div class="flex flex-col gap-2">
    {#each group.members as member (member.id)}
      <div class="flex items-center gap-2">
        <Avatar name={member.displayName} active={!!member.availableUntil} />
        <div>
          <div>
            <span class:font-bold={member.userId === $me.id}
              >{member.displayName}</span
            >
            {#if member.role === "OWNER"}
              (owner)
            {/if}
          </div>
          {#if member.availableUntil}
            <div class="text-xs" transition:fade|local>
              Available until
              {time(member.availableUntil)}!
            </div>
          {/if}
        </div>
      </div>
    {/each}
  </div>
{/if}
