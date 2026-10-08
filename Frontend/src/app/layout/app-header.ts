import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatAnchor } from '@angular/material/button';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { filter, map } from 'rxjs';
import { AuthStore } from '../core/stores/auth.store';
import { PrefsControls } from '../shared/prefs-controls';
import { UserMenu } from './user-menu';

@Component({
  selector: 'app-header',
  imports: [TranslocoDirective, RouterLink, RouterLinkActive, MatAnchor, PrefsControls, UserMenu],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header *transloco="let t">
      <div class="bar">
        <a routerLink="/surveys" class="brand">
          <!-- Inline, so the logo is there on the first paint instead of waiting for another request. -->
          <svg width="28" height="28" viewBox="0 0 32 32" aria-hidden="true" focusable="false">
            <rect width="32" height="32" rx="8" fill="#4f46e5" />
            <rect x="7" y="17" width="4" height="8" rx="1.5" fill="#fff" />
            <rect x="14" y="11" width="4" height="14" rx="1.5" fill="#fff" />
            <rect x="21" y="7" width="4" height="18" rx="1.5" fill="#fff" />
          </svg>
          <span>{{ t('app.name') }}</span>
        </a>

        <nav aria-label="Main">
          <a routerLink="/surveys" routerLinkActive="active" ariaCurrentWhenActive="page" [routerLinkActiveOptions]="{ exact: true }">
            {{ t('nav.active') }}
          </a>
          @if (auth.isLoggedIn()) {
            <a routerLink="/my" routerLinkActive="active" ariaCurrentWhenActive="page">{{ t('nav.mine') }}</a>
            <a routerLink="/voted" routerLinkActive="active" ariaCurrentWhenActive="page">{{ t('nav.voted') }}</a>
          }
        </nav>

        <div class="actions">
          <!-- Until the session is known, a placeholder: no flash of the guest buttons for a signed-in user. -->
          @if (!auth.initialized()) {
            <span class="skeleton avatar-placeholder" aria-hidden="true"></span>
          } @else if (auth.isLoggedIn()) {
            <app-user-menu />
          } @else {
            <app-prefs-controls class="guest-prefs" />
            <a mat-button routerLink="/login" [queryParams]="loginQuery()">{{ t('nav.login') }}</a>
            <a mat-flat-button routerLink="/register" [queryParams]="loginQuery()">{{ t('nav.register') }}</a>
          }
        </div>
      </div>
    </header>
  `,
  styles: `
    header {
      position: sticky;
      top: 0;
      z-index: 10;
      background: color-mix(in srgb, var(--mat-sys-surface-container-lowest) 88%, transparent);
      backdrop-filter: saturate(1.4) blur(10px);
      border-bottom: 1px solid var(--app-border);
    }
    .bar {
      display: flex;
      align-items: center;
      gap: 8px 20px;
      max-width: 1040px;
      min-height: 60px;
      margin: 0 auto;
      padding: 8px 16px;
      flex-wrap: wrap;
    }
    .brand {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      font-weight: 700;
      font-size: 1.1rem;
      color: inherit;
      text-decoration: none;
    }
    nav {
      display: flex;
      gap: 4px;
      flex: 1;
      overflow-x: auto;
    }
    nav a {
      padding: 7px 12px;
      border-radius: 999px;
      color: var(--app-muted);
      text-decoration: none;
      font-weight: 500;
      white-space: nowrap;
    }
    nav a:hover {
      background: var(--mat-sys-surface-container);
      color: var(--mat-sys-on-surface);
    }
    nav a.active {
      background: var(--mat-sys-secondary-container);
      color: var(--mat-sys-on-secondary-container);
    }
    .actions {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-left: auto;
    }
    .avatar-placeholder {
      width: 34px;
      height: 34px;
      border-radius: 50%;
    }
    /* On phones the nav takes its own row under the logo and the buttons. */
    @media (max-width: 640px) {
      nav {
        order: 3;
        flex-basis: 100%;
        margin: 0 -6px;
      }
      .guest-prefs {
        display: none;
      }
    }
  `,
})
export class AppHeader {
  protected readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  private readonly url = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map(() => this.router.url),
    ),
    { initialValue: this.router.url },
  );

  /** From a survey page, signing in should bring the guest back to that survey. */
  protected readonly loginQuery = computed(() => {
    const url = this.url();
    return /^\/surveys\/[^/?]+$/.test(url.split('?')[0]!) && url !== '/surveys/new' ? { redirect: url } : {};
  });
}
