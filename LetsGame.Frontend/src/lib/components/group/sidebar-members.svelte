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
  import { getContext } from "svelte";
  import Avatar from "$lib/components/ui/avatar.svelte";
  import { time } from "$lib/date-helpers";

  export let group;

  const me = getContext("me");
</script>

{#if group?.members}
  <div class="flex flex-col gap-2">
    {#each group.members as member}
      <div class="flex items-center gap-2">
        <Avatar name={member.displayName} />
        <div>
          <div>
            <span class:font-bold={member.userId === me.id}>{member.displayName}</span>
            {#if member.role === "OWNER"}
              (owner)
            {/if}
          </div>
          {#if member.availableUntil}
            <div class="text-xs">
              Available until
              {time(member.availableUntil)}!
            </div>
          {/if}
        </div>
      </div>
    {/each}
  </div>
{/if}
