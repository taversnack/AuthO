import { Component, inject } from '@angular/core';
import { AuthService } from 'src/app/auth/services/auth.service';

@Component({
  selector: 'super-user-dashboard-page',
  templateUrl: './super-user-dashboard-page.component.html',
})
export class SuperUserDashboardPageComponent {

  private readonly authService = inject(AuthService);

  readonly cardHolderAliasPlural$ = this.authService.cardHolderAliasPlural$;

  readonly tenantLogoSrc$ = this.authService.currentTenantLogoSrc$;
}
