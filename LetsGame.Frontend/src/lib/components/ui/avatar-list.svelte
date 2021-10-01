<script>
  import Avatar from "$lib/components/ui/avatar.svelte";

  export let people;

  $: hasPeople = people?.length > 0;
</script>

<div class="flex items-center min-h-[2.5rem]">
  {#if hasPeople}
    <div class="flex flex-wrap mb-4 mr-4">
      {#each people as person}
        <div class="w-6 h-6 hover:z-10">
          <Avatar name={person.displayName} />
        </div>
      {/each}
    </div>
  {/if}

  <div class="flex-shrink-0" class:ml-2={hasPeople}>
    <slot />
  </div>
</div>
