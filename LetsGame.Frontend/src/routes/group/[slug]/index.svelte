<script>
  import gql from "graphql-tag";
  import { query } from "svelte-apollo";

  import { page } from "$app/stores";
  import CardList from "$lib/components/ui/card-list.svelte";
  import Section from "$lib/components/ui/section.svelte";
  import UpcomingSessionCard from "$lib/components/group/upcoming-session-card.svelte";
  import ProposedSessionCard from "$lib/components/group/proposed-session-card.svelte";
  import GroupSidebar from "$lib/components/group/group-sidebar.svelte";

  import { upcomingSessionCardFragment } from "$lib/components/group/upcoming-session-card.svelte";
  import { proposedSessionCardFragment } from "$lib/components/group/proposed-session-card.svelte";
  import { sidebarFragment } from "$lib/components/group/group-sidebar.svelte";

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
        slug: $page.params.slug
      }
    }
  );

  $: groupData.refetch();

  $: group = $groupData.data?.groupBySlug;
</script>

<div class="flex flex-col flex-grow min-w-0 overflow-y-scroll">
  {#if group}
    <div class="p-4 bg-gray-600 text-gray-100">
      <h1>{group.name}</h1>
    </div>

    <div class="flex flex-col sm:flex-row flex-grow bg-gray-500">
      <div class="p-4 flex flex-col flex-grow gap-4 min-w-0">
        <Section title="Upcoming sessions ({group.upcomingSessions.length})">
          <CardList>
            {#each group.upcomingSessions as s (s.id)}
              <UpcomingSessionCard session={s} />
            {/each}
          </CardList>
        </Section>

        <Section title="Proposed sessions ({group.proposedSessions.length})">
          <CardList>
            {#each group.proposedSessions as s (s.id)}
              <ProposedSessionCard session={s} />
            {/each}
          </CardList>
        </Section>
      </div>

      <GroupSidebar {group} />
    </div>
  {/if}
</div>
