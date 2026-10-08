import { HttpEvent, HttpRequest } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import { Observable, Subject } from 'rxjs';
import { AuthStore } from '../core/stores/auth.store';
import { account, reply, respond } from '../testing/fixtures';
import { appProviders } from '../testing/test-app';
import { AppHeader } from './app-header';

const settle = () => new Promise((resolve) => setTimeout(resolve, 20));

describe('AppHeader while the session is loading', () => {
  beforeEach(() => {
    localStorage.clear();
    localStorage.setItem('qanda.locale', 'en');
  });

  /** /me stays pending until the test answers it. */
  async function renderWithPendingSession() {
    const me = new Subject<HttpEvent<unknown>>();
    let meRequest!: HttpRequest<unknown>;
    const backend = {
      handle: (request: HttpRequest<unknown>): Observable<HttpEvent<unknown>> => {
        if (request.url === '/api/auth/me') {
          meRequest = request;
          return me;
        }
        return respond(request, reply(200, { emailEnabled: false, confirmationRequired: false }));
      },
    };
    const result = await render(AppHeader, { providers: appProviders(backend) });
    void TestBed.inject(AuthStore).init();
    // Not whenStable(): it also waits for HTTP requests, and /me is pending on purpose.
    await settle();
    const answer = async (status: number, body?: unknown) => {
      respond(meRequest, reply(status, body)).subscribe(me);
      await settle();
    };
    return { ...result, answer };
  }

  it('shows a placeholder instead of the guest buttons, then the account menu', async () => {
    const { container, answer } = await renderWithPendingSession();

    expect(container.querySelector('.avatar-placeholder')).not.toBeNull();
    expect(screen.queryByRole('link', { name: 'Sign in' })).toBeNull();

    await answer(200, account());

    expect(container.querySelector('.avatar-placeholder')).toBeNull();
    expect(await screen.findByRole('button', { name: 'Account menu' })).toBeTruthy();
  });

  it('shows the guest buttons once the server says nobody is signed in', async () => {
    const { container, answer } = await renderWithPendingSession();

    await answer(401);

    expect(await screen.findByRole('link', { name: 'Sign in' })).toBeTruthy();
    expect(container.querySelector('.avatar-placeholder')).toBeNull();
  });
});
