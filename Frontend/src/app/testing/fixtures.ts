import { HttpBackend, HttpErrorResponse, HttpEvent, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import type { Account, Paged, SurveyDetails, SurveySummary } from '../core/api/types';

export function survey(overrides: Partial<SurveyDetails> = {}): SurveyDetails {
  return {
    id: 's1',
    title: 'Where do we go?',
    description: null,
    author: { id: 1, name: 'Alice', avatarUrl: null },
    createdAt: '2026-10-01T10:00:00Z',
    publishedAt: '2026-10-01T10:00:00Z',
    deadline: null,
    status: 'active',
    maxVotesPerUser: 1,
    allowParticipantOptions: false,
    maxOptionsPerParticipant: 1,
    isAuthor: false,
    voterCount: 0,
    totalVotes: 0,
    myVotes: [],
    myAddedOptions: 0,
    canVote: true,
    canAddOption: false,
    options: [
      { id: 1, text: 'Cinema', votes: 0, addedBy: null, voters: null },
      { id: 2, text: 'Park', votes: 0, addedBy: null, voters: null },
      { id: 3, text: 'Museum', votes: 0, addedBy: null, voters: null },
    ],
    ...overrides,
  };
}

export function summary(overrides: Partial<SurveySummary> = {}): SurveySummary {
  return {
    id: 's1',
    title: 'Where do we go?',
    author: { id: 1, name: 'Alice', avatarUrl: null },
    createdAt: '2026-10-01T10:00:00Z',
    publishedAt: '2026-10-01T10:00:00Z',
    deadline: null,
    status: 'active',
    optionCount: 3,
    voterCount: 0,
    hasVoted: false,
    ...overrides,
  };
}

export function paged<T>(items: T[]): Paged<T> {
  return { items, total: items.length, page: 1, pageSize: 20 };
}

export function account(overrides: Partial<Account> = {}): Account {
  return { id: 1, email: 'alice@example.com', displayName: 'Alice', emailConfirmed: true, locale: 'en', avatarUrl: null, ...overrides };
}

export interface Reply {
  status: number;
  body?: unknown;
}

export const reply = (status: number, body?: unknown): Reply => ({ status, body });

/** Turns a reply into what HttpClient's backend emits: a response, or an error for non-2xx. */
export function respond(request: HttpRequest<unknown>, { status, body }: Reply): Observable<HttpEvent<unknown>> {
  if (status >= 200 && status < 300) {
    return of(new HttpResponse({ status, body: status === 204 ? null : (body ?? null), url: request.urlWithParams }));
  }
  return throwError(() => new HttpErrorResponse({ status, error: body ?? null, url: request.urlWithParams }));
}

type Route = Reply | Reply[] | ((request: HttpRequest<unknown>) => Reply);

/**
 * An HttpBackend answering from a table keyed by "METHOD /path" (query string ignored). A list of
 * replies is used in order and its last item repeats. Unknown routes fail the test; the auth config
 * defaults to "no email". `calls` records every request as "METHOD /path?query".
 */
export class MockBackend implements HttpBackend {
  readonly calls: string[] = [];
  private readonly table: Record<string, Route>;

  constructor(routes: Record<string, Route>) {
    this.table = { 'GET /api/auth/config': reply(200, { emailEnabled: false, confirmationRequired: false }), ...routes };
  }

  callsTo(prefix: string): string[] {
    return this.calls.filter((c) => c.startsWith(prefix));
  }

  handle(request: HttpRequest<unknown>): Observable<HttpEvent<unknown>> {
    this.calls.push(`${request.method} ${request.urlWithParams}`);
    const key = `${request.method} ${request.url}`;
    const route = this.table[key];
    if (!route) return throwError(() => new Error(`Unexpected request ${key}`));
    const next = Array.isArray(route) ? (route.length > 1 ? route.shift()! : route[0]!) : typeof route === 'function' ? route(request) : route;
    return respond(request, next);
  }
}
