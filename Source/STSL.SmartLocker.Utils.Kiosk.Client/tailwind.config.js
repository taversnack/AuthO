/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors:{
        'offwhite':'#e1e1e1',
        'gray':'#354a60',
        'dark':'#2d3e50'
      }
    },
  },
  plugins: [],
}