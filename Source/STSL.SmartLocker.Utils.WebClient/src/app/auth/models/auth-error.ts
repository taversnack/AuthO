import { CustomError } from 'src/app/shared/models/error';

export class AuthError extends CustomError {

}

export class UnassignedTenantsAuthError extends AuthError {
  override name: string = 'No Registered Tenants';
  override message: string = 'You are not registered to any organizations, please contact your account administrator';
}

export class UnassignedRolesAuthError extends AuthError {
  override name: string = 'No Assigned Role';
  override message: string = 'You have not been assigned a role, please contact your account administrator';
}
