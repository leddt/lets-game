<script>
  import { setContext } from "svelte";
  import { setClient, query } from "svelte-apollo";
  import gql from "graphql-tag";
  import client from "$lib/apollo";
  import AppHeader from "$lib/components/app/app-header.svelte";
  import AppSidebar from "$lib/components/app/app-sidebar.svelte";
  import "../styles/tailwind.css";

  setClient(client);

  const layoutData = query(gql`
    query LayoutData {
      me {
        id
        email
      }
      groups {
        id
        slug
        name
      }
    }
  `);

  $: me = $layoutData.data?.me;
  $: groups = $layoutData.data?.groups;

  $: setContext("me", me);
</script>

{#if $layoutData.data}
  <div class="flex flex-col h-screen">
    <AppHeader {me} />

    <div class="flex flex-col sm:flex-row flex-grow flex-shrink overflow-y-hidden">
      <AppSidebar {groups} />
      <slot />
    </div>
  </div>
{/if}
