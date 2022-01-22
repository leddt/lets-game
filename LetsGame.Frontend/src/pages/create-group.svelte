<script>
  import gql from "graphql-tag";
  import { createEventDispatcher } from "svelte";
  import { useNavigate } from "svelte-navigator";

  import client from "@/lib/apollo";

  import Button from "@/components/ui/button.svelte";
  import Textbox from "@/components/ui/textbox.svelte";

  let groupName = "";
  let displayName = "";

  const navigate = useNavigate();
  const dispatch = createEventDispatcher();

  async function createGroup() {
    const { data } = await client.mutate({
      mutation: gql`
        mutation CreateGroup($groupName: String!, $displayName: String!) {
          createGroup(groupName: $groupName, displayName: $displayName) {
            group {
              id
              slug
            }
          }
        }
      `,
      variables: {
        groupName,
        displayName,
      },
    });

    dispatch("added");
    navigate(`/group/${data.createGroup.group.slug}`);
  }
</script>

<div class="flex-grow p-4">
  <div class="flex-grow sm:overflow-y-auto">
    <div class="p-4 max-w-2xl m-auto">
      <div class="text-gray-100 text-xl">
        <div class="text-6xl font-thin mb-16">Create new group</div>

        <form on:submit|preventDefault={createGroup}>
          <div class="form-group">
            <label class="block" for="group-name">Group name</label>
            <Textbox
              id="group-name"
              class="w-full"
              placeholder="The public name for this group"
              required
              bind:value={groupName}
            />
          </div>
          <div class="form-group">
            <label class="block" for="display-name">Your display name</label>
            <Textbox
              id="display-name"
              class="w-full"
              placeholder="Your nickname in this group"
              required
              bind:value={displayName}
            />
          </div>

          <Button submit>Create group</Button>
        </form>
      </div>
    </div>
  </div>
</div>

<style lang="postcss">
  .form-group {
    @apply mb-8;
  }
  .form-group label {
    @apply mb-2;
  }
</style>
