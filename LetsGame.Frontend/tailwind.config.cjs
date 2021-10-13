module.exports = {
  mode: "jit",
  purge: ["./src/**/*.svelte"],
  darkMode: false, // or 'media' or 'class'
  theme: {
    extend: {
      spacing: {
        158: "39.5rem",
        164: "41rem"
      }
    }
  },
  variants: {
    extend: {}
  },
  plugins: []
};
