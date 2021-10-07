<script context="module">
  import gql from "graphql-tag";

  export const sidebarGamesFragment = gql`
    fragment sidebarGames on GroupGraphType {
      self {
        id
        role
      }
      games {
        id
        name
        igdbImageId
      }
    }
  `;
</script>

<script>
  import tooltip from "@/lib/actions/tooltip";
  import Button from "@/components/ui/button.svelte";
  import { mutation } from "svelte-apollo";

  export let group;

  $: isOwner = group?.self.role === "OWNER";

  const removeGameMutation = mutation(gql`
    ${sidebarGamesFragment}
    mutation RemoveGame($groupId: ID!, $gameId: ID!) {
      removeGame(groupId: $groupId, gameId: $gameId) {
        group {
          id
          ...sidebarGames
        }
      }
    }
  `);

  function removeGame(game) {
    if (!confirm(`Remove ${game.name} and all it's events from the group?`))
      return;

    removeGameMutation({
      variables: {
        groupId: group.id,
        gameId: game.id,
      },
    });
  }
</script>

<div class="grid grid-cols-2 gap-2">
  {#each group?.games || [] as game (game.id)}
    {#if game.igdbImageId}
      <div
        class="game"
        style="background-image: url(https://images.igdb.com/igdb/image/upload/t_screenshot_med/{game.igdbImageId}.jpg)"
        use:tooltip={game.name}
      >
        {#if isOwner}
          <Button color="red" on:click={() => removeGame(game)}>&times;</Button>
        {/if}
      </div>
    {:else}
      <div class="game no-image">
        <span>{game.name}</span>
        {#if isOwner}
          <Button color="red" on:click={() => removeGame(game)}>&times;</Button>
        {/if}
      </div>
    {/if}
  {/each}
</div>

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

  .game :global(button) {
    @apply hidden absolute bottom-1 right-1;
  }

  .game:hover :global(button) {
    @apply block;
  }
</style>
