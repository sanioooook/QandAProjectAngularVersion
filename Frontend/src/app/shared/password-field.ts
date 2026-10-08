import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatHint, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { TranslocoDirective } from '@jsverse/transloco';
import { errorMessage } from '../core/form-errors';
import { LIMITS } from '../core/limits';

/** Password input with a show/hide toggle. */
@Component({
  selector: 'app-password-field',
  imports: [TranslocoDirective, ReactiveFormsModule, MatFormField, MatLabel, MatInput, MatHint, MatError, MatSuffix, MatIconButton, MatIcon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-form-field class="full" [subscriptSizing]="hint() ? 'dynamic' : 'fixed'" *transloco="let t">
      <mat-label>{{ label() }}</mat-label>
      <input
        matInput
        [id]="inputId()"
        [formControl]="control()"
        [type]="visible() ? 'text' : 'password'"
        [attr.autocomplete]="autocomplete()"
        [maxlength]="maxLength"
        spellcheck="false"
        autocapitalize="none"
      />
      <button
        mat-icon-button
        matSuffix
        type="button"
        [attr.aria-label]="visible() ? t('auth.hidePassword') : t('auth.showPassword')"
        [attr.aria-pressed]="visible()"
        (click)="visible.set(!visible())"
      >
        <mat-icon [svgIcon]="visible() ? 'eyeOff' : 'eye'" />
      </button>
      @if (hint()) {
        <mat-hint>{{ hint() }}</mat-hint>
      }
      <mat-error>{{ message() }}</mat-error>
    </mat-form-field>
  `,
  styles: `
    :host {
      display: block;
    }
  `,
})
export class PasswordField {
  readonly control = input.required<FormControl<string>>();
  readonly label = input.required<string>();
  readonly inputId = input.required<string>();
  readonly autocomplete = input('current-password');
  readonly hint = input<string>();

  protected readonly visible = signal(false);
  protected readonly maxLength = LIMITS.passwordMax;

  protected message(): string {
    return errorMessage(this.control());
  }
}
