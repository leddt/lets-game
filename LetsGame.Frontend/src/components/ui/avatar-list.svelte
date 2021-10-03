<script>
    import { scale } from "svelte/transition";
    import { flip } from "svelte/animate";
  
    import Avatar from "./avatar.svelte";
  
    export let people;
  
    $: hasPeople = people?.length > 0;
  </script>
  
  <div class="flex items-center min-h-[2.5rem]">
    <div class="flex-shrink-0 mr-2">
      <slot />
    </div>
  
    {#if hasPeople}
      <div class="flex flex-wrap mb-4 mr-4" transition:scale|local>
        {#each people as person (person.id)}
          <div class="w-6 h-6 hover:z-10" transition:scale|local animate:flip>
            <Avatar name={person.displayName} />
          </div>
        {/each}
      </div>
    {/if}
  </div>
  