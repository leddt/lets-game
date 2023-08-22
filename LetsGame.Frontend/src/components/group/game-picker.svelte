<script>
  import GameTile from "@/components/group/game-tile.svelte";

  export let games = [];
  export let game = null;

  const anyGame = {
    id: null,
    name: "Any game",
    description: "I don't care what we play, let's game!",
  };
  $: selectableGames = [...games, anyGame];
</script>

<div class="flex flex-wrap gap-4">
  {#each selectableGames as g (g.id)}
    <div class="w-80">
      <GameTile
        game={g}
        class="item {g === game ? 'selected' : ''}"
        on:click={() => (game = g)}
      />
    </div>
  {/each}
</div>
