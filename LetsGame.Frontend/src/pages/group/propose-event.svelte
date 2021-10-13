<script>
  import { format, startOfDay } from "date-fns";
  import gql from "graphql-tag";
  import { useNavigate } from "svelte-navigator";

  import Button from "@/components/ui/button.svelte";
  import CalendarPicker from "@/components/ui/calendar-picker.svelte";
  import Section from "@/components/ui/section.svelte";
  import Textarea from "@/components/ui/textarea.svelte";
  import TimeslotsPicker from "@/components/ui/timeslots-picker.svelte";
  import GameTile from "@/components/group/game-tile.svelte";
  import { proposedSessionCardFragment } from "@/components/group/proposed-session-card.svelte";

  import { friendlyDateTime } from "@/lib/date-helpers";
  import client from "@/lib/apollo";

  const navigate = useNavigate();
  const today = startOfDay(new Date());

  let selectedGame = null;
  let pickedDates = [];
  let dateTimes = [];
  let details = "";

  export let group;

  const anyGame = {
    id: null,
    name: "Any game",
    description: "I don't care what we play, let's game!",
  };
  $: selectableGames = [...group.games, anyGame];

  function toLocalTimeString(date) {
    return format(date, "yyyy-MM-dd'T'HH:mm:ss");
  }

  async function createSession() {
    await client.mutate({
      mutation: gql`
        ${proposedSessionCardFragment}
        mutation ProposeSession(
          $groupId: ID!
          $gameId: ID
          $details: String
          $dateTimes: [LocalDateTime!]!
        ) {
          proposeSession(
            groupId: $groupId
            gameId: $gameId
            details: $details
            dateTimes: $dateTimes
          ) {
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
        groupId: group.id,
        gameId: selectedGame?.id,
        details,
        dateTimes: dateTimes.map(toLocalTimeString),
      },
    });

    navigate("../");
  }
</script>

<div class="p-4 flex-grow">
  <Section title="Propose a session">
    <div>
      <h3>Pick game</h3>
      <div class="flex flex-wrap gap-4">
        {#each selectableGames as game (game.id)}
          <div class="w-80">
            <GameTile
              {game}
              class="item {game === selectedGame ? 'selected' : ''}"
              on:click={() => (selectedGame = game)}
            />
          </div>
        {/each}
      </div>
    </div>

    {#if selectedGame}
      <div class="pt-4">
        <h3>Choose time slots</h3>

        <div class="flex flex-col lg:flex-row gap-4">
          <div>
            <div class="w-80">
              <h4>Pick dates</h4>
              <CalendarPicker min={today} bind:pickedDates />
            </div>
          </div>
          {#if pickedDates.length > 0}
            <div>
              <h4>Pick times</h4>
              <TimeslotsPicker dates={pickedDates} bind:dateTimes />
            </div>
          {/if}
        </div>
      </div>

      <div class="pt-4">
        <h3>Anything to add?</h3>
        <Textarea bind:value={details} class="w-full max-w-3xl" />
      </div>
    {/if}

    {#if dateTimes.length > 0}
      <div class="pt-4">
        <h3>Summary</h3>
        Your {selectedGame === anyGame ? "gaming" : selectedGame.name} session will
        have {dateTimes.length}
        {dateTimes.length > 1 ? "slots" : "slot"}:
        <ul class="list-disc mb-4">
          {#each dateTimes as dateTime}
            <li class="ml-6">{friendlyDateTime(dateTime)}</li>
          {/each}
        </ul>
        <Button on:click={createSession}>Looks good, create the session</Button>
      </div>
    {/if}
  </Section>
</div>

<style lang="postcss">
  h3 {
    @apply mb-2;
  }
  :global(.item) {
    @apply cursor-pointer hover:shadow-xl hover:ring ring-opacity-100 ring-blue-300;
  }
  :global(.item.selected) {
    @apply ring ring-green-400;
  }
</style>
