import preprocess from "svelte-preprocess";
import { resolve } from "path";

/** @type {import('@sveltejs/kit').Config} */
const config = {
  // Consult https://github.com/sveltejs/svelte-preprocess
  // for more information about preprocessors
  preprocess: preprocess({
    defaults: {
      style: "postcss"
    },
    postcss: true
  }),

  kit: {
    // hydrate the <div id="svelte"> element in src/app.html
    target: "#svelte",
    vite: {
      resolve: {
        alias: {
          $actions: resolve("./src/actions")
        }
      }
    },
    ssr: false
  }
};

export default config;
