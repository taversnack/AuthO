import { IEnvironment } from "./environment-interface";
import { appVersion } from "./version";

export const environment: IEnvironment = {
  appVersion,
  production: false,
  apiUrl: 'https://api.ci.locker.stsl.cloud/api/v1',
  auth: {
    scopes: 'openid profile offline_access auth0-user-api-spa create:tenants read:tenants update:tenants delete:tenants read:locations maintain:locations read:locker-banks maintain:locker-banks read:lockers maintain:lockers read:locks maintain:locks read:card-holders maintain:card-holders read:card-credentials maintain:card-credentials read:admins maintain:admins read:lease-users maintain:lease-users',
    domain: 'https://auth-dev.stsl.cloud',
    clientId: 'bMvgoAAssqCcREZrjhzWdY5B5bT3GM3M',
    audience: 'https://smart-locker.dev.stsl.co.uk/api/v1',
    claimsNamespace: 'https://goto-secure.stsl.co.uk',
  }
};
