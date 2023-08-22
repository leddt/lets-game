<script>
  import { createEventDispatcher } from "svelte";
  import tooltip from "@/lib/actions/tooltip";

  export let tip = null;
  const dispatch = createEventDispatcher();
</script>

<button on:click={() => dispatch("click")} use:tooltip={tip}>
  <slot />
</button>

<style lang="postcss">
  button {
    @apply border-b border-transparent hover:border-b-current;
  }
</style>
