import { Component, inject } from '@angular/core';
import { AuthService } from 'src/app/auth/services/auth.service';

@Component({
  selector: 'tenant-admin-tenant-admin-dashboard-page',
  templateUrl: './tenant-admin-dashboard-page.component.html',
})
export class TenantAdminDashboardPageComponent {

  private readonly authService = inject(AuthService);

  cardHolderAliasPlural$ = this.authService.cardHolderAliasPlural$;

  tenantLogoSrc$ = this.authService.currentTenantLogoSrc$;
}
