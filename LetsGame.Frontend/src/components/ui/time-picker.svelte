<script>
  let className = "";
  export let id = null;
  export let value = "";
  export { className as class };
</script>

<input type="time" {id} class={className} bind:value />

<style lang="postcss">
  input {
    @apply border border-gray-400 
           bg-gray-100 text-gray-600 
           hover:bg-white
           focus:bg-white focus:ring-2 focus:ring-blue-500 focus:ring-opacity-25
           rounded shadow-inner outline-none 
           p-1;
  }
</style>
