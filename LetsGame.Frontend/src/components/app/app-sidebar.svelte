<script>
  import Avatar from "@/components/ui/avatar.svelte";
  import CircleButton from "@/components/ui/circle-button.svelte";
  import { useNavigate } from "svelte-navigator";

  const navigate = useNavigate();

  export let groups;
</script>

<div
  class="p-4 bg-gray-700 flex items-center flex-wrap flex-row gap-2 sm:writing-mode-vertical sm:text-orientation-upright"
>
  {#each groups as group (group.id)}
    <div class="writing-mode-horizontal">
      <Avatar id={group.id} name={group.name} link="/group/{group.slug}" />
    </div>
  {/each}

  <CircleButton
    class="ml-4 sm:ml-0 sm:mt-4"
    tip="Create a new group"
    on:click={() => navigate("/create-group")}
  >
    +
  </CircleButton>
</div>
