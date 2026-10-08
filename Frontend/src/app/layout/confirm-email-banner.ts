import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { ErrorTexts } from '../core/i18n/error-texts';
import { injectNow } from '../core/now';
import { AuthStore } from '../core/stores/auth.store';
import { Toasts } from '../core/toasts';

const RESEND_COOLDOWN_MS = 60_000;

/** Shown while the server requires a confirmed email and this account has not confirmed it yet. */
@Component({
  selector: 'app-confirm-email-banner',
  imports: [TranslocoDirective, MatButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (auth.needsConfirmation()) {
      <div class="banner" role="status" *transloco="let t">
        <div class="inner">
          <p>{{ t('auth.confirmBanner', { email: auth.user()?.email }) }}</p>
          <button mat-stroked-button [disabled]="sending() || wait() > 0" (click)="resend()">
            {{ wait() > 0 ? t('auth.resendIn', { s: wait() }) : t('auth.resend') }}
          </button>
        </div>
      </div>
    }
  `,
  styles: `
    .banner {
      background: var(--app-warning-soft);
      color: light-dark(#713f12, #fde68a);
      border-bottom: 1px solid color-mix(in srgb, var(--app-warning) 30%, transparent);
    }
    .inner {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: space-between;
      gap: 8px 16px;
      max-width: 1040px;
      margin: 0 auto;
      padding: 10px 16px;
      font-size: 0.93rem;
    }
  `,
})
export class ConfirmEmailBanner {
  protected readonly auth = inject(AuthStore);
  private readonly toasts = inject(Toasts);
  private readonly errors = inject(ErrorTexts);
  private readonly transloco = inject(TranslocoService);
  private readonly now = injectNow(1000);

  private readonly sentAt = signal(0);
  protected readonly sending = signal(false);
  protected readonly wait = computed(() => Math.max(0, Math.ceil((this.sentAt() + RESEND_COOLDOWN_MS - this.now()) / 1000)));

  protected async resend(): Promise<void> {
    this.sending.set(true);
    try {
      await this.auth.resendConfirmation();
      this.sentAt.set(Date.now());
      this.toasts.success(this.transloco.translate('toast.confirmationSent'));
    } catch (error) {
      this.toasts.error(this.errors.message(error));
    } finally {
      this.sending.set(false);
    }
  }
}
