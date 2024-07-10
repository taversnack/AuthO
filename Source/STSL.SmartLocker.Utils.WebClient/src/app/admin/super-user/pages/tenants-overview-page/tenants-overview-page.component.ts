import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AuthService } from 'src/app/auth/services/auth.service';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { IPageFilterSort, IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort } from 'src/app/shared/models/api';
import { StickyPosition } from 'src/app/shared/models/data-table';
import { ICreateTenantDTO, ITenantDTO, IUpdateTenantDTO } from 'src/app/shared/models/tenant';

@Component({
  selector: 'super-user-tenants-overview-page',
  templateUrl: './tenants-overview-page.component.html',
  styleUrls: ['./tenants-overview-page.component.css']
})
export class TenantsOverviewPageComponent implements OnInit {

  private readonly adminApiService = inject(AdminApiService);
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly tenantsSubject = new BehaviorSubject<IPagingResponse<ITenantDTO> | undefined>(undefined);
  readonly tenants$ = this.tenantsSubject.asObservable();

  private readonly isLoadingTenantsSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingTenants$ = this.isLoadingTenantsSubject.asObservable();

  private readonly tenantToEditSubject = new BehaviorSubject<ITenantDTO | undefined>(undefined);
  readonly tenantToEdit$ = this.tenantToEditSubject.asObservable();

  readonly StickyPosition = StickyPosition;

  private tenantPageFilterSort: IPageFilterSort = new PageFilterSort();

  ngOnInit(): void {
    this.getTenants();
  }

  createOrEditTenant(tenant: ICreateTenantDTO | IUpdateTenantDTO) {
    const currentTenant = this.tenantToEditSubject.getValue();
    if(currentTenant) {
      this.adminApiService.Tenants.updateSingle(currentTenant.id, tenant).subscribe(updated => {
        if(updated) {
          this.authService.refreshTenantDetails().subscribe();
        }
      });
    } else {
      this.adminApiService.Tenants.createSingle(tenant).subscribe(created => {
        if(created) {
          this.authService.refreshTenantDetails().subscribe();
        }
      });
    }
  }

  editTenant(tenant: ITenantDTO) {
    this.tenantToEditSubject.next(tenant);
  }

  cancelEdits() {
    this.tenantToEditSubject.next(undefined);
  }

  deleteTenant(tenant: ITenantDTO) {
    this.dialogService.confirm(`Are you sure you want to delete ${tenant.name}?`, `Delete ${tenant.name}?`).subscribe(confirmed => {
      if(confirmed) {
        this.adminApiService.Tenants.deleteSingle(tenant.id).subscribe(deleted => {
          if(deleted) {
            this.authService.refreshTenantDetails().subscribe();
          }
        });
      }
    })
  }

  onPagingEvent(page: IPagingRequest) {
    const pageFilterSort = { ...this.tenantPageFilterSort, page };
    this.tenantPageFilterSort = pageFilterSort;
    this.getTenants();
  }

  onSortingEvent(sort?: ISortedRequest) {
    const pageFilterSort = { ...this.tenantPageFilterSort, sort };
    this.tenantPageFilterSort = pageFilterSort;
    this.getTenants();
  }

  private getTenants() {
    const onResponse = {
      next: (x: IPagingResponse<ITenantDTO>) => {
        this.tenantsSubject.next(x);
        this.isLoadingTenantsSubject.next(false);
      },
      error: () => this.isLoadingTenantsSubject.next(false),
    };

    this.isLoadingTenantsSubject.next(true);

    this.adminApiService.Tenants
      .getMany(this.tenantPageFilterSort)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(onResponse);
  }
}
