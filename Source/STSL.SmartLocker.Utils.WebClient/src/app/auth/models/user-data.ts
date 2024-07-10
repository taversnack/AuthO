import { ITenantDTO } from 'src/app/shared/models/tenant';
import { UserRole } from "./user-roles";

export interface IUserData {
  sub: string;
  name: string;
  email: string;
  nickname?: string;
  picture?: string;
  roles: UserRole[];
  tenants: ITenantDTO[];
}
