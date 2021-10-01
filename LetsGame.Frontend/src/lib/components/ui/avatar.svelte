<script>
  import tooltip from "$actions/tooltip";
  import { page } from "$app/stores";

  export let name;
  export let link = null;

  $: initials = !name ? "" : name[0].toUpperCase();
  $: active = link && $page.path === link;
</script>

{#if link}
  <a href={link}>
    <div use:tooltip={name} class:active>
      {initials}
    </div>
  </a>
{:else}
  <div use:tooltip={name} class:active>
    {initials}
  </div>
{/if}

<style>
  div {
    @apply rounded-full w-10 h-10 
           flex items-center justify-center 
           cursor-default 
           bg-gray-500 text-gray-100 
           border-2 border-gray-100;
  }

  a > div:not(.active) {
    @apply cursor-pointer hover:border-blue-500;
  }

  .active {
    @apply border-green-500;
  }
</style>
