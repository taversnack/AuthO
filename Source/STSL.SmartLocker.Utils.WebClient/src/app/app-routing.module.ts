import { Injectable, NgModule } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRouteSnapshot, DetachedRouteHandle, ExtraOptions, RouteReuseStrategy, RouterModule, RouterStateSnapshot, Routes, TitleStrategy } from '@angular/router';
import { AutoLoginPartialRoutesGuard } from 'angular-auth-oidc-client';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { NotFoundPageComponent } from './pages/not-found-page/not-found-page.component';

@Injectable({ providedIn: 'root' })
export class SitePageTitleStrategy extends TitleStrategy {
  constructor(private readonly title: Title) {
    super();
  }

  override updateTitle(routerState: RouterStateSnapshot) {
    const title = this.buildTitle(routerState);
    if (title !== undefined) {
      this.title.setTitle(`STSL Goto Secure™ | ${title}`);
    }
  }
}

@Injectable({ providedIn: 'root' })
export class PageRefreshRouteReuseStrategy extends RouteReuseStrategy {

  // TODO: Create a Tenant caching service that can be called by the auth service.
  // This strategy can check if the AuthService has set 'tenantHasChanged'.
  // If set; this strategy resets it and reloads the route if the route is the same (page refresh).
  // Store and retrieve could perhaps store the last tenantId and if not matching retrieve will return null.

  override shouldDetach(route: ActivatedRouteSnapshot): boolean {
    return false;
  }
  override store(route: ActivatedRouteSnapshot, handle: DetachedRouteHandle | null): void {

  }
  override shouldAttach(route: ActivatedRouteSnapshot): boolean {
    return false;
  }
  override retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle | null {
    return null;
  }
  override shouldReuseRoute(future: ActivatedRouteSnapshot, curr: ActivatedRouteSnapshot): boolean {
    // future.url === curr.url;
    return false;
  }

}

const routes: Routes = [
  {
    path: '',
    title: 'Home',
    component: HomePageComponent
  },
  {
    path: 'admin',
    canLoad: [AutoLoginPartialRoutesGuard],
    loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule)
  },
  {
    path: '**',
    title: 'Page Not Found',
    component: NotFoundPageComponent
  }
];

const routerOptions: ExtraOptions = {
  // scrollPositionRestoration: 'enabled',
  // anchorScrolling: 'enabled',
  onSameUrlNavigation: 'reload',
};

// TODO: [7]: Change to using provideRouter with app component as standalone using
// new bootstrapApplication. Will reduce bundle size in production by tree shaking
// unused routing features.
@NgModule({
  imports: [RouterModule.forRoot(routes, routerOptions)],
  exports: [RouterModule],
  providers: [
    { provide: TitleStrategy, useClass: SitePageTitleStrategy },
    { provide: RouteReuseStrategy, useClass: PageRefreshRouteReuseStrategy },
  ]
})
export class AppRoutingModule { }
