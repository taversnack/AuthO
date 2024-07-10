import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { AuthInterceptor, AuthModule } from 'angular-auth-oidc-client';
import { environment } from 'src/environments/environment';

@NgModule({
  imports: [
    AuthModule.forRoot({
      config: {
        authority: environment.auth.domain,
        redirectUrl: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
        clientId: environment.auth.clientId,
        scope: environment.auth.scopes,
        responseType: 'code',
        silentRenew: true,
        useRefreshToken: true,
        renewTimeBeforeTokenExpiresInSeconds: 60,
        useCustomAuth0Domain: true,
        secureRoutes: [environment.apiUrl],
        customParamsAuthRequest: {
          audience: environment.auth.audience,
        },
        // customParamsRefreshTokenRequest: {
        //     scope: 'openid profile offline_access auth0-user-api-spa',
        // },
      },
    }),
    HttpClientModule,
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
  ],
  exports: [AuthModule],
})
export class AuthConfigModule {}
