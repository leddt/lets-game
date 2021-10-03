<script>
  import tooltip from "@/lib/actions/tooltip";
  import { useLocation } from "svelte-navigator";

  export let name;
  export let link = null;
  export let active = false;

  const location = useLocation();

  $: initials = getInitials(name);
  $: backgroundColor = getBackgroundColor(name);

  $: highlighted = active || (link && $location.pathname.startsWith(link));

  function getInitials(text) {
    return (text || "")
      .split(" ")
      .map((x) => x[0].toUpperCase())
      .slice(0, 3)
      .join();
  }

  function getBackgroundColor(text) {
    var hash = 0;
    for (var i = 0; i < text.length; i++) {
      hash = text.charCodeAt(i) + ((hash << 5) - hash);
    }

    var hue = hash % 360;
    return `hsl(${hue}, 60%, 40%)`;
  }
</script>

{#if link}
  <a href={link}>
    <div
      use:tooltip={name}
      class:highlighted
      style="background-color: {backgroundColor};"
    >
      {initials}
    </div>
  </a>
{:else}
  <div
    use:tooltip={name}
    class:highlighted
    style="background-color: {backgroundColor};"
  >
    {initials}
  </div>
{/if}

<style>
  div {
    @apply rounded-full w-10 h-10 
           flex items-center justify-center 
           cursor-default 
           text-sm
           bg-gray-500 text-gray-100 
           border-2 border-gray-300;
  }

  a:hover {
    @apply no-underline;
  }

  a > div:not(.highlighted) {
    @apply cursor-pointer hover:border-blue-300;
  }

  .highlighted {
    @apply border-green-300;
  }
</style>
