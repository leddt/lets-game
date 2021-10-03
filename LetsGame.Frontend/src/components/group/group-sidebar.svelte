<script context="module">
  import gql from "graphql-tag";
  import { sidebarMembersFragment } from "./sidebar-members.svelte";
  import { sidebarAvailabilityFragment } from "./sidebar-availability.svelte";
  import { sidebarInvitesFragment } from "./sidebar-invites.svelte";
  import { sidebarGamesFragment } from "./sidebar-games.svelte";

  export const sidebarFragment = gql`
    ${sidebarMembersFragment}
    ${sidebarAvailabilityFragment}
    ${sidebarInvitesFragment}
    ${sidebarGamesFragment}
    fragment sidebar on GroupGraphType {
      self {
        id
        role
      }
      ...sidebarMembers
      ...sidebarAvailability
      ...sidebarInvites
      ...sidebarGames
    }
  `;
</script>

<script>
  import { useNavigate } from "svelte-navigator";
  import Button from "@/components/ui/button.svelte";
  import Section from "@/components/ui/section.svelte";

  import SidebarMembers from "./sidebar-members.svelte";
  import SidebarAvailability from "./sidebar-availability.svelte";
  import SidebarInvites from "./sidebar-invites.svelte";
  import SidebarGames from "./sidebar-games.svelte";

  export let group;

  const navigate = useNavigate();

  $: isOwner = group?.self.role === "OWNER";
</script>

<div>
  <div
    class="sm:w-72 bg-gray-500 pb-4 sm:pt-4 px-4 sm:pl-0 flex flex-col gap-4"
  >
    <Section title="Members"><SidebarMembers {group} /></Section>
    <Section title="Available?"><SidebarAvailability {group} /></Section>
    {#if group?.invites}
      <Section title="Invites"><SidebarInvites {group} /></Section>
    {/if}
    <Section title="Games">
      <div slot="right">
        {#if isOwner}
          <Button slot="right" on:click={() => navigate("add-game")}>
            Add
          </Button>
        {/if}
      </div>
      <SidebarGames {group} />
    </Section>
  </div>
</div>
