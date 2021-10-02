<script>
  export let me;
  $: isLoggedIn = !!me;
</script>

<div class="p-4 flex flex-wrap justify-between items-center shadow-lg top-0 bg-gray-800">
  <a class="text-xl text-gray-100" href="/">Let's Game!</a>

  {#if isLoggedIn}
    <div class="flex flex-wrap gap-2">
      <a href="/" class="text-blue-300">{me.email}</a>
      <a href="/" class="text-blue-300">Logout</a>
    </div>
  {:else}
    <div>
      <a href="/" class="text-blue-300">Login</a>
    </div>
  {/if}
</div>
