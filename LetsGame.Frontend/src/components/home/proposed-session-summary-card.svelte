<script context="module">
  import gql from "graphql-tag";

  export const proposedSessionSummaryCardFragment = gql`
    fragment proposedSessionSummaryCard on ProposedSessionGraphType {
      id
      game {
        id
        name
      }
      group {
        id
        name
        slug
        members {
          id
        }
      }
      slots {
        id
      }
      missingVotes {
        id
        userId
      }
    }
  `;
</script>

<script>
  import Card from "@/components/ui/card.svelte";
  import { useNavigate } from "svelte-navigator";
  import { me } from "@/lib/store";

  export let session;

  $: slotCount = session.slots.length;
  $: memberCount = session.group.members.length;
  $: voteCount = memberCount - session.missingVotes.length;
  $: didVote = !session.missingVotes.find((x) => x.userId === $me.id);

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
    {slotCount} slots,

    {#if voteCount < memberCount}
      {voteCount}/{memberCount} votes
      {#if didVote}
        <span class="text-green-500">(✓ you voted)</span>
      {:else}
        <span class="text-red-500">(your vote is missing)</span>
      {/if}
    {:else}
      <span class="text-green-500">all votes are in</span>
    {/if}
  </p>
</Card>
