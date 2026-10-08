import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Subject, catchError, throwError } from 'rxjs';
import { toApiError } from './api-error';

/** Signals that the server rejected the session cookie (expired, password changed elsewhere, user gone). */
@Injectable({ providedIn: 'root' })
export class SessionEvents {
  readonly expired = new Subject<void>();
}

/**
 * Every API call goes through here: failures become an `ApiError` with the backend's error code,
 * and a 401 outside the auth endpoints themselves announces an expired session.
 */
export const apiInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(SessionEvents);
  return next(request.clone({ setHeaders: { Accept: 'application/json' } })).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) return throwError(() => error);
      if (error.status === 401 && !new URL(request.url, 'http://app').pathname.startsWith('/api/auth/')) {
        session.expired.next();
      }
      return throwError(() => toApiError(error));
    }),
  );
};
