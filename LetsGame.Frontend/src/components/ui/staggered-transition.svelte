<script>
  import { onMount } from "svelte";

  export let key = undefined;
  export let delay = 0;
  export let increment = 75;
  export let duration = 200;
  export let start = 0.95;

  let nextIndex = 0;

  // Create a function to get transition params
  const getTransition = () => ({
    duration,
    delay: delay + increment * nextIndex++,
    start,
  });

  // Reset counter when mounted
  onMount(() => {
    nextIndex = 0;
  });

  // Reset counter when key changes
  $: if (key) {
    nextIndex = 0;
  }
</script>

{#key key}
  <div class="flex flex-col gap-4">
    <slot {getTransition} />
  </div>
{/key}
