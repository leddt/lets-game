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
      group {
        id
        self {
          id
          role
        }
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
        userId
        displayName
      }
    }
  `;
</script>

<script>
  import { onDestroy } from "svelte";
  import { fade } from "svelte/transition";
  import { subscribe } from "svelte-apollo";
  import throttle from "lodash/throttle.js";

  import Button from "@/components/ui/button.svelte";
  import Card from "@/components/ui/card.svelte";
  import CircleButton from "@/components/ui/circle-button.svelte";
  import Panel from "@/components/ui/panel.svelte";
  import FlexTrailer from "@/components/ui/flex-trailer.svelte";
  import AvatarList from "@/components/ui/avatar-list.svelte";

  import { friendlyDateTime } from "@/lib/date-helpers";
  import client from "@/lib/apollo";
  import { me } from "@/lib/store";

  export let session;

  $: gameImage = session.game?.igdbImageId
    ? `https://images.igdb.com/igdb/image/upload/t_screenshot_med/${session.game.igdbImageId}.jpg`
    : null;

  $: isInCantPlayList = session.cantPlays.find((x) => x.userId == $me?.id);
  $: isSessionCreator = session.creator.userId === $me?.id;
  $: isGroupOwner = session.group.self.role === "OWNER";
  $: canManageSession = isSessionCreator || isGroupOwner;

  $: if (session) {
    const unsubscribe = subscribe(
      gql`
        ${proposedSessionCardFragment}
        subscription WatchProposedSession($sessionId: ID!) {
          proposedSessionUpdated(sessionId: $sessionId) {
            ...proposedSessionCard
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

  function isInSlot(slot) {
    return !!slot?.voters?.find((x) => x.userId === $me.id);
  }

  const setVote = throttle(
    (slot, state) => (state ? vote(slot) : unvote(slot)),
    400,
    {
      trailing: false,
    }
  );

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
        slotId: slot.id,
      },
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
        slotId: slot.id,
      },
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
        sessionId: session.id,
      },
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
        sessionId: session.id,
      },
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
        slotId,
      },
    });

    window.scrollTo(0, 0);
  }

  async function deleteSession() {
    if (!confirm(`Cancel this ${session.game?.name || "gaming"} session?`))
      return;

    await client.mutate({
      mutation: gql`
        ${proposedSessionCardFragment}
        mutation DeleteProposedSession($sessionId: ID!) {
          deleteSession(sessionId: $sessionId) {
            group {
              id
              proposedSessions {
                ...proposedSessionCard
              }
            }
          }
        }
      `,
      variables: {
        sessionId: session.id,
      },
    });
  }
</script>

<Card image={gameImage} width="w-164" imageHeight="h-48">
  <div class="flex flex-col gap-2 h-full">
    <div class="flex justify-between items-center">
      <h3>{session.game?.name || "Any game"}</h3>
      {#if canManageSession}
        <Button color="red" tip="Cancel this session" on:click={deleteSession}>
          &times;
        </Button>
      {/if}
    </div>
    {#if session.details}
      <p class="text-gray-500 font-light whitespace-pre-wrap">
        {session.details}
      </p>
    {/if}
    {#if session.missingVotes.length > 0}
      <div>
        Missing votes:
        <AvatarList people={session.missingVotes}>
          {#if canManageSession}
            <Button on:click={remind}>Send reminder</Button>
          {/if}
        </AvatarList>
        {#if session.reminderSentAtTime}
          <p class="text-xs">
            Reminder sent {friendlyDateTime(session.reminderSentAtTime, false)}
          </p>
        {/if}
      </div>
    {/if}
    <div class="flex gap-2 flex-wrap">
      {#each session.slots as slot (slot.id)}
        <Panel
          class="flex flex-col gap-2 w-52"
          title={friendlyDateTime(slot.proposedTime)}
        >
          <AvatarList people={slot.voters}>
            {#if isInSlot(slot)}
              <div>
                <CircleButton
                  on:click={() => setVote(slot, false)}
                  tip="Remove your vote from this slot"
                >
                  &ndash;
                </CircleButton>
              </div>
            {:else}
              <div>
                <CircleButton
                  on:click={() => setVote(slot, true)}
                  tip="Add your vote to this slot"
                >
                  +
                </CircleButton>
              </div>
            {/if}
          </AvatarList>
          {#if canManageSession}
            <FlexTrailer>
              <Button on:click={() => pick(slot.id)}>
                Select as winning slot
              </Button>
            </FlexTrailer>
          {/if}
        </Panel>
      {/each}

      <Panel
        color="bg-red-50"
        class="flex flex-col gap-2 w-52"
        title="Not available"
      >
        {#if session.cantPlays.length > 0}
          <div in:fade|local>
            <AvatarList people={session.cantPlays} />
          </div>
        {/if}
        {#if !isInCantPlayList}
          <FlexTrailer>
            <div in:fade|local class="flex flex-col">
              <Button on:click={cantPlay}>I'm not available</Button>
            </div>
          </FlexTrailer>
        {/if}
      </Panel>
    </div>
    <FlexTrailer>
      <p class="text-xs">Session proposed by {session.creator.displayName}</p>
    </FlexTrailer>
  </div>
</Card>