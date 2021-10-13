<script>
  export let title = "";
</script>

<div class="bg-gray-200 p-4 rounded-lg shadow-lg">
  <div class="mb-4 flex items-center justify-between">
    <h2>{title}</h2>
    <slot name="right" />
  </div>

  <slot />
</div>
