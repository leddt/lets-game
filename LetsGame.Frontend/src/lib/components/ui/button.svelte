<script>
  import { createEventDispatcher } from "svelte";
  const dispatch = createEventDispatcher();
</script>

<button on:click={() => dispatch("click")}>
  <slot />
</button>

<style>
  button {
    @apply rounded px-2 py-1 font-bold shadow-sm
           bg-gradient-to-b active:bg-gradient-to-t 
           from-gray-200 to-gray-300 
           hover:from-gray-600 hover:to-gray-900 hover:text-gray-200;
  }
</style>
