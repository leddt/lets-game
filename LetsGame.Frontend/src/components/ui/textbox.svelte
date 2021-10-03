<script>
  let className = "";
  export let id = null;
  export let value = "";
  export { className as class };
</script>

<input type="text" {id} class={className} bind:value />

<style>
  input {
    @apply border border-gray-400 
           bg-gray-100 text-gray-600 
           hover:bg-white
           focus:bg-white focus:ring-2 focus:ring-blue-500 focus:ring-opacity-25
           rounded shadow-inner outline-none 
           px-2 py-1;
  }
</style>
