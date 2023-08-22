<script>
  import { createEventDispatcher } from "svelte";
  import tooltip from "@/lib/actions/tooltip";

  let classNames = null;
  export { classNames as class };
  export let tip = null;
  const dispatch = createEventDispatcher();
</script>

<button
  type="button"
  class={classNames}
  on:click={() => dispatch("click")}
  use:tooltip={tip}
>
  <slot />
</button>

<style lang="postcss">
  button {
    @apply border-b border-transparent hover:border-b-current;
  }
</style>
