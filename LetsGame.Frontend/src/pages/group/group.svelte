<script>
  import gql from "graphql-tag";
  import { createEventDispatcher, onDestroy } from "svelte";
  import { query, subscribe } from "svelte-apollo";
  import { Route, useLocation, useNavigate } from "svelte-navigator";

  import { upcomingSessionCardFragment } from "@/components/group/upcoming-session-card.svelte";
  import { proposedSessionCardFragment } from "@/components/group/proposed-session-card.svelte";
  import { sidebarFragment } from "@/components/group/group-sidebar.svelte";

  import CardList from "@/components/ui/card-list.svelte";
  import Section from "@/components/ui/section.svelte";
  import StaggeredTransition from "@/components/ui/staggered-transition.svelte";
  import Button from "@/components/ui/button.svelte";
  import UpcomingSessionCard from "@/components/group/upcoming-session-card.svelte";
  import ProposedSessionCard from "@/components/group/proposed-session-card.svelte";
  import GroupSidebar from "@/components/group/group-sidebar.svelte";

  import GroupAddGame from "./add-game.svelte";
  import GroupProposeEvent from "./propose-event.svelte";

  import { copyToClipboard } from "@/lib/clipboard";
  import client from "@/lib/apollo";
  import { scale } from "svelte/transition";

  export let slug;

  const navigate = useNavigate();
  const location = useLocation();
  const dispatch = createEventDispatcher();

  $: groupData = query(
    gql`
      ${upcomingSessionCardFragment}
      ${proposedSessionCardFragment}
      ${sidebarFragment}
      query GroupData($slug: String!) {
        groupBySlug(slug: $slug) {
          id
          name
          sharingKey
          slug
          games {
            id
          }
          self {
            id
            role
          }
          upcomingSessions {
            ...upcomingSessionCard
          }
          proposedSessions {
            ...proposedSessionCard
          }
          ...sidebar
        }
      }
    `,
    {
      variables: {
        slug,
      },
    },
  );

  $: groupData.refetch();
  $: group = $groupData.data?.groupBySlug;
  $: mainGroupPage = `/group/${slug}`;
  $: isMainGroupPage = $location.pathname === mainGroupPage;
  $: icalLink = `${window.location.origin}/group/${slug}.ics?k=${group?.sharingKey}`;
  $: isOwner = group?.self.role === "OWNER";
  $: key = $location.pathname;

  $: if (group) {
    const unsubscribe = subscribe(
      gql`
        ${upcomingSessionCardFragment}
        ${proposedSessionCardFragment}
        ${sidebarFragment}
        subscription WatchGroup($groupId: ID!) {
          groupUpdated(groupId: $groupId) {
            upcomingSessions {
              ...upcomingSessionCard
            }
            proposedSessions {
              ...proposedSessionCard
            }
            ...sidebar
          }
        }
      `,
      {
        variables: {
          groupId: group.id,
        },
      },
    ).subscribe(() => {});

    onDestroy(unsubscribe);
  }

  async function deleteGroup() {
    if (!confirm(`Delete ${group.name} forever?`)) return;

    await client.mutate({
      mutation: gql`
        mutation DeleteGroup($groupId: ID!) {
          deleteGroup(groupId: $groupId)
        }
      `,
      variables: {
        groupId: group.id,
      },
    });

    dispatch("deleted");
    navigate("/");
  }

  async function leaveGroup() {
    if (
      !confirm(`Leave ${group.name}? You'll need an invite link to join again.`)
    ) {
      return;
    }

    await client.mutate({
      mutation: gql`
        mutation LeaveGroup($groupId: ID!) {
          leaveGroup(groupId: $groupId)
        }
      `,
      variables: {
        groupId: group.id,
      },
    });

    navigate("/");
  }
</script>

<div class="flex flex-col flex-grow min-w-0 sm:overflow-y-scroll">
  {#if $groupData.data}
    <div
      class="p-4 bg-gray-600 text-gray-100 flex flex-col sm:flex-row items-start sm:items-center justify-between"
    >
      <h1>{group?.name}</h1>
      <StaggeredTransition {key} let:getTransition>
        <div class="flex gap-2 flex-row-reverse">
          {#if isMainGroupPage}
            <span in:scale={getTransition()}>
              {#if isOwner}
                <Button color="red" on:click={deleteGroup}>Delete group</Button>
              {:else}
                <Button color="red" on:click={leaveGroup}>Leave group</Button>
              {/if}
            </span>
            <span in:scale={getTransition()}>
              <Button on:click={() => copyToClipboard(icalLink)}>
                Copy iCal URL
              </Button>
            </span>
          {:else}
            <span in:scale={getTransition()}>
              <Button on:click={() => navigate(mainGroupPage)}>
                Back to group page
              </Button>
            </span>
          {/if}
        </div>
      </StaggeredTransition>
    </div>

    <div class="flex flex-col sm:flex-row flex-grow">
      <Route path="/">
        <div class="p-4 flex flex-col flex-grow gap-4 min-w-0">
          {#if group?.games.length === 0}
            <div class="text-xl text-gray-100">
              Welcome to your new group!
              <a href="/group/{group.slug}/add-game" class="text-blue-300">
                Add a game
              </a>
              to get started.
            </div>
          {:else}
            <StaggeredTransition {key} let:getTransition>
              {#if group?.upcomingSessions?.length > 0}
                <div in:scale={getTransition()}>
                  <Section
                    title="Upcoming sessions ({group.upcomingSessions.length})"
                  >
                    <CardList>
                      {#each group.upcomingSessions as s (s.id)}
                        <UpcomingSessionCard session={s} />
                      {/each}
                    </CardList>
                  </Section>
                </div>
              {/if}

              {#if group?.proposedSessions}
                <div in:scale={getTransition()}>
                  <Section
                    title={"Proposed sessions" +
                      (group.proposedSessions.length > 0
                        ? ` (${group.proposedSessions.length})`
                        : "")}
                  >
                    <Button
                      color="green"
                      slot="right"
                      on:click={() => navigate("propose-event")}
                    >
                      Propose new session
                    </Button>

                    {#if group.proposedSessions.length > 0}
                      <CardList>
                        {#each group.proposedSessions as s (s.id)}
                          <ProposedSessionCard session={s} {group} />
                        {/each}
                      </CardList>
                    {:else}
                      <p>No session is being planned right now.</p>
                    {/if}
                  </Section>
                </div>
              {/if}
            </StaggeredTransition>
          {/if}
        </div>

        <GroupSidebar {group} />
      </Route>

      <Route path="add-game"><GroupAddGame groupId={group.id} /></Route>
      <Route path="propose-event"><GroupProposeEvent {group} /></Route>
    </div>
  {/if}
</div>
