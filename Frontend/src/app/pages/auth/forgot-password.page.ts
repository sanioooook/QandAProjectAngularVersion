import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { errorMessage } from '../../core/form-errors';
import { ErrorTexts } from '../../core/i18n/error-texts';
import { LIMITS } from '../../core/limits';
import { AuthStore } from '../../core/stores/auth.store';
import { PrefsStore } from '../../core/stores/prefs.store';
import { emailError } from '../../core/utils/account-rules';
import { AuthCard } from './auth-card';

@Component({
  selector: 'app-forgot-password-page',
  imports: [TranslocoDirective, ReactiveFormsModule, RouterLink, MatFormField, MatLabel, MatInput, MatError, MatButton, AuthCard],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ng-container *transloco="let t">
      <app-auth-card [title]="t('auth.forgotTitle')" [subtitle]="sent() ? undefined : t('auth.forgotSubtitle')">
        @if (!auth.config().emailEnabled) {
          <p class="alert warning">{{ t('auth.emailUnavailable') }}</p>
        } @else if (sent()) {
          <p class="alert" role="status">{{ t('auth.forgotSent', { email: email.value }) }}</p>
        } @else {
          <form novalidate (ngSubmit)="submit()">
            @if (formError()) {
              <p class="alert error" role="alert">{{ formError() }}</p>
            }
            <mat-form-field class="full">
              <mat-label>{{ t('auth.email') }}</mat-label>
              <input matInput id="email" type="email" autocomplete="email" [formControl]="email" [maxlength]="limits.emailMax" />
              <mat-error>{{ message() }}</mat-error>
            </mat-form-field>
            <button mat-flat-button type="submit" class="submit" [disabled]="submitting()">{{ t('auth.sendResetLink') }}</button>
          </form>
        }
        <a routerLink="/login" class="back">{{ t('auth.backToLogin') }}</a>
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
    .back {
      justify-self: center;
      font-size: 0.93rem;
    }
  `,
})
export class ForgotPasswordPage {
  protected readonly auth = inject(AuthStore);
  private readonly prefs = inject(PrefsStore);
  private readonly errors = inject(ErrorTexts);

  protected readonly limits = LIMITS;
  protected readonly email = new FormControl(this.auth.user()?.email ?? '', { nonNullable: true });
  protected readonly sent = signal(false);
  protected readonly submitting = signal(false);
  protected readonly formError = signal('');

  protected message(): string {
    return errorMessage(this.email);
  }

  protected async submit(): Promise<void> {
    this.formError.set('');
    const code = emailError(this.email.value);
    if (code) {
      this.email.setErrors({ message: this.errors.code(code) });
      this.email.markAsTouched();
      return;
    }
    this.submitting.set(true);
    try {
      await this.auth.forgotPassword(this.email.value.trim(), this.prefs.locale());
      this.sent.set(true);
    } catch (error) {
      this.formError.set(this.errors.message(error));
    } finally {
      this.submitting.set(false);
    }
  }
}
