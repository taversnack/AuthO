/** @type {import('tailwindcss').Config} */
// const colors = require('tailwindcss/colors');

// Material colours:
const materialPalettes = {
  blue: {
    50: "#e3f2fd",
    100: "#bbdefb",
    200: "#90caf9",
    300: "#64b5f6",
    400: "#42a5f5",
    500: "#2196f3",
    600: "#1e88e5",
    700: "#1976d2",
    800: "#1565c0",
    900: "#0d47a1",
    A100: "#82B1FF",
    A200: "#448AFF",
    A400: "#2979FF",
    A700: "#2962FF",
  },
  green: {
    50: "#E8F5E9",
    100: "#C8E6C9",
    200: "#A5D6A7",
    300: "#81C784",
    400: "#66BB6A",
    500: "#4CAF50",
    600: "#43A047",
    700: "#388E3C",
    800: "#2E7D32",
    900: "#1B5E20",
    A100: "#B9F6CA",
    A200: "#69F0AE",
    A400: "#00E676",
    A700: "#00C853",
  },
  red: {
    50: "#ffebee",
    100: "#ffcdd2",
    200: "#ef9a9a",
    300: "#e57373",
    400: "#ef5350",
    500: "#f44336",
    600: "#e53935",
    700: "#d32f2f",
    800: "#c62828",
    900: "#b71c1c",
    A100: "#FF8A80",
    A200: "#FF5252",
    A400: "#FF1744",
    A700: "#D50000",
  },
};

module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        primary: materialPalettes.blue,
        accent: materialPalettes.green,
        warn: materialPalettes.red
      },
      fontFamily: {
        poppins: ['Poppins', 'sans-serif'],
      },
    },
  },
  plugins: [
    require('./tailwind-plugins/better-base-styles'),
  ],
}
