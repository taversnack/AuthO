const path = require('path');
const fs = require('fs');

const APP_VERSION_DEFAULT_VALUE = '1.0.0';
const APP_VERSION_VARIABLE_NAME = 'appVersion';
const APP_VERSION_FILE_PATH = '/src/environments/version.ts';

const colours = {
  green: "\x1b[32m",
  cyan: "\x1b[36m",
};

const colour = (string, colour) => {
  return `${colour}${string}\x1b[0m`
}

const args = process.argv.slice(2);

// Get version from args or npm_package_version envvar or use default as fallback
const appVersion = args?.[0] ?? process.env.npm_package_version ?? APP_VERSION_DEFAULT_VALUE;

console.log(colour('\nSynchronising app version\n', colours.cyan));

const versionFilePath = path.join(__dirname + APP_VERSION_FILE_PATH);

const versionFileContent = `export const ${APP_VERSION_VARIABLE_NAME} = '${appVersion}';`;

fs.writeFile(versionFilePath, versionFileContent, err =>
  err ? console.error(err) : console.log('App version set to: ' + colour(appVersion, colours.green))
);
