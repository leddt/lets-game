<script>
  import { createEventDispatcher } from "svelte";

  let className = "";
  export let id = null;
  export let value = "";
  export let autofocus = false;
  export { className as class };

  const dispatch = createEventDispatcher();
</script>

<!-- svelte-ignore a11y-autofocus -->
<textarea
  {id}
  {autofocus}
  class={className}
  bind:value
  on:keydown={(e) => e.key === "Escape" && dispatch("escape")}
/>

<style lang="postcss">
  textarea {
    @apply border border-gray-400 
           bg-gray-100 text-gray-600 
           hover:bg-white
           focus:bg-white focus:ring-2 focus:ring-blue-500 focus:ring-opacity-25
           rounded shadow-inner outline-none 
           px-2 py-1;
  }
</style>
