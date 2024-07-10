export interface IEnvironment {
  appVersion: string;
  production: boolean;
  apiUrl: string;
  auth: IAuthConfig;
}

export interface IAuthConfig {
  scopes: string;
  domain: string;
  clientId: string;
  audience: string;
  claimsNamespace: string;
}
