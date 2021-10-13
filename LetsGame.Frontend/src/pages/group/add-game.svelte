<script>
  import gql from "graphql-tag";
  import { mutation } from "svelte-apollo";
  import { useNavigate } from "svelte-navigator";

  import Section from "@/components/ui/section.svelte";
  import Textbox from "@/components/ui/textbox.svelte";
  import Button from "@/components/ui/button.svelte";
  import CardList from "@/components/ui/card-list.svelte";
  import Card from "@/components/ui/card.svelte";
  import client from "@/lib/apollo";

  export let groupId;

  const navigate = useNavigate();

  let searchTerm = "";
  let searchResults = null;

  async function searchGames() {
    const { data } = await client.query({
      query: gql`
        query SearchGames($term: String!) {
          searchGames(term: $term) {
            id
            name
            igdbImageId
          }
        }
      `,
      variables: {
        term: searchTerm,
      },
    });

    searchResults = data.searchGames;
  }

  function getImage(game) {
    return game.igdbImageId
      ? `https://images.igdb.com/igdb/image/upload/t_screenshot_med/${game.igdbImageId}.jpg`
      : null;
  }

  const addGameMutation = mutation(gql`
    mutation AddGame($groupId: ID!, $gameId: ID!) {
      addGame(groupId: $groupId, gameId: $gameId) {
        group {
          id
          games {
            id
            name
            igdbImageId
          }
        }
      }
    }
  `);

  async function addGame(gameId) {
    await addGameMutation({
      variables: {
        groupId,
        gameId,
      },
    });

    navigate("../");
  }
</script>

<div class="p-4 flex-grow">
  <Section title="Add game">
    <form on:submit|preventDefault={searchGames}>
      <label for="searchTerm">Game name:</label>
      <Textbox id="searchTerm" bind:value={searchTerm} />
      <Button submit>Search</Button>
    </form>

    {#if searchResults}
      <div class="pt-4">
        {#if searchResults.length > 0}
          <CardList>
            {#each searchResults as game (game.id)}
              <Card
                image={getImage(game)}
                clickable
                on:click={() => addGame(game.id)}
              >
                <strong>{game.name}</strong>
              </Card>
            {/each}
          </CardList>
        {:else}
          <p>No game found</p>
        {/if}
      </div>
    {/if}
  </Section>
</div>
