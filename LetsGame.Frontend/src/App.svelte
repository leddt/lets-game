<script>
  import { setClient, query } from "svelte-apollo";
  import { Router, Route, links } from "svelte-navigator";
  import gql from "graphql-tag";
  import { me } from "./lib/store";
  import client from "./lib/apollo";
  import AppHeader from "./components/app/app-header.svelte";
  import AppSidebar from "./components/app/app-sidebar.svelte";

  import HomePage from "./pages/home.svelte";
  import GroupPage from "./pages/group/group.svelte";
  import CreateGroupPage from "./pages/create-group.svelte";

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
  <Router primary={false}>
    {#if $layoutData.data}
      <div class="flex flex-col h-screen">
        <AppHeader me={$me} />

        <div
          class="flex flex-col sm:flex-row flex-grow flex-shrink sm:overflow-y-hidden"
        >
          <AppSidebar {groups} />

          <Route path="/">
            <HomePage {groups} />
          </Route>

          <Route path="/create-group">
            <CreateGroupPage on:added={() => layoutData.refetch()} />
          </Route>

          <Route path="group/:slug/*" let:params>
            <GroupPage
              on:deleted={() => layoutData.refetch()}
              slug={params.slug}
            />
          </Route>
        </div>
      </div>
    {/if}
  </Router>
</div>
