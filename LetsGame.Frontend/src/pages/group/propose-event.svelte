<script>
  import Button from "@/components/ui/button.svelte";
  import CalendarPicker from "@/components/ui/calendar-picker.svelte";
  import Section from "@/components/ui/section.svelte";
  import TimePicker from "@/components/ui/time-picker.svelte";
  import GameTile from "@/components/group/game-tile.svelte";
  import { format } from "date-fns";
  import isSameDay from "date-fns/isSameDay";

  let selectedGame = null;
  let pickedDates = [];
  let times = new Map();
  let allSame = true;

  export let group;

  $: syncTimesMap(pickedDates);
  $: if (!allSame) replicateTimes();

  // times params is there for reactivity reasons
  function getTimes(times, date) {
    if (!date) return [];
    const key = date.toISOString();
    return times.get(key);
  }

  function setTimes(date, newTimes) {
    const key = date.toISOString();
    times.set(key, newTimes);
    times = times; // Reactivity
  }

  function getDefaultTimes(forDate) {
    if (pickedDates.length <= 1) return [null];

    return [
      ...getTimes(times, pickedDates.filter((x) => !isSameDay(x, forDate))[0]),
    ];
  }

  function addTime(date) {
    setTimes(date, [...getTimes(times, date), null]);
  }
  function removeTime(date) {
    setTimes(date, [...getTimes(times, date).slice(0, -1)]);
  }

  function syncTimesMap(dates) {
    const newMap = new Map();

    for (const date of dates) {
      const key = date.toISOString();
      newMap.set(key, times.get(key) || getDefaultTimes(date));
    }

    times = newMap;
  }

  function replicateTimes() {
    const firstTimes = getTimes(times, pickedDates[0]);
    for (const date of pickedDates) {
      setTimes(date, [...firstTimes]);
    }
  }
</script>

<div class="p-4 flex-grow">
  <Section title="Propose a session">
    <h3>Pick game</h3>
    <div class="pt-4 flex flex-wrap gap-4">
      {#each group.games as game (game.id)}
        <div class="w-80">
          <GameTile
            {game}
            class="item {game === selectedGame ? 'selected' : ''}"
            on:click={() => (selectedGame = game)}
          />
        </div>
      {/each}
    </div>

    {#if selectedGame}
      <div class="pt-4">
        <h3>Choose time slots</h3>

        <div class="flex flex-col lg:flex-row gap-4">
          <div>
            <div class="w-80">
              Pick dates
              <CalendarPicker bind:pickedDates />
            </div>
          </div>
          {#if pickedDates.length > 0}
            <div>
              Pick times
              <table class="w-full">
                {#each pickedDates as date, index}
                  {#if index === 1}
                    <tr>
                      <td colspan="2">
                        <label>
                          <input type="checkbox" bind:checked={allSame} />
                          Use same time slots for all dates
                        </label>
                      </td>
                    </tr>
                  {/if}
                  <tr>
                    <td class="whitespace-nowrap align-top pt-1 w-32">
                      {format(date, "MMMM do")}
                    </td>
                    <td>
                      <div
                        class="flex flex-wrap gap-1"
                        class:invisible={allSame && index > 0}
                      >
                        {#each getTimes(times, date) as time}
                          <TimePicker bind:value={time} />
                        {/each}
                        <div class="whitespace-nowrap">
                          <Button
                            tip="Add another time slot for {format(
                              date,
                              'MMMM do'
                            )}"
                            on:click={() => addTime(date)}>+</Button
                          >
                          {#if getTimes(times, date).length > 1}
                            <Button
                              tip="Remove time slot from {format(
                                date,
                                'MMMM do'
                              )}"
                              on:click={() => removeTime(date)}>-</Button
                            >
                          {/if}
                        </div>
                      </div>
                    </td>
                  </tr>
                {/each}
              </table>
            </div>
          {/if}
        </div>
      </div>
    {/if}
  </Section>
</div>

<style>
  :global(.item) {
    @apply cursor-pointer hover:shadow-xl hover:ring ring-opacity-100 ring-blue-300;
  }
  :global(.item.selected) {
    @apply ring ring-green-400;
  }
</style>
