<script>
    export let color = "bg-gray-100";
    export let padding = "p-2";
    export let title = "";
  
    let classNames = "";
    export { classNames as class };
  </script>
  
  <div class="rounded-md {color} {padding} {classNames}">
    <div class="text-xs text-center font-bold">{title}</div>
    <slot />
  </div>
  