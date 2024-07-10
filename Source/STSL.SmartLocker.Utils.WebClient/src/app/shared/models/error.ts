import { HttpStatusCode } from "@angular/common/http";

export interface IProblemDetails {
  type?: string;
  title: string;
  status?: HttpStatusCode;
  detail: string
  instance?: string;
}

export class CustomError extends Error {
  static readonly isCustomError: true = true;
}

export type ErrorType = Error | IProblemDetails;

export interface IBadRequestError extends IProblemDetails {
  errors: Record<string, string> | Record<number, Record<string, string>>;
}

export const isProblemDetails = (error: ErrorType): error is IProblemDetails =>
  ['title', 'detail'].every(x => x in error);

export const isBadRequestProblemDetails = (error: ErrorType): error is IBadRequestError =>
  ['title', 'errors'].every(x => x in error) && 'status' in error && error.status === 400;
