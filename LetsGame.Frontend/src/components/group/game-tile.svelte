<script>
  import tooltip from "@/lib/actions/tooltip";
  import { createEventDispatcher } from "svelte";

  let classNames = "";

  export let game;
  export let small = false;
  export { classNames as class };

  const dispatch = createEventDispatcher();
</script>

{#if game.igdbImageId}
  <div
    class="game-tile {classNames} {small ? 'small' : ''}"
    style="background-image: url(https://images.igdb.com/igdb/image/upload/t_screenshot_med/{game.igdbImageId}.jpg)"
    on:click={() => dispatch("click")}
  >
    <div>
      <p>{game.name}</p>
    </div>
    <div class="slot">
      <slot />
    </div>
  </div>
{:else}
  <div
    class="game-tile no-image {classNames} {small ? 'small' : ''}"
    on:click={() => dispatch("click")}
  >
    <div>
      <p>{game.name}</p>
      {#if game.description}
        <p class="italic">{game.description}</p>
      {/if}
    </div>
    <div class="slot">
      <slot />
    </div>
  </div>
{/if}

<style lang="postcss">
  .game-tile {
    @apply border bg-cover bg-center w-full pb-[50%] rounded relative;
  }

  .game-tile.no-image {
    @apply border-gray-600;
  }

  .game-tile div:not(.slot) {
    @apply absolute overflow-hidden px-1
           w-full h-full flex flex-col items-center justify-evenly
           text-2xl text-center leading-tight
           text-white font-bold
           bg-[radial-gradient(ellipse_at_center,rgba(0,0,0,0.5),transparent)];
  }

  .game-tile.small div:not(.slot) {
    @apply text-sm;
  }

  .game-tile.no-image div:not(.slot) {
    @apply text-sm text-black font-normal;
    background-image: none;
  }

  .game-tile .slot {
    @apply hidden absolute top-0 bottom-0 left-0 right-0;
  }

  .game-tile:hover .slot {
    @apply block;
  }
</style>
