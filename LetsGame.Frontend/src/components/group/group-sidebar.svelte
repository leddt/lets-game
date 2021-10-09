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
      members {
        id
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
  import client from "@/lib/apollo";

  export let group;

  const navigate = useNavigate();

  $: isOwner = group?.self.role === "OWNER";

  function createInvite(singleUse) {
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
        singleUse,
      },
    });
  }
</script>

<div>
  <div class="sm:w-72 pb-4 sm:pt-4 px-4 sm:pl-0 flex flex-col gap-4">
    <Section title="Members"><SidebarMembers {group} /></Section>
    {#if group?.members.length > 1}
      <Section title="Available?"><SidebarAvailability {group} /></Section>
    {/if}
    {#if group?.invites}
      <Section title="Invites">
        <div slot="right">
          {#if isOwner}
            <Button
              tip="Create new single-use invite"
              on:click={() => createInvite(true)}
            >
              + &#9312;
            </Button>
            <Button
              tip="Create new unlimited invite"
              on:click={() => createInvite(false)}
            >
              + &infin;
            </Button>
          {/if}
        </div>
        <SidebarInvites {group} />
      </Section>
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
