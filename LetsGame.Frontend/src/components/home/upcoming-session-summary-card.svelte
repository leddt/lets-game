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
  import Card from "@/components/ui/card.svelte";
  import { useNavigate } from "svelte-navigator";
  import { friendlyDateTime } from "@/lib/date-helpers";

  export let session;

  const navigate = useNavigate();
</script>

<Card
  width="w-full"
  clickable
  on:click={() => navigate(`/group/${session.group.slug}`)}
>
  <p>
    <strong>{session.game.name}</strong> with
    <span class="text-blue-500">{session.group.name}</span>
  </p>
  <p class="text-gray-500">
    {friendlyDateTime(session.sessionTime)},
    {session.participants.length} confirmed players
  </p>
</Card>
