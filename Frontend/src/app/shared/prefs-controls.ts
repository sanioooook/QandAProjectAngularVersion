import { ChangeDetectionStrategy, Component, forwardRef, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatButtonToggle, MatButtonToggleGroup } from '@angular/material/button-toggle';
import { MatIcon } from '@angular/material/icon';
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { TranslocoDirective } from '@jsverse/transloco';
import { LOCALES, LOCALE_NAMES } from '../core/i18n/i18n';
import { PrefsStore, Theme } from '../core/stores/prefs.store';
import { IconName } from './icons';

// Short labels keep the header on one line; "UA" rather than the ISO "UK" to avoid reading it as the United Kingdom.
const LOCALE_LABELS = { uk: 'UA', en: 'EN', ru: 'RU' } as const;

/** Language and theme switches: in the header for guests, in the account menu when signed in. */
@Component({
  selector: 'app-prefs-controls',
  imports: [TranslocoDirective, MatButton, MatMenu, MatMenuItem, MatMenuTrigger, forwardRef(() => ThemeToggle)],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ng-container *transloco="let t">
      <button
        mat-button
        class="locale"
        [matMenuTriggerFor]="languages"
        [attr.aria-label]="t('prefs.language') + ': ' + names[prefs.locale()]"
        [title]="t('prefs.language') + ': ' + names[prefs.locale()]"
      >
        {{ labels[prefs.locale()] }}
      </button>
      <mat-menu #languages="matMenu">
        @for (code of locales; track code) {
          <button mat-menu-item [attr.lang]="code" (click)="prefs.setLocale(code)">{{ names[code] }}</button>
        }
      </mat-menu>

      <app-theme-toggle />
    </ng-container>
  `,
  styles: `
    :host {
      display: inline-flex;
      align-items: center;
      gap: 6px;
    }
    .locale {
      min-width: 0;
      padding: 0 10px;
      font-weight: 600;
    }
  `,
})
export class PrefsControls {
  protected readonly prefs = inject(PrefsStore);
  protected readonly locales = LOCALES;
  protected readonly names = LOCALE_NAMES;
  protected readonly labels = LOCALE_LABELS;
}

/** Light / dark / system, as three icon buttons. */
@Component({
  selector: 'app-theme-toggle',
  imports: [TranslocoDirective, MatButtonToggleGroup, MatButtonToggle, MatIcon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-button-toggle-group
      *transloco="let t"
      class="theme"
      hideSingleSelectionIndicator
      [value]="prefs.theme()"
      (change)="prefs.setTheme($event.value)"
      [attr.aria-label]="t('prefs.theme')"
    >
      @for (item of themes; track item.value) {
        <mat-button-toggle [value]="item.value" [aria-label]="t('prefs.' + item.value)" [title]="t('prefs.' + item.value)">
          <mat-icon [svgIcon]="item.icon" />
        </mat-button-toggle>
      }
    </mat-button-toggle-group>
  `,
  styles: `
    .theme {
      --mat-button-toggle-height: 32px;
      --mat-button-toggle-shape: 999px;
    }
    .theme mat-icon {
      width: 16px;
      height: 16px;
      vertical-align: middle;
    }
    .theme ::ng-deep .mat-button-toggle-label-content {
      padding: 0 9px;
    }
  `,
})
export class ThemeToggle {
  protected readonly prefs = inject(PrefsStore);
  protected readonly themes: { value: Theme; icon: IconName }[] = [
    { value: 'light', icon: 'sun' },
    { value: 'dark', icon: 'moon' },
    { value: 'system', icon: 'monitor' },
  ];
}
