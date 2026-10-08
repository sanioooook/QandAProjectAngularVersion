import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatAnchor, MatButton } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { ApiError } from '../../core/api/api-error';
import { showErrors } from '../../core/form-errors';
import { ErrorTexts } from '../../core/i18n/error-texts';
import { LIMITS } from '../../core/limits';
import { AuthStore } from '../../core/stores/auth.store';
import { Toasts } from '../../core/toasts';
import { passwordError } from '../../core/utils/account-rules';
import { PasswordField } from '../../shared/password-field';
import { AuthCard } from './auth-card';

/** Opened from the password reset email: /reset-password?token=... */
@Component({
  selector: 'app-reset-password-page',
  imports: [TranslocoDirective, ReactiveFormsModule, RouterLink, MatButton, MatAnchor, AuthCard, PasswordField],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ng-container *transloco="let t">
      <app-auth-card [title]="t('auth.resetTitle')" [subtitle]="invalid() ? undefined : t('auth.resetSubtitle')">
        @if (invalid()) {
          <p class="alert error" role="alert">{{ t('errors.token_invalid') }}</p>
          <a mat-flat-button routerLink="/forgot-password">{{ t('auth.requestNewLink') }}</a>
        } @else {
          <form [formGroup]="form" novalidate (ngSubmit)="submit()">
            @if (formError()) {
              <p class="alert error" role="alert">{{ formError() }}</p>
            }
            <app-password-field
              inputId="password"
              [control]="form.controls.password"
              [label]="t('auth.newPassword')"
              autocomplete="new-password"
              [hint]="t('auth.passwordHint', { min: limits.passwordMin })"
            />
            <app-password-field
              inputId="password-repeat"
              [control]="form.controls.passwordRepeat"
              [label]="t('auth.passwordRepeat')"
              autocomplete="new-password"
            />
            <button mat-flat-button type="submit" class="submit" [disabled]="submitting()">{{ t('auth.saveNewPassword') }}</button>
          </form>
        }
      </app-auth-card>
    </ng-container>
  `,
  styles: `
    form {
      display: grid;
      gap: 6px;
    }
    .submit {
      height: 44px;
    }
  `,
})
export class ResetPasswordPage {
  readonly token = input<string | undefined>();

  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly toasts = inject(Toasts);
  private readonly errors = inject(ErrorTexts);
  private readonly transloco = inject(TranslocoService);

  protected readonly limits = LIMITS;
  protected readonly form = inject(NonNullableFormBuilder).group({ password: '', passwordRepeat: '' });
  private readonly rejected = signal(false);
  protected readonly invalid = computed(() => !this.token() || this.rejected());
  protected readonly formError = signal('');
  protected readonly submitting = signal(false);

  protected async submit(): Promise<void> {
    this.formError.set('');
    const { password, passwordRepeat } = this.form.getRawValue();
    const problems: Record<string, string> = {};
    const code = passwordError(password, '');
    if (code) problems['password'] = this.errors.code(code);
    if (password !== passwordRepeat) problems['passwordRepeat'] = this.transloco.translate('auth.passwordsMismatch');
    if (Object.keys(problems).length) {
      showErrors(this.form, problems);
      return;
    }

    this.submitting.set(true);
    try {
      await this.auth.resetPassword(this.token()!, password);
      this.toasts.success(this.transloco.translate('toast.passwordChanged'));
      await this.router.navigateByUrl('/surveys', { replaceUrl: true });
    } catch (error) {
      if (error instanceof ApiError && error.code === 'token_invalid') this.rejected.set(true);
      const fields = this.errors.fields(error);
      if (Object.keys(fields).length) showErrors(this.form, fields);
      else this.formError.set(this.errors.message(error));
    } finally {
      this.submitting.set(false);
    }
  }
}
