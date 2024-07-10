const plugin = require('tailwindcss/plugin');

const gapVariableName = '--flex-cols-gap';

module.exports = plugin(function ({ matchUtilities, theme }) {
  matchUtilities({
    gap: (value) => ({
      gap: value,
      [gapVariableName]: value
    })
  },
    {
      values: theme('gap')
    }
  );
  matchUtilities(
    {
      'flex-cols': (value) => ({
        flexDirection: 'row',
        '& > *': {
          flexBasis: `calc((100% / ${value}) - ((var(${gapVariableName}, 0rem) * ${value - 1})) / ${value})`,
        }
      }),
    },
    {
      values: theme('flexCols'),
      type: 'number',
      supportsNegativeValues: false,
    }
  );
}, {
  theme: {
    flexCols: {
      1: 1,
      2: 2,
      3: 3,
      4: 4,
      5: 5,
      6: 6,
      7: 7,
      8: 8,
      9: 9,
      10: 10,
      11: 11,
      12: 12,
    }
  },
  corePlugins: {
    gap: false
  }
});
