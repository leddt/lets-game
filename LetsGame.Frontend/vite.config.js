import { resolve } from "path";
import { defineConfig } from "vite";
import { svelte } from "@sveltejs/vite-plugin-svelte";
import preprocess from "svelte-preprocess";

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    port: process.env.PORT || 3000,
  },
  build: {
    outDir: resolve(__dirname, "../LetsGame.Web/ClientApp"),
  },
  plugins: [
    svelte({
      preprocess: [
        preprocess({
          postcss: true,
        }),
      ],
    }),
  ],
  resolve: {
    alias: {
      "@": resolve(__dirname, "./src"),
    },
  },
});
