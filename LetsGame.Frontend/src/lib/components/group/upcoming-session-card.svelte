<script context="module">
  import gql from "graphql-tag";

  export const upcomingSessionCardFragment = gql`
    fragment upcomingSessionCard on UpcomingSessionGraphType {
      id
      game {
        id
        name
        igdbImageId
      }
      sessionTime
      participants {
        id
        userId
        displayName
      }
      details
      creator {
        id
        displayName
      }
    }
  `;
</script>

<script>
  import { getContext } from "svelte";
  import Card from "$lib/components/ui/card.svelte";
  import CircleButton from "$lib/components/ui/circle-button.svelte";
  import FlexTrailer from "$lib/components/ui/flex-trailer.svelte";
  import AvatarList from "$lib/components/ui/avatar-list.svelte";
  import { friendlyDateTime } from "$lib/date-helpers";

  const me = getContext("me");
  export let session;

  $: gameImage = session.game?.igdbImageId
    ? `https://images.igdb.com/igdb/image/upload/t_screenshot_med/${session.game.igdbImageId}.jpg`
    : null;

  $: isPartOfSession = !!session?.participants?.find((x) => x.userId === me.id);
</script>

<Card image={gameImage}>
  <div class="flex flex-col gap-2 h-full">
    <h3>{session.game?.name || "Any game"}</h3>
    <strong>{friendlyDateTime(session.sessionTime)}</strong>
    <AvatarList people={session.participants}>
      {#if isPartOfSession}
        <CircleButton on:click={() => alert("leave")} tip="Leave this session">
          &ndash;
        </CircleButton>
      {:else}
        <CircleButton on:click={() => alert("join")} tip="Join this session">+</CircleButton>
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
