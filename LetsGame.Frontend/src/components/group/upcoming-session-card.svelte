<script context="module">
  import gql from "graphql-tag";

  const sessionParticipantsFragment = gql`
    fragment sessionParticipants on UpcomingSessionGraphType {
      participants {
        id
        userId
        displayName
      }
    }
  `;

  export const upcomingSessionCardFragment = gql`
    ${sessionParticipantsFragment}
    fragment upcomingSessionCard on UpcomingSessionGraphType {
      id
      game {
        id
        name
        igdbImageId
      }
      sessionTime
      ...sessionParticipants
      details
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
  import { subscribe } from "svelte-apollo";

  import client from "@/lib/apollo";
  import { friendlyDateTime } from "@/lib/date-helpers";
  import { me } from "@/lib/store";

  import Card from "@/components/ui/card.svelte";
  import Button from "@/components/ui/button.svelte";
  import CircleButton from "@/components/ui/circle-button.svelte";
  import FlexTrailer from "@/components/ui/flex-trailer.svelte";
  import AvatarList from "@/components/ui/avatar-list.svelte";

  export let session;

  $: gameImage = session.game?.igdbImageId
    ? `https://images.igdb.com/igdb/image/upload/t_screenshot_med/${session.game.igdbImageId}.jpg`
    : null;

  $: isPartOfSession = !!session?.participants?.find(
    (x) => x.userId === $me.id,
  );
  $: isSessionCreator = session.creator.userId === $me?.id;

  $: if (session) {
    const unsubscribe = subscribe(
      gql`
        ${upcomingSessionCardFragment}
        subscription WatchUpcomingSession($sessionId: ID!) {
          upcomingSessionUpdated(sessionId: $sessionId) {
            ...upcomingSessionCard
          }
        }
      `,
      {
        variables: {
          sessionId: session.id,
        },
      },
    ).subscribe(() => {});

    onDestroy(unsubscribe);
  }

  function join() {
    return client.mutate({
      mutation: gql`
        ${sessionParticipantsFragment}
        mutation JoinSession($sessionId: ID!) {
          joinSession(sessionId: $sessionId) {
            session {
              id
              ...sessionParticipants
            }
          }
        }
      `,
      variables: {
        sessionId: session.id,
      },
    });
  }

  function leave() {
    return client.mutate({
      mutation: gql`
        ${sessionParticipantsFragment}
        mutation LeaveSession($sessionId: ID!) {
          leaveSession(sessionId: $sessionId) {
            session {
              id
              ...sessionParticipants
            }
          }
        }
      `,
      variables: {
        sessionId: session.id,
      },
    });
  }

  async function deleteSession() {
    if (!confirm(`Cancel this ${session.game?.name || "gaming"} session?`))
      return;

    await client.mutate({
      mutation: gql`
        ${upcomingSessionCardFragment}
        mutation DeleteUpcomingSession($sessionId: ID!) {
          deleteSession(sessionId: $sessionId) {
            group {
              id
              upcomingSessions {
                ...upcomingSessionCard
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

<Card image={gameImage}>
  <div
    slot="image-content"
    class="relative h-full flex items-center justify-center bg-[radial-gradient(ellipse_at_center,rgba(0,0,0,0.5),transparent)]"
  >
    <div class="text-white text-xl font-bold">
      {session.game?.name}
    </div>
    {#if isSessionCreator}
      <div class="absolute top-0 right-0 p-1 flex gap-1">
        <Button color="red" tip="Cancel this session" on:click={deleteSession}>
          &times;
        </Button>
      </div>
    {/if}
  </div>
  <div class="flex flex-col gap-2 h-full">
    {#if !gameImage}
      <div class="flex justify-between items-center">
        <h3>{session.game?.name || "Any game"}</h3>
        {#if isSessionCreator}
          <Button
            color="red"
            tip="Cancel this session"
            on:click={deleteSession}
          >
            &times;
          </Button>
        {/if}
      </div>
    {/if}
    <span class="font-semibold">{friendlyDateTime(session.sessionTime)}</span>
    <AvatarList people={session.participants}>
      {#if isPartOfSession}
        <CircleButton on:click={leave} tip="Leave this session"
          >&ndash;</CircleButton
        >
      {:else}
        <CircleButton on:click={join} tip="Join this session">+</CircleButton>
      {/if}
    </AvatarList>
    {#if session.details}
      <p class="text-gray-500 font-light whitespace-pre-wrap">
        {session.details}
      </p>
    {/if}
    <FlexTrailer>
      <p class="text-xs">Session created by {session.creator.displayName}</p>
    </FlexTrailer>
  </div>
</Card>
