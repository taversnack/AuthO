import { IEnvironment } from "./environment-interface";

export const environment: IEnvironment = {
  appVersion: '',
  production: false,
  apiUrl: '',
  auth: {
    scopes: '',
    domain: '',
    clientId: '',
    audience: '',
    claimsNamespace: '',
  },
};
