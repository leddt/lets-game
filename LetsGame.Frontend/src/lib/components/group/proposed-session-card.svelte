<script context="module">
  import gql from "graphql-tag";
  import { upcomingSessionCardFragment } from "./upcoming-session-card.svelte";

  const slotVotesFragment = gql`
    fragment slotVotes on SessionSlotGraphType {
      voters {
        id
        userId
        displayName
      }
    }
  `;
  const sessionVotesFragment = gql`
    fragment sessionVotes on ProposedSessionGraphType {
      missingVotes {
        id
        userId
        displayName
      }
      cantPlays {
        id
        userId
        displayName
      }
    }
  `;

  export const proposedSessionCardFragment = gql`
    ${slotVotesFragment}
    ${sessionVotesFragment}
    fragment proposedSessionCard on ProposedSessionGraphType {
      id
      game {
        id
        name
        igdbImageId
      }
      details
      ...sessionVotes
      reminderSentAtTime
      slots {
        id
        proposedTime
        ...slotVotes
      }
      creator {
        id
        displayName
      }
    }
  `;
</script>

<script>
  import Button from "$lib/components/ui/button.svelte";
  import Card from "$lib/components/ui/card.svelte";
  import CircleButton from "$lib/components/ui/circle-button.svelte";
  import Panel from "$lib/components/ui/panel.svelte";
  import FlexTrailer from "$lib/components/ui/flex-trailer.svelte";
  import AvatarList from "$lib/components/ui/avatar-list.svelte";
  import { friendlyDateTime } from "$lib/date-helpers";
  import { getContext } from "svelte";
  import client from "$lib/apollo";

  const me = getContext("me");
  export let session;

  $: gameImage = session.game?.igdbImageId
    ? `https://images.igdb.com/igdb/image/upload/t_screenshot_med/${session.game.igdbImageId}.jpg`
    : null;

  $: isInCantPlayList = session.cantPlays.find((x) => x.userId == me.id);

  function isInSlot(slot) {
    return !!slot?.voters?.find((x) => x.userId === me.id);
  }

  function vote(slot) {
    return client.mutate({
      mutation: gql`
        ${slotVotesFragment}
        ${sessionVotesFragment}
        mutation VoteSlot($slotId: ID!) {
          voteSlot(slotId: $slotId) {
            slot {
              id
              ...slotVotes
              session {
                id
                ...sessionVotes
              }
            }
          }
        }
      `,
      variables: {
        slotId: slot.id
      }
    });
  }

  function unvote(slot) {
    return client.mutate({
      mutation: gql`
        ${slotVotesFragment}
        ${sessionVotesFragment}
        mutation UnvoteSlot($slotId: ID!) {
          unvoteSlot(slotId: $slotId) {
            slot {
              id
              ...slotVotes
              session {
                id
                ...sessionVotes
              }
            }
          }
        }
      `,
      variables: {
        slotId: slot.id
      }
    });
  }

  function cantPlay() {
    return client.mutate({
      mutation: gql`
        ${slotVotesFragment}
        ${sessionVotesFragment}
        mutation CantPlay($sessionId: ID!) {
          cantPlay(sessionId: $sessionId) {
            session {
              id
              ...sessionVotes
              slots {
                id
                ...slotVotes
              }
            }
          }
        }
      `,
      variables: {
        sessionId: session.id
      }
    });
  }

  function remind() {
    return client.mutate({
      mutation: gql`
        mutation SendReminder($sessionId: ID!) {
          sendReminder(sessionId: $sessionId) {
            session {
              id
              reminderSentAtTime
            }
          }
        }
      `,
      variables: {
        sessionId: session.id
      }
    });
  }

  async function pick(slotId) {
    await client.mutate({
      mutation: gql`
        ${proposedSessionCardFragment}
        ${upcomingSessionCardFragment}
        mutation PickSlot($slotId: ID!) {
          selectWinningSlot(slotId: $slotId) {
            group {
              id
              proposedSessions {
                ...proposedSessionCard
              }
              upcomingSessions {
                ...upcomingSessionCard
              }
            }
          }
        }
      `,
      variables: {
        slotId
      }
    });

    window.scrollTo(0, 0);
  }
</script>

<Card image={gameImage} width="w-164" imageHeight="h-48">
  <div class="flex flex-col gap-2 h-full">
    <h3>{session.game?.name || "Any game"}</h3>
    {#if session.details}
      <p class="text-gray-500 font-light">{session.details}</p>
    {/if}
    {#if session.missingVotes.length > 0}
      <div>
        Missing votes:
        <AvatarList people={session.missingVotes}>
          <Button on:click={remind}>Send reminder</Button>
        </AvatarList>
        {#if session.reminderSentAtTime}
          <p class="text-xs">Reminder sent {friendlyDateTime(session.reminderSentAtTime, false)}</p>
        {/if}
      </div>
    {/if}
    <div class="flex gap-2 flex-wrap">
      {#each session.slots as slot}
        <Panel class="flex flex-col gap-2 w-52" title={friendlyDateTime(slot.proposedTime)}>
          <AvatarList people={slot.voters}>
            {#if isInSlot(slot)}
              <CircleButton on:click={() => unvote(slot)} tip="Remove your vote from this slot">
                -
              </CircleButton>
            {:else}
              <CircleButton on:click={() => vote(slot)} tip="Add your vote to this slot">
                +
              </CircleButton>
            {/if}
          </AvatarList>
          <FlexTrailer>
            <Button on:click={() => pick(slot.id)}>Select as winning slot</Button>
          </FlexTrailer>
        </Panel>
      {/each}

      <Panel color="bg-red-50" class="flex flex-col gap-2 w-52" title="Not available">
        {#if session.cantPlays.length > 0}
          <AvatarList people={session.cantPlays} />
        {/if}
        {#if !isInCantPlayList}
          <FlexTrailer>
            <Button on:click={cantPlay}>I'm not available</Button>
          </FlexTrailer>
        {/if}
      </Panel>
    </div>
    <FlexTrailer>
      <p class="text-xs">Session proposed by {session.creator.displayName}</p>
    </FlexTrailer>
  </div>
</Card>
