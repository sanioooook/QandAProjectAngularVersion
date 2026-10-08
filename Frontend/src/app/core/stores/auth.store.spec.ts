import { HttpBackend, provideHttpClient, withInterceptors } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { MockBackend, account, reply, survey } from '../../testing/fixtures';
import { apiInterceptor } from '../api/api.interceptor';
import { AuthStore } from './auth.store';
import { SurveysStore } from './surveys.store';

function setup(routes: ConstructorParameters<typeof MockBackend>[0]) {
  const backend = new MockBackend(routes);
  TestBed.configureTestingModule({
    providers: [provideHttpClient(withInterceptors([apiInterceptor])), { provide: HttpBackend, useValue: backend }],
  });
  return { auth: TestBed.inject(AuthStore), backend };
}

describe('AuthStore', () => {
  it('asks the server for the session and config only once', async () => {
    const { auth, backend } = setup({ 'GET /api/auth/me': reply(200, account()) });

    await Promise.all([auth.init(), auth.init()]);
    await auth.init();

    expect(backend.calls).toEqual(['GET /api/auth/me', 'GET /api/auth/config']);
    expect(auth.user()?.displayName).toBe('Alice');
  });

  it('treats 401 from /me as a guest', async () => {
    const { auth } = setup({ 'GET /api/auth/me': reply(401, { code: 'unauthorized' }) });

    await auth.init();

    expect(auth.isLoggedIn()).toBe(false);
    expect(auth.initialized()).toBe(true);
  });

  it('a broken config endpoint does not block the app: email features just stay off', async () => {
    const { auth } = setup({ 'GET /api/auth/me': reply(200, account()), 'GET /api/auth/config': reply(500) });

    await auth.init();

    expect(auth.isLoggedIn()).toBe(true);
    expect(auth.config().emailEnabled).toBe(false);
  });

  it('needs confirmation only when the server requires it and the email is unconfirmed', async () => {
    const { auth } = setup({
      'GET /api/auth/me': reply(200, account({ emailConfirmed: false })),
      'GET /api/auth/config': reply(200, { emailEnabled: true, confirmationRequired: true }),
    });
    await auth.init();

    expect(auth.needsConfirmation()).toBe(true);
    auth.setUser(account({ emailConfirmed: true }));
    expect(auth.needsConfirmation()).toBe(false);
  });

  it('wrong password keeps the user signed out and surfaces the error code', async () => {
    const { auth } = setup({ 'POST /api/auth/login': reply(401, { code: 'invalid_credentials' }) });

    await expect(auth.login({ email: 'alice@example.com', password: 'nope' })).rejects.toMatchObject({ code: 'invalid_credentials' });

    expect(auth.isLoggedIn()).toBe(false);
  });

  it('switching users drops the cached surveys of the previous one', async () => {
    const { auth } = setup({
      'POST /api/auth/login': [reply(200, account()), reply(200, account({ id: 2, displayName: 'Bob' }))],
      'GET /api/surveys/s1': reply(200, survey()),
    });
    const surveys = TestBed.inject(SurveysStore);
    await auth.login({ email: 'alice@example.com', password: 'Secret123' });
    await surveys.fetchSurvey('s1');

    await auth.login({ email: 'bob@example.com', password: 'Secret123' });

    expect(surveys.survey('s1')).toBeUndefined();
  });

  it('a new display name drops cached surveys (they show the old name), a new language does not', async () => {
    const { auth } = setup({
      'POST /api/auth/login': reply(200, account()),
      'GET /api/surveys/s1': reply(200, survey()),
      'PUT /api/account/profile': [reply(200, account({ locale: 'uk' })), reply(200, account({ displayName: 'Alicia' }))],
    });
    const surveys = TestBed.inject(SurveysStore);
    await auth.login({ email: 'alice@example.com', password: 'Secret123' });
    await surveys.fetchSurvey('s1');

    await auth.updateProfile({ locale: 'uk' });
    expect(surveys.survey('s1')).toBeDefined();

    await auth.updateProfile({ displayName: 'Alicia' });
    expect(surveys.survey('s1')).toBeUndefined();
    expect(auth.user()?.displayName).toBe('Alicia');
  });

  it('confirming the email in a signed-in browser unlocks the account at once', async () => {
    const { auth } = setup({
      'POST /api/auth/login': reply(200, account({ emailConfirmed: false })),
      'POST /api/auth/confirm-email': reply(204),
      'GET /api/auth/me': reply(200, account({ emailConfirmed: true })),
    });
    await auth.login({ email: 'alice@example.com', password: 'Secret123' });

    await auth.confirmEmail('token');

    expect(auth.user()?.emailConfirmed).toBe(true);
  });

  it('logout clears the session even when the request fails', async () => {
    const { auth } = setup({ 'POST /api/auth/login': reply(200, account()), 'POST /api/auth/logout': reply(500) });
    await auth.login({ email: 'alice@example.com', password: 'Secret123' });

    await expect(auth.logout()).rejects.toBeTruthy();

    expect(auth.isLoggedIn()).toBe(false);
  });
});
