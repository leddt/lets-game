<script>
  import Avatar from "$lib/components/ui/avatar.svelte";

  export let groups;
</script>

<div class="border-b sm:border-b-0 sm:border-r p-4 flex flex-row sm:flex-col gap-2">
  {#each groups as group}
    <Avatar name={group.name} link="/group/{group.slug}" />
  {/each}
</div>
