<script>
  import {
    addDays,
    addMonths,
    format,
    getDate,
    getDay,
    getMonth,
    isSameDay,
    previousDay,
    startOfMonth,
  } from "date-fns";
  import sortBy from "lodash/sortBy";

  const today = new Date();
  let firstDay = startOfMonth(today);
  export let pickedDates = [];

  $: currentMonth = getMonth(firstDay);
  $: currentMonthName = format(firstDay, "MMMM yyyy");
  $: weeks = getWeeks(firstDay, pickedDates);

  function selectMonth(offset) {
    firstDay = addMonths(firstDay, offset);
  }

  function getWeeks(firstDay, pickedDates) {
    const results = [];

    let currentDay = firstDay;
    if (getDay(currentDay) > 0) {
      currentDay = previousDay(currentDay, 0);
    }

    do {
      const week = [];
      results.push(week);

      do {
        week[getDay(currentDay)] = {
          isOtherMonth: getMonth(currentDay) !== currentMonth,
          date: currentDay,
          dayNumber: getDate(currentDay),
          selected: pickedDates.find((x) => isSameDay(x, currentDay)),
          today: isSameDay(today, currentDay),
        };
        currentDay = addDays(currentDay, 1);
      } while (getDay(currentDay) > 0);
    } while (getMonth(currentDay) === currentMonth);

    return results;
  }

  function toggleDate(date) {
    if (pickedDates.find((x) => isSameDay(x, date))) {
      pickedDates = pickedDates.filter((x) => !isSameDay(x, date));
    } else {
      pickedDates = sortBy([...pickedDates, date]);
    }
  }
</script>

<div>
  <table>
    <thead>
      <tr>
        <td><button on:click={() => selectMonth(-1)}>&lt;</button></td>
        <td colspan="5">{currentMonthName}</td>
        <td><button on:click={() => selectMonth(+1)}>&gt;</button></td>
      </tr>
      <tr>
        <td>S</td>
        <td>M</td>
        <td>T</td>
        <td>W</td>
        <td>T</td>
        <td>F</td>
        <td>S</td>
      </tr>
    </thead>
    <tbody>
      {#each weeks as week}
        <tr>
          {#each week as day}
            <td
              class="day"
              class:other-month={day.isOtherMonth}
              class:selected={day.selected}
              class:today={day.today}
              on:click={() => toggleDate(day.date)}>{day.dayNumber}</td
            >
          {/each}
        </tr>
      {/each}
    </tbody>
  </table>
</div>

<style lang="postcss">
  table {
    @apply w-full;
  }
  thead td {
    @apply text-center bg-gray-700 text-gray-100;
  }
  thead button {
    @apply w-full;
  }
  td {
    @apply border border-gray-700 p-1 w-[calc(100%/7)];
  }
  td.day {
    @apply bg-white text-right cursor-pointer;
  }
  td.today {
    @apply font-bold bg-blue-100;
  }
  td.other-month {
    @apply text-gray-500 bg-gray-100;
  }
  td.selected {
    @apply bg-green-200;
  }
</style>
