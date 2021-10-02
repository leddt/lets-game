<script>
  import Avatar from "$lib/components/ui/avatar.svelte";

  export let groups;
</script>

<div
  class="p-4 bg-gray-700 flex flex-wrap flex-row gap-2 sm:writing-mode-vertical sm:text-orientation-upright"
>
  {#each groups as group}
    <Avatar name={group.name} link="/group/{group.slug}" />
  {/each}
</div>
