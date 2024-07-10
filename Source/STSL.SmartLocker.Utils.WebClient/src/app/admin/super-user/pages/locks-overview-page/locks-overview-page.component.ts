import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { entityFullPermissions } from 'src/app/admin/admin-common/models/entity-views';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { ILockDTO } from 'src/app/shared/models/lock';

@Component({
  selector: 'super-user-locks-overview-page',
  templateUrl: './locks-overview-page.component.html',
})
export class LocksOverviewPageComponent {

  private readonly adminApiService = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly lockPermissions = entityFullPermissions;

  forceLockUpdate(lock: ILockDTO) {
    this.adminApiService.Locks.ForceConfigUpdate(lock.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
  }
}
