import { HttpErrorResponse } from '@angular/common/http';

const DEFAULT_MESSAGE = 'Could not reach the server. Please check your connection and try again.';

export function apiErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse && typeof error.error?.detail === 'string') {
    return error.error.detail;
  }

  return DEFAULT_MESSAGE;
}
