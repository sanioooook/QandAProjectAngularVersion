import { HttpBackend, provideHttpClient, withInterceptors } from '@angular/common/http';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, Routes, provideRouter } from '@angular/router';
import { NEVER } from 'rxjs';
import { MockBackend, account, reply } from '../testing/fixtures';
import { apiInterceptor } from './api/api.interceptor';
import { authGuard, safeRedirect } from './auth.guard';

@Component({ template: '' })
class Page {}

// The real route table's access rules, with empty pages.
const routes: Routes = [
  {
    path: '',
    canActivateChild: [authGuard],
    children: [
      { path: 'login', component: Page, data: { guestOnly: true } },
      { path: 'register', component: Page, data: { guestOnly: true } },
      { path: 'surveys', component: Page },
      { path: 'my', component: Page, data: { requiresAuth: true } },
      { path: 'voted', component: Page, data: { requiresAuth: true } },
      { path: 'surveys/new', component: Page, data: { requiresAuth: true } },
      { path: 'surveys/:id/edit', component: Page, data: { requiresAuth: true } },
      { path: 'surveys/:id', component: Page },
      { path: '**', component: Page },
    ],
  },
];

function setup(backend: HttpBackend) {
  TestBed.configureTestingModule({
    providers: [provideRouter(routes), provideHttpClient(withInterceptors([apiInterceptor])), { provide: HttpBackend, useValue: backend }],
  });
  return TestBed.inject(Router);
}

const guest = () => new MockBackend({ 'GET /api/auth/me': reply(401) });

describe('authGuard', () => {
  it('lets guests open a shared survey link and the active list', async () => {
    const router = setup(guest());

    expect(await router.navigateByUrl('/surveys/abc')).toBe(true);
    expect(router.url).toBe('/surveys/abc');
    expect(await router.navigateByUrl('/surveys')).toBe(true);
  });

  it('does not hold public pages until the session is known', async () => {
    const router = setup({ handle: () => NEVER });

    await router.navigateByUrl('/surveys');

    expect(router.url).toBe('/surveys');
  });

  it.each(['/my', '/voted', '/surveys/new', '/surveys/abc/edit'])('sends guests from %s to login and keeps the target', async (path) => {
    const router = setup(guest());

    await router.navigateByUrl(path);

    expect(router.url).toBe(`/login?redirect=${encodeURIComponent(path)}`);
  });

  it('sends signed-in users away from the login page to the redirect target', async () => {
    const router = setup(new MockBackend({ 'GET /api/auth/me': reply(200, account()) }));

    await router.navigateByUrl('/login?redirect=/my');

    expect(router.url).toBe('/my');
  });

  it('ignores a redirect to another site', async () => {
    const router = setup(new MockBackend({ 'GET /api/auth/me': reply(200, account()) }));

    await router.navigateByUrl('/login?redirect=//evil.example');

    expect(router.url).toBe('/surveys');
  });

  it('shows unknown paths without asking for a login', async () => {
    const router = setup(guest());

    await router.navigateByUrl('/no/such/page');

    expect(router.url).toBe('/no/such/page');
  });
});

describe('safeRedirect', () => {
  it.each([
    ['/surveys/abc', '/surveys/abc'],
    ['/my?status=draft', '/my?status=draft'],
    ['//evil.example', null],
    ['https://evil.example', null],
    ['javascript:alert(1)', null],
    [undefined, null],
    [['/a', '/b'], null],
  ])('%s -> %s', (input, expected) => {
    expect(safeRedirect(input)).toBe(expected);
  });
});
