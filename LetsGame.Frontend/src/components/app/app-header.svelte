<script>
  import { Link } from "svelte-routing"

  export let me;
  $: isLoggedIn = !!me;
</script>

<div class="p-4 flex flex-wrap justify-between items-center shadow-lg top-0 bg-gray-800">
  <Link class="text-xl text-gray-100" to="/">Let's Game!</Link>

  {#if isLoggedIn}
    <div class="flex flex-wrap gap-2">
      <Link to="/" class="text-blue-300">{me.email}</Link>
      <Link to="/" class="text-blue-300">Logout</Link>
    </div>
  {:else}
    <div>
      <Link to="/" class="text-blue-300">Login</Link>
    </div>
  {/if}
</div>
