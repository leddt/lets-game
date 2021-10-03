<script>
  import tooltip from "@/lib/actions/tooltip";
  import { useLocation } from "svelte-navigator";

  export let name;
  export let link = null;
  export let active = false;

  const location = useLocation();

  $: if (link) {
    console.log($location.pathname, link, $location.pathname === link);
  }

  $: initials = !name ? "" : name[0].toUpperCase();
  $: highlighted = active || (link && $location.pathname === link);
</script>

{#if link}
  <a href={link}>
    <div use:tooltip={name} class:highlighted>
      {initials}
    </div>
  </a>
{:else}
  <div use:tooltip={name} class:highlighted>
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

  a:hover {
    @apply no-underline;
  }

  a > div:not(.highlighted) {
    @apply cursor-pointer hover:border-blue-500;
  }

  .highlighted {
    @apply border-green-500;
  }
</style>
