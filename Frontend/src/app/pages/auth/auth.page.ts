import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatHint, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { safeRedirect } from '../../core/auth.guard';
import { errorMessage, showErrors } from '../../core/form-errors';
import { ErrorTexts } from '../../core/i18n/error-texts';
import { LIMITS } from '../../core/limits';
import { AuthStore } from '../../core/stores/auth.store';
import { PrefsStore } from '../../core/stores/prefs.store';
import { Toasts } from '../../core/toasts';
import { displayNameError, emailError, passwordError } from '../../core/utils/account-rules';
import { PasswordField } from '../../shared/password-field';
import { AuthCard } from './auth-card';

@Component({
  selector: 'app-auth-page',
  imports: [TranslocoDirective, ReactiveFormsModule, RouterLink, MatFormField, MatLabel, MatInput, MatHint, MatError, MatButton, AuthCard, PasswordField],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ng-container *transloco="let t">
      <app-auth-card
        [title]="isRegister() ? t('auth.registerTitle') : t('auth.loginTitle')"
        [subtitle]="isRegister() ? t('auth.registerSubtitle') : t('auth.loginSubtitle')"
      >
        <form [formGroup]="form" novalidate (ngSubmit)="submit()">
          @if (redirect()?.startsWith('/surveys/')) {
            <p class="alert">{{ t('auth.redirectNotice') }}</p>
          }
          @if (formError()) {
            <p class="alert error" role="alert">{{ formError() }}</p>
          }

          <mat-form-field class="full">
            <mat-label>{{ t('auth.email') }}</mat-label>
            <input
              matInput
              id="email"
              formControlName="email"
              type="email"
              name="email"
              autocomplete="email"
              autocapitalize="none"
              spellcheck="false"
              [maxlength]="limits.emailMax"
            />
            <mat-error>{{ error('email') }}</mat-error>
          </mat-form-field>

          @if (isRegister()) {
            <mat-form-field class="full" subscriptSizing="dynamic">
              <mat-label>{{ t('auth.displayName') }}</mat-label>
              <input matInput id="display-name" formControlName="displayName" name="name" autocomplete="nickname" [maxlength]="limits.displayNameMax" />
              <mat-hint>{{ t('auth.displayNameHint') }}</mat-hint>
              <mat-error>{{ error('displayName') }}</mat-error>
            </mat-form-field>
          }

          <app-password-field
            inputId="password"
            [control]="form.controls.password"
            [label]="t('auth.password')"
            [autocomplete]="isRegister() ? 'new-password' : 'current-password'"
            [hint]="isRegister() ? t('auth.passwordHint', { min: limits.passwordMin }) : undefined"
          />

          @if (isRegister()) {
            <app-password-field
              inputId="password-repeat"
              [control]="form.controls.passwordRepeat"
              [label]="t('auth.passwordRepeat')"
              autocomplete="new-password"
            />
          }

          @if (!isRegister() && auth.config().emailEnabled) {
            <a routerLink="/forgot-password" class="forgot">{{ t('auth.forgotPassword') }}</a>
          }

          <button mat-flat-button type="submit" class="submit" [disabled]="submitting()">
            {{ isRegister() ? t('auth.submitRegister') : t('auth.submitLogin') }}
          </button>

          <p class="switch muted">
            {{ isRegister() ? t('auth.haveAccount') : t('auth.noAccount') }}
            <a [routerLink]="isRegister() ? '/login' : '/register'" [queryParams]="redirect() ? { redirect: redirect() } : {}">
              {{ isRegister() ? t('nav.login') : t('nav.register') }}
            </a>
          </p>
        </form>
      </app-auth-card>
    </ng-container>
  `,
  styles: `
    form {
      display: grid;
      gap: 6px;
    }
    /* Fields with a long hint take the height they need. */
    mat-form-field[subscriptSizing='dynamic'],
    app-password-field {
      margin-bottom: 14px;
    }
    .alert {
      margin-bottom: 10px;
    }
    .forgot {
      justify-self: end;
      margin: -4px 0 6px;
      font-size: 0.9rem;
    }
    .submit {
      height: 44px;
      margin-top: 6px;
    }
    .switch {
      margin-top: 10px;
      text-align: center;
      font-size: 0.93rem;
    }
  `,
})
export class AuthPage {
  /** From the route data. */
  readonly mode = input.required<'login' | 'register'>();
  /** From the query string: where to go after signing in. */
  readonly redirectParam = input<string | undefined>(undefined, { alias: 'redirect' });

  protected readonly auth = inject(AuthStore);
  private readonly prefs = inject(PrefsStore);
  private readonly router = inject(Router);
  private readonly toasts = inject(Toasts);
  private readonly errors = inject(ErrorTexts);
  private readonly transloco = inject(TranslocoService);

  protected readonly limits = LIMITS;
  protected readonly form = inject(NonNullableFormBuilder).group({ email: '', displayName: '', password: '', passwordRepeat: '' });
  protected readonly formError = signal('');
  protected readonly submitting = signal(false);
  protected readonly isRegister = computed(() => this.mode() === 'register');
  protected readonly redirect = computed(() => safeRedirect(this.redirectParam()));

  constructor() {
    effect(() => {
      this.mode();
      this.formError.set('');
    });
  }

  protected error(name: keyof typeof this.form.controls): string {
    return errorMessage(this.form.controls[name]);
  }

  /** Same rules as the API, so most mistakes are shown without a round trip. */
  private validate(): Record<string, string> {
    const { email, displayName, password, passwordRepeat } = this.form.getRawValue();
    const required = this.transloco.translate('errors.required');
    const result: Record<string, string> = {};
    if (!this.isRegister()) {
      if (!email.trim()) result['email'] = required;
      if (!password) result['password'] = required;
      return result;
    }
    const checks: [string, string | null][] = [
      ['email', emailError(email)],
      ['displayName', displayNameError(displayName)],
      ['password', passwordError(password, email)],
    ];
    for (const [field, code] of checks) if (code) result[field] = this.errors.code(code);
    if (passwordRepeat !== password) result['passwordRepeat'] = this.transloco.translate('auth.passwordsMismatch');
    return result;
  }

  protected async submit(): Promise<void> {
    this.formError.set('');
    const problems = this.validate();
    if (Object.keys(problems).length > 0) {
      showErrors(this.form, problems);
      return;
    }

    this.submitting.set(true);
    try {
      const { email, displayName, password } = this.form.getRawValue();
      const user = this.isRegister()
        ? await this.auth.register({ email: email.trim(), displayName: displayName.trim(), password, locale: this.prefs.locale() })
        : await this.auth.login({ email: email.trim(), password });
      this.toasts.success(this.transloco.translate('toast.welcome', { name: user.displayName }));
      await this.router.navigateByUrl(this.redirect() ?? '/surveys', { replaceUrl: true });
    } catch (error) {
      const fields = this.errors.fields(error);
      if (Object.keys(fields).length > 0) showErrors(this.form, fields);
      else this.formError.set(this.errors.message(error));
    } finally {
      this.submitting.set(false);
    }
  }
}
