<script context="module">
  import gql from "graphql-tag";

  export const upcomingSessionSummaryCardFragment = gql`
    fragment upcomingSessionSummaryCard on UpcomingSessionGraphType {
      id
      sessionTime
      participants {
        id
      }
      game {
        id
        name
      }
      group {
        id
        name
        slug
      }
    }
  `;
</script>

<script>
  import { onDestroy } from "svelte";
  import { subscribe } from "svelte-apollo";
  import { useNavigate } from "svelte-navigator";
  import Card from "@/components/ui/card.svelte";
  import { friendlyDateTime } from "@/lib/date-helpers";

  export let session;

  const navigate = useNavigate();

  $: if (session) {
    const unsubscribe = subscribe(
      gql`
        ${upcomingSessionSummaryCardFragment}
        subscription WatchUpcomingSessionSummary($sessionId: ID!) {
          upcomingSessionUpdated(sessionId: $sessionId) {
            ...upcomingSessionSummaryCard
          }
        }
      `,
      {
        variables: {
          sessionId: session.id,
        },
      }
    ).subscribe(() => {});

    onDestroy(unsubscribe);
  }
</script>

<Card
  width="w-full"
  clickable
  on:click={() => navigate(`/group/${session.group.slug}`)}
>
  <p>
    <strong>{session.game?.name || "Any game"}</strong> with
    <span class="text-blue-500">{session.group.name}</span>
  </p>
  <p class="text-gray-500">
    {friendlyDateTime(session.sessionTime)},
    {session.participants.length} confirmed players
  </p>
</Card>
