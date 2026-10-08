import { HttpErrorResponse } from '@angular/common/http';

/** Error returned by the API. `code` is the stable key from the backend's problem details. */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: string,
    message: string,
    /** Field name (camelCase) -> error codes. */
    readonly fields: Record<string, string[]> = {},
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

const STATUS_CODES: Record<number, string> = {
  401: 'unauthorized',
  403: 'forbidden',
  404: 'not_found',
  429: 'too_many_requests',
};

interface ProblemDetails {
  code?: string;
  title?: string;
  errors?: Record<string, string[]>;
}

export function toApiError(response: HttpErrorResponse): ApiError {
  // Status 0: the request never got an answer (offline, DNS, CORS).
  if (response.status === 0) return new ApiError(0, 'network', 'Network error');

  // A non-JSON body (e.g. a proxy error page) arrives as a string.
  const body: ProblemDetails = response.error && typeof response.error === 'object' ? response.error : {};
  const fields = Object.fromEntries(
    Object.entries(body.errors ?? {}).map(([key, value]) => [key.charAt(0).toLowerCase() + key.slice(1), value]),
  );
  const code = body.code ?? STATUS_CODES[response.status] ?? (response.status >= 500 ? 'server_error' : 'unknown');
  return new ApiError(response.status, code, body.title ?? response.statusText, fields);
}
