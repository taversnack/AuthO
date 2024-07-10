import { IEnvironment } from "./environment-interface";
import { appVersion } from "./version";

export const environment: IEnvironment = {
  appVersion,
  production: true,
  apiUrl: 'https://api.staging.locker.stsl.cloud/api/v1',
  auth: {
    scopes: 'openid profile offline_access auth0-user-api-spa create:tenants read:tenants update:tenants delete:tenants read:locations maintain:locations read:locker-banks maintain:locker-banks read:lockers maintain:lockers read:locks maintain:locks read:card-holders maintain:card-holders read:card-credentials maintain:card-credentials read:admins maintain:admins read:lease-users maintain:lease-users',
    domain: 'https://auth.stsl.cloud',
    clientId: '8hAhAuneAuF1R12T4CMkmGaBURdhrg9b',
    audience: 'https://goto-secure.stsl.co.uk/api/v1',
    claimsNamespace: 'https://goto-secure.stsl.co.uk',
  }
};
