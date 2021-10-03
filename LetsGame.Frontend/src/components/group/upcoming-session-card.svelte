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
          displayName
        }
      }
    `;
  </script>
  
  <script>
    import client from "../../lib/apollo";
    import Card from "../ui/card.svelte";
    import CircleButton from "../ui/circle-button.svelte";
    import FlexTrailer from "../ui/flex-trailer.svelte";
    import AvatarList from "../ui/avatar-list.svelte";
    import { friendlyDateTime } from "../../lib/date-helpers";
    import { me } from "../../lib/store"
      
    export let session;
  
    $: gameImage = session.game?.igdbImageId
      ? `https://images.igdb.com/igdb/image/upload/t_screenshot_med/${session.game.igdbImageId}.jpg`
      : null;
  
    $: isPartOfSession = !!session?.participants?.find((x) => x.userId === $me.id);
  
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
          sessionId: session.id
        }
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
          sessionId: session.id
        }
      });
    }
  </script>
  
  <Card image={gameImage}>
    <div class="flex flex-col gap-2 h-full">
      <h3>{session.game?.name || "Any game"}</h3>
      <strong>{friendlyDateTime(session.sessionTime)}</strong>
      <AvatarList people={session.participants}>
        {#if isPartOfSession}
          <CircleButton on:click={leave} tip="Leave this session">&ndash;</CircleButton>
        {:else}
          <CircleButton on:click={join} tip="Join this session">+</CircleButton>
        {/if}
      </AvatarList>
      {#if session.details}
        <p class="text-gray-500 font-light">{session.details}</p>
      {/if}
      <FlexTrailer>
        <p class="text-xs">Session created by {session.creator.displayName}</p>
      </FlexTrailer>
    </div>
  </Card>
  