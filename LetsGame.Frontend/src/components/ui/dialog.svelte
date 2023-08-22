<script>
  let dialog;

  export function showModal() {
    dialog.showModal();
  }

  export function close() {
    dialog.close();
  }
</script>

<dialog bind:this={dialog}>
  <slot {close} />
</dialog>

<style lang="postcss">
  dialog {
    @apply border border-black rounded-lg p-16 drop-shadow-xl;
  }

  dialog::backdrop {
    @apply bg-gray-700 opacity-50;
  }
</style>
