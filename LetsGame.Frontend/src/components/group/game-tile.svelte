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
    class="game {classNames}"
    style="background-image: url(https://images.igdb.com/igdb/image/upload/t_screenshot_med/{game.igdbImageId}.jpg)"
    use:tooltip={game.name}
    on:click={() => dispatch("click")}
  >
    <slot />
  </div>
{:else}
  <div class="game no-image {classNames}" on:click={() => dispatch("click")}>
    <span>{game.name}</span>
    <slot />
  </div>
{/if}

<style lang="postcss">
  .game {
    @apply border bg-cover bg-center w-full pb-[50%] rounded relative;
  }

  .game.no-image {
    @apply border-gray-600;
  }

  .game.no-image span {
    @apply absolute overflow-hidden px-1
           w-full h-full flex items-center justify-center
           text-sm text-center leading-tight;
  }
</style>
