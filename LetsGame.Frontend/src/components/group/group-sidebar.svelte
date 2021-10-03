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
      ...sidebarMembers
      ...sidebarAvailability
      ...sidebarInvites
      ...sidebarGames
    }
  `;
</script>

<script>
  import Section from "../ui/section.svelte";
  import SidebarMembers from "./sidebar-members.svelte";
  import SidebarAvailability from "./sidebar-availability.svelte";
  import SidebarInvites from "./sidebar-invites.svelte";
  import SidebarGames from "./sidebar-games.svelte";

  export let group;
  export let isOwner = true; //TODO
</script>

<div>
  <div class="sm:w-72 bg-gray-500 pb-4 sm:pt-4 px-4 sm:pl-0 flex flex-col gap-4">
    <Section title="Members"><SidebarMembers {group} /></Section>
    <Section title="Available?"><SidebarAvailability {group} /></Section>
    {#if isOwner}
      <Section title="Invites"><SidebarInvites {group} /></Section>
    {/if}
    <Section title="Games"><SidebarGames {group} /></Section>
  </div>
</div>
