<script>
  export let title = "";
</script>

<div class="bg-gray-50 p-4 rounded-lg">
  <h2 class="mb-2 pb-2 border-b">{title}</h2>
  <slot />
</div>
