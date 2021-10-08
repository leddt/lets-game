<script>
  import tooltip from "@/lib/actions/tooltip";
  import { createEventDispatcher } from "svelte";

  let classNames = "";
  export { classNames as class };
  export let tip;

  const dispatch = createEventDispatcher();
</script>

<button class={classNames} on:click={() => dispatch("click")} use:tooltip={tip}>
  <slot />
</button>

<style lang="postcss">
  button {
    @apply rounded-full w-7 h-7 shadow-sm  font-bold
           flex items-center justify-center
           bg-gradient-to-br active:bg-gradient-to-tl 
           border border-gray-300 hover:border-gray-900
           select-none
           from-gray-200 to-gray-300 
           hover:from-gray-600 hover:to-gray-900 hover:text-gray-200;
  }
</style>
