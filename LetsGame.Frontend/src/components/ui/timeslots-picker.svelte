<script>
  import { addMinutes, format, isSameDay } from "date-fns";
  import Button from "@/components/ui/button.svelte";
  import TimePicker from "@/components/ui/time-picker.svelte";

  export let dates;
  export let dateTimes = [];

  let allSame = true;
  let times = new Map();

  $: syncTimesMap(dates);
  $: if (!allSame) replicateTimes();
  $: dateTimes = buildDateTimes(dates, times);

  // times params is there for reactivity reasons
  function getTimes(times, date) {
    if (!date) return [];
    const key = date.toISOString();
    return times.get(key);
  }

  function addTime(date) {
    setTimes(date, [...getTimes(times, date), null]);
  }
  function removeTime(date) {
    setTimes(date, [...getTimes(times, date).slice(0, -1)]);
  }

  function setTimes(date, newTimes) {
    const key = date.toISOString();
    times.set(key, newTimes);
    times = times; // Reactivity
  }

  function syncTimesMap(dates) {
    const newMap = new Map();

    for (const date of dates) {
      const key = date.toISOString();
      newMap.set(key, times.get(key) || getDefaultTimes(date));
    }

    times = newMap;
  }

  function getDefaultTimes(forDate) {
    if (dates.length <= 1) return [null];

    return [...getTimes(times, dates.filter((x) => !isSameDay(x, forDate))[0])];
  }

  function replicateTimes() {
    const firstTimes = getTimes(times, dates[0]);
    for (const date of dates) {
      setTimes(date, [...firstTimes]);
    }
  }

  function buildDateTimes(dates, timesMap) {
    if (dates.length === 0) return [];

    const results = [];
    const firstDateTimes = getTimes(timesMap, dates[0]);

    for (const date of dates) {
      const times = allSame ? firstDateTimes : getTimes(timesMap, date);
      for (const time of times) {
        if (!time) continue;
        const [hours, minutes] = time.split(":").map((x) => parseInt(x));
        const totalMinutes = hours * 60 + minutes;
        results.push(addMinutes(date, totalMinutes));
      }
    }

    return results;
  }
</script>

<table class="w-full">
  {#each dates as date, index}
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
            <TimePicker
              bind:value={time}
              on:change={() => (times = times)}
              list="common-times"
            />
          {/each}
          <div class="whitespace-nowrap">
            <Button
              tip={"Add another time slot" +
                (allSame ? "" : ` for ${format(date, "MMMM do")}`)}
              on:click={() => addTime(date)}>+</Button
            >
            {#if getTimes(times, date).length > 1}
              <Button
                tip={"Remove time slot" + allSame
                  ? ""
                  : ` from ${format(date, "MMMM do")}`}
                on:click={() => removeTime(date)}>-</Button
              >
            {/if}
          </div>
        </div>
      </td>
    </tr>
  {/each}
</table>

<datalist id="common-times">
  <option value="13:00" />
  <option value="19:30" />
</datalist>
