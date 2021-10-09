<script>
  import tooltip from "@/lib/actions/tooltip";
  import { createEventDispatcher } from "svelte";

  let classNames = "";

  export let game;
  export { classNames as class };

  const dispatch = createEventDispatcher();
</script>

{#if game.igdbImageId}
  <div
    class="game-tile {classNames}"
    style="background-image: url(https://images.igdb.com/igdb/image/upload/t_screenshot_med/{game.igdbImageId}.jpg)"
    use:tooltip={game.name}
    on:click={() => dispatch("click")}
  >
    <div class="slot">
      <slot />
    </div>
  </div>
{:else}
  <div
    class="game-tile no-image {classNames}"
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

  .game-tile.no-image div {
    @apply absolute overflow-hidden px-1
           w-full h-full flex flex-col items-center justify-evenly
           text-sm text-center leading-tight;
  }

  .game-tile .slot {
    @apply hidden absolute top-0 bottom-0 left-0 right-0;
  }

  .game-tile:hover .slot {
    @apply block;
  }
</style>
