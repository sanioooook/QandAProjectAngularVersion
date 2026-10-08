import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { NavigationEnd, NavigationError, NavigationCancel, Router, RouterOutlet } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { SessionEvents } from './core/api/api.interceptor';
import { AuthStore } from './core/stores/auth.store';
import { PrefsStore } from './core/stores/prefs.store';
import { Toasts } from './core/toasts';
import { AppHeader } from './layout/app-header';
import { ConfirmEmailBanner } from './layout/confirm-email-banner';
import { PageSkeleton } from './shared/skeletons';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, TranslocoDirective, AppHeader, ConfirmEmailBanner, PageSkeleton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a class="skip-link" href="#main" *transloco="let t">{{ t('app.skipToContent') }}</a>
    <app-header />
    <app-confirm-email-banner />
    <main id="main" class="container">
      <router-outlet />
      <!-- First visit to a page that needs the session: a page skeleton until the server answers. -->
      @if (!navigated()) {
        <app-page-skeleton />
      }
    </main>
  `,
})
export class App {
  private readonly auth = inject(AuthStore);
  private readonly prefs = inject(PrefsStore);
  private readonly router = inject(Router);
  private readonly toasts = inject(Toasts);
  private readonly transloco = inject(TranslocoService);

  protected readonly navigated = signal(false);

  constructor() {
    const events = this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd || event instanceof NavigationCancel || event instanceof NavigationError) {
        this.navigated.set(true);
      }
    });

    // A request was rejected with 401: the session ended on the server. Sign in again and come back.
    const expired = inject(SessionEvents).expired.subscribe(() => {
      if (!this.auth.isLoggedIn()) return;
      this.auth.sessionExpired();
      this.toasts.error(this.transloco.translate('toast.sessionExpired'));
      void this.router.navigate(['/login'], { queryParams: { redirect: this.router.url } });
    });

    inject(DestroyRef).onDestroy(() => {
      events.unsubscribe();
      expired.unsubscribe();
    });

    // Emails go out in the account's language: keep it equal to the language chosen in the UI.
    effect(() => {
      const user = this.auth.user();
      const locale = this.prefs.locale();
      if (user && user.locale !== locale) void this.auth.updateProfile({ locale }).catch(() => {});
    });
  }
}
