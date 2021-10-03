<script>
    import { createEventDispatcher } from "svelte";
    import tooltip from "../../lib/actions/tooltip";
  
    let classNames = null;
    export { classNames as class };
    export let color = null;
    export let tip = null;
    const dispatch = createEventDispatcher();
  </script>
  
  <button class={[color, classNames].join(" ")} on:click={() => dispatch("click")} use:tooltip={tip}>
    <slot />
  </button>
  
  <style>
    button {
      @apply rounded px-2 py-1 font-bold shadow-sm
             bg-gradient-to-b active:bg-gradient-to-t 
             from-gray-200 to-gray-300 
             hover:from-gray-600 hover:to-gray-900 hover:text-gray-200;
    }
  
    button.red {
      @apply from-red-200 to-red-300 
             hover:from-red-600 hover:to-red-900 hover:text-red-200;
    }
  </style>
  