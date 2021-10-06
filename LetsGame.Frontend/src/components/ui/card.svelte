<script>
  import { createEventDispatcher } from "svelte";

  export let width = "w-80";
  export let imageHeight = "h-32";
  export let image = null;
  export let clickable = false;

  const dispatch = createEventDispatcher();
</script>

<div class="{width} card" class:clickable on:click={() => dispatch("click")}>
  {#if image}
    <div
      class="bg-cover bg-center {imageHeight} rounded-t"
      style="background-image: url({image})"
    />
  {/if}
  <div class="m-2 flex-grow">
    <slot />
  </div>
</div>

<style lang="postcss">
  .card {
    @apply rounded shadow flex flex-col bg-white;
  }
  .clickable {
    @apply cursor-pointer hover:shadow-xl hover:ring ring-opacity-100 ring-blue-300;
  }
</style>
