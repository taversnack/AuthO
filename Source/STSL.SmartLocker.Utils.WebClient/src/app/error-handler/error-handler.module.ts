import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ErrorHandler, NgModule } from '@angular/core';
import { Router } from '@angular/router';
import { DialogService } from '../dialog/services/dialog.service';
import { ApplicationErrorHandler } from './application-error-handler';
import { HttpErrorInterceptor } from './http-error-interceptor';

@NgModule({
  providers: [
    { provide: ErrorHandler, useClass: ApplicationErrorHandler, deps: [DialogService] },
    { provide: HTTP_INTERCEPTORS, useClass: HttpErrorInterceptor, deps: [DialogService, Router], multi: true },
  ]
})
export class ErrorHandlerModule { }
