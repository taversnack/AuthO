import { environment } from "src/environments/environment";

export const UserClaims = {
  Roles: `${environment.auth.claimsNamespace}/roles`,
  Tenants: `${environment.auth.claimsNamespace}/tenants`,
  Email: `${environment.auth.claimsNamespace}/email`,
};
