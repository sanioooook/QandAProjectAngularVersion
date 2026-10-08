import { HttpBackend, HttpEvent, HttpRequest } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import type { Account, SurveyDetails, SurveyInput, SurveySummary } from '../core/api/types';
import { Reply, reply, respond } from './fixtures';

/**
 * In-memory stand-in for the API, for app-level integration tests. It keeps users, the session and
 * surveys, and answers with the same DTO shapes as Backend/src/QandA.Api. It replaces HttpClient's
 * backend, so every request still goes through the app's interceptor. Only the behaviour the
 * screens rely on is modelled; the real rules are covered by the backend tests.
 */
export class FakeApi implements HttpBackend {
  users: (Account & { password: string })[] = [];
  surveys: SurveyDetails[] = [];
  /** Votes per survey: userId -> option ids. */
  votes = new Map<string, Map<number, number[]>>();
  sessionUserId: number | null = null;
  /** Every request as "METHOD /path?query", in order. */
  calls: string[] = [];
  /** Requests answered with 401 as if the session expired on the server. */
  expireSessionOn: string | null = null;

  private nextId = 1;

  addUser(email: string, displayName: string, password = 'Secret123'): Account {
    const user = { id: this.nextId++, email, displayName, emailConfirmed: true, locale: 'en', avatarUrl: null, password };
    this.users.push(user);
    return this.account(user.id);
  }

  addSurvey(author: Account, title: string, options: string[], extra: Partial<SurveyDetails> = {}): SurveyDetails {
    const survey: SurveyDetails = {
      id: `s${this.nextId++}`,
      title,
      description: null,
      author: { id: author.id, name: author.displayName, avatarUrl: null },
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
      canVote: false,
      canAddOption: false,
      options: options.map((text) => ({ id: this.nextId++, text, votes: 0, addedBy: null, voters: null })),
      ...extra,
    };
    this.surveys.push(survey);
    return survey;
  }

  callsTo(prefix: string): string[] {
    return this.calls.filter((c) => c.startsWith(prefix));
  }

  handle(request: HttpRequest<unknown>): Observable<HttpEvent<unknown>> {
    const path = request.url;
    this.calls.push(`${request.method} ${request.urlWithParams}`);
    if (this.expireSessionOn === `${request.method} ${path}`) {
      this.sessionUserId = null;
      return respond(request, reply(401, { code: 'unauthorized' }));
    }
    const answer = this.route(request);
    return answer ? respond(request, answer) : throwError(() => new Error(`FakeApi: no route for ${request.method} ${path}`));
  }

  private route(request: HttpRequest<unknown>): Reply | null {
    const { method, url: path } = request;
    const body = (request.body ?? {}) as Record<string, unknown>;
    const me = this.users.find((u) => u.id === this.sessionUserId);

    if (method === 'GET' && path === '/api/auth/config') return reply(200, { emailEnabled: false, confirmationRequired: false });
    if (method === 'GET' && path === '/api/auth/me') return me ? reply(200, this.account(me.id)) : reply(401, { code: 'unauthorized' });
    if (method === 'POST' && path === '/api/auth/logout') {
      this.sessionUserId = null;
      return reply(204);
    }
    if (method === 'POST' && path === '/api/auth/register') {
      const email = String(body['email'] ?? '');
      if (this.users.some((u) => u.email.toLowerCase() === email.toLowerCase())) {
        return reply(409, { code: 'email_taken', errors: { email: ['email_taken'] } });
      }
      const account = this.addUser(email, String(body['displayName'] ?? ''), String(body['password'] ?? ''));
      this.sessionUserId = account.id;
      return reply(201, account);
    }
    if (method === 'POST' && path === '/api/auth/login') {
      const user = this.users.find(
        (u) => u.email.toLowerCase() === String(body['email']).toLowerCase() && u.password === body['password'],
      );
      if (!user) return reply(401, { code: 'invalid_credentials' });
      this.sessionUserId = user.id;
      return reply(200, this.account(user.id));
    }
    if (method === 'PUT' && path === '/api/account/profile' && me) {
      if (typeof body['locale'] === 'string') me.locale = body['locale'];
      if (typeof body['displayName'] === 'string') me.displayName = body['displayName'];
      return reply(200, this.account(me.id));
    }

    if (method === 'GET' && path === '/api/surveys') {
      const scope = request.params.get('scope') ?? 'active';
      if (scope !== 'active' && !me) return reply(401, { code: 'unauthorized' });
      const items = this.surveys
        .filter((s) =>
          scope === 'mine' ? s.author.id === me?.id : scope === 'voted' ? this.myVotes(s, me?.id).length > 0 : s.status === 'active',
        )
        .map((s) => this.summary(s, me?.id));
      return reply(200, { items, total: items.length, page: 1, pageSize: 12 });
    }
    if (method === 'POST' && path === '/api/surveys') {
      if (!me) return reply(401, { code: 'unauthorized' });
      const input = body as unknown as SurveyInput;
      const survey = this.addSurvey(this.account(me.id), input.title, input.options, {
        description: input.description,
        deadline: input.deadline,
        maxVotesPerUser: input.maxVotesPerUser,
        status: input.publish ? 'active' : 'draft',
        publishedAt: input.publish ? '2026-10-08T10:00:00Z' : null,
      });
      return reply(201, this.details(survey, me.id));
    }

    const match = path.match(/^\/api\/surveys\/([^/]+)(\/votes)?$/);
    if (!match) return null;
    const survey = this.surveys.find((s) => s.id === match[1]);
    if (!survey) return reply(404, { code: 'not_found' });
    if (method === 'GET' && !match[2]) return reply(200, this.details(survey, me?.id));
    if (method === 'PUT' && match[2]) {
      if (!me) return reply(401, { code: 'unauthorized' });
      const optionIds = (body['optionIds'] as number[]) ?? [];
      if (optionIds.length > survey.maxVotesPerUser) return reply(400, { code: 'too_many_votes' });
      this.votesOf(survey).set(me.id, optionIds);
      return reply(200, this.details(survey, me.id));
    }
    return null;
  }

  private account(id: number): Account {
    const { password: _, ...account } = this.users.find((u) => u.id === id)!;
    return { ...account };
  }

  private votesOf(survey: SurveyDetails) {
    if (!this.votes.has(survey.id)) this.votes.set(survey.id, new Map());
    return this.votes.get(survey.id)!;
  }

  private myVotes(survey: SurveyDetails, userId: number | undefined) {
    return userId === undefined ? [] : (this.votesOf(survey).get(userId) ?? []);
  }

  private details(survey: SurveyDetails, userId: number | undefined): SurveyDetails {
    const all = [...this.votesOf(survey).values()];
    const active = survey.status === 'active';
    return {
      ...survey,
      isAuthor: survey.author.id === userId,
      myVotes: this.myVotes(survey, userId),
      voterCount: all.filter((v) => v.length > 0).length,
      totalVotes: all.flat().length,
      canVote: active && userId !== undefined,
      options: survey.options.map((o) => ({ ...o, votes: all.flat().filter((id) => id === o.id).length })),
    };
  }

  private summary(survey: SurveyDetails, userId: number | undefined): SurveySummary {
    const d = this.details(survey, userId);
    return {
      id: d.id,
      title: d.title,
      author: d.author,
      createdAt: d.createdAt,
      publishedAt: d.publishedAt,
      deadline: d.deadline,
      status: d.status,
      optionCount: d.options.length,
      voterCount: d.voterCount,
      hasVoted: d.myVotes.length > 0,
    };
  }
}
