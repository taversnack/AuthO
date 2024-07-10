import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { DATE_PIPE_DEFAULT_OPTIONS, DatePipeConfig } from '@angular/common';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { environment } from 'src/environments/environment';
import { AdminModule } from './admin/admin.module';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthConfigModule } from './auth/auth-config.module';
import { DialogModule } from './dialog/dialog.module';
import { ErrorHandlerModule } from './error-handler/error-handler.module';
import { MaterialModule } from './material/material.module';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { NotFoundPageComponent } from './pages/not-found-page/not-found-page.component';
import { FooterComponent } from './pages/partials/footer/footer.component';
import { NavigationComponent } from './pages/partials/navigation/navigation.component';
import { LayoutComponent } from './shared/components/layout/layout.component';
import { API_CONFIG, IApiConfig } from './shared/models/api';
import { AnnouncekitModule } from 'announcekit-angular';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@NgModule({
  declarations: [
    AppComponent,
    HomePageComponent,
    NotFoundPageComponent,
    NavigationComponent,
    FooterComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    MaterialModule,
    DialogModule,
    AdminModule,
    AuthConfigModule,
    ErrorHandlerModule,
    LayoutComponent,
    AnnouncekitModule,
    MatTooltipModule,
    FontAwesomeModule,
  ],
  providers: [
    {
      provide: API_CONFIG,
      useValue: {
        baseUrl: environment.apiUrl,
        useGlobalErrorHandler: true,
        timeoutMilliseconds: 45 * 1000,
        // retryCount: 1,
      } as IApiConfig
    },
    {
      provide: DATE_PIPE_DEFAULT_OPTIONS,
      useValue: {
        dateFormat: 'HH:mm dd/MM/yy'
      } as DatePipeConfig
    }
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
