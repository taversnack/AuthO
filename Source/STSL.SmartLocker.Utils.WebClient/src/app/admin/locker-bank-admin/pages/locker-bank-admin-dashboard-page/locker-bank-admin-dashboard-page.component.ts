import { Component, inject } from '@angular/core';
import { AuthService } from 'src/app/auth/services/auth.service';

@Component({
  selector: 'admin-locker-bank-admin-dashboard-page',
  templateUrl: './locker-bank-admin-dashboard-page.component.html',
})
export class LockerBankAdminDashboardPageComponent {

  private readonly authService = inject(AuthService);

  readonly cardHolderAliasPlural$ = this.authService.cardHolderAliasPlural$;

  readonly tenantLogoSrc$ = this.authService.currentTenantLogoSrc$;
}
