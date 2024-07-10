import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { inject } from "@angular/core";
import { Observable, catchError } from "rxjs";
import { environment } from "src/environments/environment";
import { AuthService } from "../auth/services/auth.service";
import { DialogService } from "../dialog/services/dialog.service";

export class HttpErrorInterceptor implements HttpInterceptor {

  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(catchError(
        (error, caught) => {
          if(error instanceof HttpErrorResponse) {
            if (error.error instanceof ErrorEvent) {
              this.handleErrorDetail(error.error);
            }

            switch(error.status) {
              case 0: {
                this.dialogService.error("There was a problem connecting to the server, please try again later", "Connection Failure");
                break;
              }
              case 401: this.authService.login(); break;
              case 403: {
                this.dialogService.error("You don't have permission to complete this action", "Permission Denied");
                break;
              }
              case 405: {
                this.dialogService.error("This API method is not allowed, please report this error to your account administrator", "Method Not Allowed");
                break;
              }
            }
          }
          throw error;
        }
      ));
  }

  private handleErrorDetail(error: ErrorEvent) {
    if(!environment.production) {
      console.error('application error occurred during network request', error);
    }
  }
}
