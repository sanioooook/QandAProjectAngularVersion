import { ChangeDetectionStrategy, Component, OnInit, inject, input, signal } from '@angular/core';
import { MatAnchor } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { AuthStore } from '../../core/stores/auth.store';
import { AuthCard } from './auth-card';

/** Opened from the confirmation email: /confirm-email?token=... */
@Component({
  selector: 'app-confirm-email-page',
  imports: [TranslocoDirective, RouterLink, MatAnchor, AuthCard],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ng-container *transloco="let t">
      <app-auth-card [title]="t('auth.confirmTitle')">
        @switch (state()) {
          @case ('pending') {
            <p class="muted" aria-busy="true">{{ t('auth.confirming') }}</p>
          }
          @case ('done') {
            <p class="alert" role="status">{{ t('auth.confirmed') }}</p>
            <a mat-flat-button routerLink="/surveys">{{ t('notFound.home') }}</a>
          }
          @default {
            <p class="alert error" role="alert">{{ t('errors.token_invalid') }}</p>
            <p class="muted">{{ auth.isLoggedIn() ? t('auth.confirmRetrySignedIn') : t('auth.confirmRetry') }}</p>
            <a mat-stroked-button [routerLink]="auth.isLoggedIn() ? '/surveys' : '/login'">
              {{ auth.isLoggedIn() ? t('notFound.home') : t('nav.login') }}
            </a>
          }
        }
      </app-auth-card>
    </ng-container>
  `,
})
export class ConfirmEmailPage implements OnInit {
  readonly token = input<string | undefined>();
  protected readonly auth = inject(AuthStore);
  protected readonly state = signal<'pending' | 'done' | 'failed'>('pending');

  // Confirming is idempotent and harmless, so the link confirms on open (no extra click).
  async ngOnInit(): Promise<void> {
    const token = this.token();
    if (!token) {
      this.state.set('failed');
      return;
    }
    try {
      await this.auth.confirmEmail(token);
      this.state.set('done');
    } catch {
      this.state.set('failed');
    }
  }
}
