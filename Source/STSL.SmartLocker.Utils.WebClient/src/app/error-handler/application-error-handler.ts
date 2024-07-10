import { ErrorHandler, inject } from "@angular/core";
import { DialogService } from "../dialog/services/dialog.service";
import { CustomError, ErrorType, IBadRequestError, isBadRequestProblemDetails, isProblemDetails } from "../shared/models/error";

export class ApplicationErrorHandler implements ErrorHandler {
  private readonly dialogService = inject(DialogService);

  handleError(error: ErrorType) {
    if(isBadRequestProblemDetails(error)) {
      this.dialogService.error(this.formatBadRequestError(error), error.title);
    }
    else if(isProblemDetails(error)) {
      this.dialogService.error(error.detail, error.title);
    }
    else if(error instanceof CustomError) {
      this.dialogService.error(error.message, error.name);
    }
    console.error(error);
  }

  private formatBadRequestError(error: IBadRequestError): string[] {
    return Object.entries(error.errors).flatMap(([key, value]) => {
      const numericKey = parseInt(key);
      if(!isNaN(numericKey)) {
        // validation of multiple objects
        const indexedErrors = Object.entries(value);
        return [numericKey + ':', ...indexedErrors.map(([property, propertyValue]) => this.formatValidationErrorRecord(property, propertyValue as string))];
      } else {
        return this.formatValidationErrorRecord(key, value);
      }
    });
  }

  private formatValidationErrorRecord(property: string, value: string) {
    return `${property}: ${value}`;
  }
}

