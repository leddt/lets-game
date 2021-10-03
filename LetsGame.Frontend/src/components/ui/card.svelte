<script>
    export let width = "w-80";
    export let imageHeight = "h-32";
    export let image = null;
  </script>
  
  <div class="{width} rounded shadow flex flex-col bg-white">
    {#if image}
      <div
        class="bg-cover bg-center {imageHeight} rounded-t"
        style="background-image: url({image})"
      />
    {/if}
    <div class="m-2 flex-grow">
      <slot />
    </div>
  </div>
  