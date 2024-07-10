const plugin = require('tailwindcss/plugin');

module.exports = plugin(function({ addBase, theme }) {
  addBase({
    // Styled Headings (h1 - h5)
    'h1': { fontSize: theme('fontSize.lg'), fontWeight: theme('fontWeight.semibold') },
    'h2': { fontSize: theme('fontSize.lg') },
    'h3': { fontSize: theme('fontWeight.semibold') },
    'h4': { fontWeight: theme('fontWeight.semibold') },
    'h5': { fontWeight: theme('fontWeight.semibold') },

    '@media all and (min-width: 1024px)': {
      'h1': { fontSize: theme('fontSize.xl'), fontWeight: theme('fontWeight.semibold') },
      'h2': { fontSize: theme('fontSize.xl') },
      'h3': { fontSize: theme('fontSize.lg') },
      'h4': { fontWeight: theme('fontWeight.semibold') },
      'h5': { fontWeight: theme('fontWeight.semibold') },
    }
  })
})
