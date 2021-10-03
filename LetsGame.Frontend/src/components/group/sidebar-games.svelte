<script context="module">
  import gql from "graphql-tag";

  export const sidebarGamesFragment = gql`
    fragment sidebarGames on GroupGraphType {
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

  export let group;
</script>

<div class="grid grid-cols-2 gap-2">
  {#each group?.games || [] as game (game.id)}
    {#if game.igdbImageId}
      <div
        class="bg-cover bg-center w-full pb-[50%] rounded"
        style="background-image: url(https://images.igdb.com/igdb/image/upload/t_screenshot_med/{game.igdbImageId}.jpg)"
        use:tooltip={game.name}
      />
    {/if}
  {/each}
</div>
