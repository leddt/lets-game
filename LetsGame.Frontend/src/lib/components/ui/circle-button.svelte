<script>
  import tooltip from "$actions/tooltip";
  import { createEventDispatcher } from "svelte";

  export let tip;

  const dispatch = createEventDispatcher();
</script>

<button on:click={() => dispatch("click")} use:tooltip={tip}>
  <slot />
</button>

<style>
  button {
    @apply rounded-full w-7 h-7 shadow-sm  font-bold
           flex items-center justify-center
           bg-gradient-to-br active:bg-gradient-to-tl 
           from-gray-200 to-gray-300 
           hover:from-gray-600 hover:to-gray-900 hover:text-gray-200;
  }
</style>
