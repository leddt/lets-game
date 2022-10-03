<script>
  import { createEventDispatcher } from "svelte";

  const dispatch = createEventDispatcher();
  let className = "";

  export let id = null;
  export let value = "";
  export let step = 60 * 30; // thirty minutes
  export { className as class };
</script>

<input
  type="time"
  {id}
  class={className}
  {step}
  bind:value
  on:change={(ev) => dispatch("change", ev)}
  {...$$restProps}
/>

<style lang="postcss">
  input {
    @apply border border-gray-400 
           bg-gray-100 text-gray-600 
           hover:bg-white
           focus:bg-white focus:ring-2 focus:ring-blue-500 focus:ring-opacity-25
           rounded shadow-inner outline-none 
           p-1;
  }
</style>
