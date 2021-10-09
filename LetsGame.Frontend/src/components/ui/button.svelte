<script>
  import { createEventDispatcher } from "svelte";
  import tooltip from "@/lib/actions/tooltip";

  let classNames = null;
  export { classNames as class };
  export let color = null;
  export let tip = null;
  export let submit = false;
  const dispatch = createEventDispatcher();

  $: buttonType = submit ? "submit" : "button";
</script>

<button
  type={buttonType}
  class={[color, classNames].join(" ")}
  on:click={() => dispatch("click")}
  use:tooltip={tip}
>
  <slot />
</button>

<style lang="postcss">
  button {
    @apply border rounded px-2 py-1 font-semibold
           bg-gradient-to-b active:bg-gradient-to-t 
           select-none
           
           from-gray-200 to-gray-300 border-gray-300 text-gray-800
           hover:from-gray-600 hover:to-gray-900 hover:text-gray-200 hover:border-gray-900;
  }

  button.red {
    @apply from-red-200 to-red-300 border-red-300
           hover:from-red-600 hover:to-red-900 hover:text-red-200 hover:border-red-900;
  }

  button.green {
    @apply from-green-200 to-green-300 border-green-300
           hover:from-green-600 hover:to-green-900 hover:text-green-200 hover:border-green-900;
  }
</style>
