import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatAnchor, MatButton } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { showErrors } from '../../core/form-errors';
import { ErrorTexts } from '../../core/i18n/error-texts';
import { LIMITS } from '../../core/limits';
import { AuthStore } from '../../core/stores/auth.store';
import { Toasts } from '../../core/toasts';
import { passwordError } from '../../core/utils/account-rules';
import { PasswordField } from '../../shared/password-field';

@Component({
  selector: 'app-change-password-page',
  imports: [TranslocoDirective, ReactiveFormsModule, RouterLink, MatButton, MatAnchor, PasswordField],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section *transloco="let t">
      <div class="page-head">
        <div>
          <h1>{{ t('menu.changePassword') }}</h1>
          <p class="muted">{{ t('account.changePasswordHint') }}</p>
        </div>
      </div>
      <form class="card" [formGroup]="form" novalidate (ngSubmit)="submit()">
        @if (formError()) {
          <p class="alert error" role="alert">{{ formError() }}</p>
        }
        <app-password-field
          inputId="current-password"
          [control]="form.controls.currentPassword"
          [label]="t('account.currentPassword')"
          autocomplete="current-password"
        />
        @if (auth.config().emailEnabled) {
          <a routerLink="/forgot-password" class="forgot">{{ t('auth.forgotPassword') }}</a>
        }
        <app-password-field
          inputId="new-password"
          [control]="form.controls.newPassword"
          [label]="t('auth.newPassword')"
          autocomplete="new-password"
          [hint]="t('auth.passwordHint', { min: limits.passwordMin })"
        />
        <app-password-field
          inputId="repeat-password"
          [control]="form.controls.repeat"
          [label]="t('auth.passwordRepeat')"
          autocomplete="new-password"
        />
        <div class="buttons">
          <a mat-button routerLink="/account">{{ t('form.cancel') }}</a>
          <button mat-flat-button type="submit" [disabled]="saving()">{{ t('auth.saveNewPassword') }}</button>
        </div>
      </form>
    </section>
  `,
  styles: `
    section {
      max-width: 520px;
      margin: 0 auto;
    }
    .card {
      display: grid;
      gap: 6px;
      padding: clamp(18px, 3vw, 26px);
      border: 1px solid var(--app-border);
      border-radius: var(--app-radius);
      background: var(--mat-sys-surface-container-lowest);
      box-shadow: var(--app-shadow);
    }
    .forgot {
      justify-self: end;
      margin: -4px 0 6px;
      font-size: 0.9rem;
    }
    .buttons {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      margin-top: 8px;
    }
  `,
})
export class ChangePasswordPage {
  protected readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly toasts = inject(Toasts);
  private readonly errors = inject(ErrorTexts);
  private readonly transloco = inject(TranslocoService);

  protected readonly limits = LIMITS;
  protected readonly form = inject(NonNullableFormBuilder).group({ currentPassword: '', newPassword: '', repeat: '' });
  protected readonly formError = signal('');
  protected readonly saving = signal(false);

  protected async submit(): Promise<void> {
    this.formError.set('');
    const { currentPassword, newPassword, repeat } = this.form.getRawValue();
    const problems: Record<string, string> = {};
    if (!currentPassword) problems['currentPassword'] = this.transloco.translate('errors.required');
    const code = passwordError(newPassword, this.auth.user()?.email ?? '');
    if (code) problems['newPassword'] = this.errors.code(code);
    else if (newPassword === currentPassword) problems['newPassword'] = this.transloco.translate('account.samePassword');
    if (repeat !== newPassword) problems['repeat'] = this.transloco.translate('auth.passwordsMismatch');
    if (Object.keys(problems).length) {
      showErrors(this.form, problems);
      return;
    }

    this.saving.set(true);
    try {
      await this.auth.changePassword(currentPassword, newPassword);
      this.toasts.success(this.transloco.translate('toast.passwordChanged'));
      await this.router.navigateByUrl('/account');
    } catch (error) {
      const fields = this.errors.fields(error);
      if (Object.keys(fields).length) showErrors(this.form, fields);
      else this.formError.set(this.errors.message(error));
    } finally {
      this.saving.set(false);
    }
  }
}
