<script>
  import { Link, Router } from "svelte-routing"
  import tooltip from "../../lib/actions/tooltip";
  // import { page } from "$app/stores";

  export let name;
  export let link = null;
  export let active = false;

  $: initials = !name ? "" : name[0].toUpperCase();
</script>

<div class="avatar">
  {#if link}
    <Link to={link}>
      <div use:tooltip={name} class:active>
        {initials}
      </div>
    </Link>
  {:else}
    <div use:tooltip={name} class:active>
      {initials}
    </div>
  {/if}
</div>

<style>
  .avatar div {
    @apply rounded-full w-10 h-10 
           flex items-center justify-center 
           cursor-default 
           bg-gray-500 text-gray-100 
           border-2 border-gray-100;
  }

  .avatar :global(a:hover) {
    @apply no-underline;
  }

  .avatar :global(a > div:not(.active)) {
    @apply cursor-pointer hover:border-blue-500;
  }

  .active {
    @apply border-green-500;
  }
</style>
