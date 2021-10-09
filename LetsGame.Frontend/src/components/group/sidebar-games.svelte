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
  import Button from "@/components/ui/button.svelte";
  import GameTile from "./game-tile.svelte";
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

{#if group?.games.length === 0}
  There are no games... yet!
{:else}
  <div class="grid grid-cols-2 gap-2">
    {#each group?.games || [] as game (game.id)}
      <GameTile {game}>
        {#if isOwner}
          <Button
            class="absolute bottom-1 right-1"
            color="red"
            on:click={() => removeGame(game)}>&times;</Button
          >
        {/if}
      </GameTile>
    {/each}
  </div>
{/if}
