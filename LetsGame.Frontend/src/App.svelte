<script>
  import { setClient, query } from "svelte-apollo";
  import { Router, Route, links } from "svelte-navigator";
  import gql from "graphql-tag";
  import { me } from "./lib/store";
  import client from "./lib/apollo";
  import AppHeader from "./components/app/app-header.svelte";
  import AppSidebar from "./components/app/app-sidebar.svelte";

  import GroupIndex from "./pages/group/index.svelte";

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

  $: groups = $layoutData.data?.groups;
  $: me.set($layoutData.data?.me);
</script>

<div use:links>
  <Router>
    {#if $layoutData.data}
      <div class="flex flex-col h-screen">
        <AppHeader me={$me} />

        <div
          class="flex flex-col sm:flex-row flex-grow flex-shrink overflow-y-hidden"
        >
          <AppSidebar {groups} />

          <Route path="group/:slug" let:params>
            <GroupIndex slug={params.slug} />
          </Route>
          <!-- <slot /> -->
        </div>
      </div>
    {/if}
  </Router>
</div>
