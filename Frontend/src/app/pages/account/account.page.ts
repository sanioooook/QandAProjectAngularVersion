import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatAnchor, MatButton } from '@angular/material/button';
import { MatButtonToggle, MatButtonToggleGroup } from '@angular/material/button-toggle';
import { MatOption } from '@angular/material/core';
import { MatError, MatFormField, MatHint, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatSelect } from '@angular/material/select';
import { RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { ApiError } from '../../core/api/api-error';
import { errorMessage } from '../../core/form-errors';
import { ErrorTexts } from '../../core/i18n/error-texts';
import { LOCALES, LOCALE_NAMES } from '../../core/i18n/i18n';
import { LIMITS } from '../../core/limits';
import { AuthStore } from '../../core/stores/auth.store';
import { PrefsStore, Theme } from '../../core/stores/prefs.store';
import { Toasts } from '../../core/toasts';
import { displayNameError } from '../../core/utils/account-rules';
import { toSquareImage } from '../../core/utils/image';
import { allTimeZones, browserTimeZone, zoneName } from '../../core/utils/time-zone';
import { IconName } from '../../shared/icons';
import { UserAvatar } from '../../shared/user-avatar';

@Component({
  selector: 'app-account-page',
  imports: [
    TranslocoDirective,
    ReactiveFormsModule,
    RouterLink,
    MatButton,
    MatAnchor,
    MatIcon,
    MatFormField,
    MatLabel,
    MatInput,
    MatHint,
    MatError,
    MatSelect,
    MatOption,
    MatButtonToggleGroup,
    MatButtonToggle,
    UserAvatar,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './account.page.html',
  styleUrl: './account.page.scss',
})
export class AccountPage {
  protected readonly auth = inject(AuthStore);
  protected readonly prefs = inject(PrefsStore);
  private readonly toasts = inject(Toasts);
  private readonly errors = inject(ErrorTexts);
  private readonly transloco = inject(TranslocoService);

  protected readonly limits = LIMITS;
  protected readonly locales = LOCALES;
  protected readonly localeNames = LOCALE_NAMES;
  protected readonly themes: { value: Theme; icon: IconName }[] = [
    { value: 'light', icon: 'sun' },
    { value: 'dark', icon: 'moon' },
    { value: 'system', icon: 'monitor' },
  ];
  protected readonly zoneOptions = allTimeZones().map((zone) => ({ zone, label: zoneName(zone) }));
  protected readonly autoZoneLabel = zoneName(browserTimeZone());

  // --- avatar ---------------------------------------------------------------
  protected readonly avatarBusy = signal(false);
  protected readonly avatarError = signal('');

  protected async onFile(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    this.avatarError.set('');
    this.avatarBusy.set(true);
    try {
      // Cropped and compressed in the browser: the server only stores a small square image.
      await this.auth.uploadAvatar(await toSquareImage(file));
      this.toasts.success(this.transloco.translate('account.avatarSaved'));
    } catch (error) {
      this.avatarError.set(error instanceof ApiError ? this.errors.message(error) : this.transloco.translate('account.avatarUnreadable'));
    } finally {
      this.avatarBusy.set(false);
    }
  }

  protected async removeAvatar(): Promise<void> {
    this.avatarBusy.set(true);
    try {
      await this.auth.removeAvatar();
      this.toasts.success(this.transloco.translate('account.avatarRemoved'));
    } catch (error) {
      this.toasts.error(this.errors.message(error));
    } finally {
      this.avatarBusy.set(false);
    }
  }

  // --- name -----------------------------------------------------------------
  protected readonly name = new FormControl('', { nonNullable: true });
  protected readonly nameBusy = signal(false);
  private readonly nameValue = signal('');
  protected readonly nameUnchanged = computed(() => this.nameValue().trim() === this.auth.user()?.displayName);

  constructor() {
    effect(() => {
      const current = this.auth.user()?.displayName ?? '';
      this.name.setValue(current);
      this.nameValue.set(current);
    });
    this.name.valueChanges.pipe(takeUntilDestroyed()).subscribe((value) => this.nameValue.set(value));
  }

  protected nameError(): string {
    return errorMessage(this.name);
  }

  protected async saveName(): Promise<void> {
    const code = displayNameError(this.name.value);
    if (code) {
      this.name.setErrors({ message: this.errors.code(code) });
      this.name.markAsTouched();
      return;
    }
    this.nameBusy.set(true);
    try {
      await this.auth.updateProfile({ displayName: this.name.value.trim() });
      this.toasts.success(this.transloco.translate('toast.saved'));
    } catch (error) {
      this.name.setErrors({ message: this.errors.fields(error)['displayName'] ?? this.errors.message(error) });
      this.name.markAsTouched();
    } finally {
      this.nameBusy.set(false);
    }
  }

  // --- email ----------------------------------------------------------------
  protected readonly resendBusy = signal(false);

  protected async resend(): Promise<void> {
    this.resendBusy.set(true);
    try {
      await this.auth.resendConfirmation();
      this.toasts.success(this.transloco.translate('toast.confirmationSent'));
    } catch (error) {
      this.toasts.error(this.errors.message(error));
    } finally {
      this.resendBusy.set(false);
    }
  }
}
