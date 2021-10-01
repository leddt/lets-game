<script>
  export let me;
  $: isLoggedIn = !!me;
</script>

<div
  class="border-b p-4 flex justify-between items-center sm:sticky top-0 bg-white bg-opacity-90 backdrop-blur-xl sm:shadow-sm"
>
  <a class="text-xl text-gray-800" href="/">Let's Game!</a>

  {#if isLoggedIn}
    <div class="flex gap-2">
      <a href="/">{me.email}</a>
      <a href="/">Logout</a>
    </div>
  {:else}
    <div>
      <a href="/">Login</a>
    </div>
  {/if}
</div>
