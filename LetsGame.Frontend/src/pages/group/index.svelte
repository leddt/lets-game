<script>
  import gql from "graphql-tag";
  import { query } from "svelte-apollo";
  import { Route, useNavigate } from "svelte-navigator";

  import CardList from "@/components/ui/card-list.svelte";
  import Section from "@/components/ui/section.svelte";
  import Button from "@/components/ui/button.svelte";
  import UpcomingSessionCard from "@/components/group/upcoming-session-card.svelte";
  import ProposedSessionCard from "@/components/group/proposed-session-card.svelte";
  import GroupSidebar from "@/components/group/group-sidebar.svelte";

  import GroupAddGame from "./add-game.svelte";
  import GroupProposeEvent from "./propose-event.svelte";

  import { upcomingSessionCardFragment } from "@/components/group/upcoming-session-card.svelte";
  import { proposedSessionCardFragment } from "@/components/group/proposed-session-card.svelte";
  import { sidebarFragment } from "@/components/group/group-sidebar.svelte";

  export let slug;

  const navigate = useNavigate();

  $: groupData = query(
    gql`
      ${upcomingSessionCardFragment}
      ${proposedSessionCardFragment}
      ${sidebarFragment}
      query GroupData($slug: String) {
        groupBySlug(slug: $slug) {
          id
          name
          upcomingSessions {
            ...upcomingSessionCard
          }
          proposedSessions {
            ...proposedSessionCard
          }
          ...sidebar
        }
      }
    `,
    {
      variables: {
        slug,
      },
    }
  );

  $: groupData.refetch();
  $: group = $groupData.data?.groupBySlug;
</script>

<div class="flex flex-col flex-grow min-w-0 overflow-y-scroll">
  {#if $groupData.data}
    <div class="p-4 bg-gray-600 text-gray-100">
      <h1>{group?.name}</h1>
    </div>

    <div class="flex flex-col sm:flex-row flex-grow">
      <Route path="/">
        <div class="p-4 flex flex-col flex-grow gap-4 min-w-0">
          {#if group?.upcomingSessions?.length > 0}
            <Section
              title="Upcoming sessions ({group.upcomingSessions.length})"
            >
              <CardList>
                {#each group.upcomingSessions as s (s.id)}
                  <UpcomingSessionCard session={s} />
                {/each}
              </CardList>
            </Section>
          {/if}

          {#if group?.proposedSessions}
            <Section
              title="Proposed sessions ({group.proposedSessions.length})"
            >
              <Button slot="right" on:click={() => navigate("propose-event")}
                >Propose new session</Button
              >

              <CardList>
                {#each group.proposedSessions as s (s.id)}
                  <ProposedSessionCard session={s} />
                {/each}
              </CardList>
            </Section>
          {/if}
        </div>

        <GroupSidebar {group} />
      </Route>

      <Route path="add-game"><GroupAddGame groupId={group.id} /></Route>
      <Route path="propose-event"><GroupProposeEvent {group} /></Route>
    </div>
  {/if}
</div>
