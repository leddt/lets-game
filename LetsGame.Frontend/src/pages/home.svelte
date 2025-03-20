<script>
  import { upcomingSessionSummaryCardFragment } from "@/components/home/upcoming-session-summary-card.svelte";
  import { proposedSessionSummaryCardFragment } from "@/components/home/proposed-session-summary-card.svelte";

  import Section from "@/components/ui/section.svelte";
  import StaggeredTransition from "@/components/ui/staggered-transition.svelte";
  import CardList from "@/components/ui/card-list.svelte";
  import UpcomingSessionSummaryCard from "@/components/home/upcoming-session-summary-card.svelte";
  import ProposedSessionSummaryCard from "@/components/home/proposed-session-summary-card.svelte";
  import { scale } from "svelte/transition";

  import gql from "graphql-tag";
  import { query } from "svelte-apollo";

  export let groups;

  $: hasOneGroup = groups?.length === 1;
  $: hasManyGroups = groups?.length > 1;

  const sessionsData = query(gql`
    ${upcomingSessionSummaryCardFragment}
    ${proposedSessionSummaryCardFragment}
    query MySessions {
      upcomingSessions {
        id
        ...upcomingSessionSummaryCard
      }
      proposedSessions {
        id
        ...proposedSessionSummaryCard
      }
    }
  `);

  $: sessionsData.refetch();
  $: upcomingSessions = $sessionsData.data?.upcomingSessions;
  $: proposedSessions = $sessionsData.data?.proposedSessions;
</script>

<div class="flex-grow sm:overflow-y-auto">
  <div class="p-4 max-w-5xl m-auto">
    <div class="text-gray-100 text-xl text-center">
      <div class="text-6xl sm:text-8xl font-thin mb-16">Welcome</div>

      {#if hasOneGroup}
        <a href="/group/{groups[0].slug}">Go to your group</a>, or
        <a href="/create-group">create a new one</a>!
      {:else if hasManyGroups}
        Choose one of your groups, or
        <a href="/create-group">create a new one</a>!
      {:else}
        <a href="/create-group">Create a group</a> to get started!
      {/if}
    </div>

    <StaggeredTransition let:getTransition>
      <div class="mt-8 grid grid-cols-1 sm:grid-cols-2 gap-4">
        {#if upcomingSessions?.length > 0}
          <div in:scale={getTransition()}>
            <Section title="Upcoming sessions">
              <CardList>
                {#each upcomingSessions as session (session.id)}
                  <UpcomingSessionSummaryCard {session} />
                {/each}
              </CardList>
            </Section>
          </div>
        {/if}
        {#if proposedSessions?.length > 0}
          <div in:scale={getTransition()}>
            <Section title="Proposed sessions">
              <CardList>
                {#each proposedSessions as session (session.id)}
                  <ProposedSessionSummaryCard {session} />
                {/each}
              </CardList>
            </Section>
          </div>
        {/if}
      </div>
    </StaggeredTransition>
  </div>
</div>

<style>
  a {
    @apply text-blue-200;
  }
</style>
