import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { AuthStore } from '../core/stores/auth.store';
import { LOCALES, LOCALE_NAMES } from '../core/i18n/i18n';
import { PrefsStore } from '../core/stores/prefs.store';
import { ThemeToggle } from '../shared/prefs-controls';
import { UserAvatar } from '../shared/user-avatar';

/** Avatar in the header; opens settings, password change, language and theme, and sign-out. */
@Component({
  selector: 'app-user-menu',
  imports: [TranslocoDirective, RouterLink, MatMenu, MatMenuItem, MatMenuTrigger, MatIcon, UserAvatar, ThemeToggle],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (auth.user(); as user) {
      <ng-container *transloco="let t">
        <button
          type="button"
          class="toggle"
          [matMenuTriggerFor]="menu"
          [attr.aria-label]="t('menu.open')"
          [title]="t('nav.signedInAs', { name: user.displayName, email: user.email })"
        >
          <app-user-avatar [name]="user.displayName" [url]="user.avatarUrl" [size]="34" />
        </button>

        <mat-menu #menu="matMenu" xPosition="before" class="user-menu-panel">
          <div class="who">
            <app-user-avatar [name]="user.displayName" [url]="user.avatarUrl" [size]="44" />
            <div class="who-text">
              <strong>{{ user.displayName }}</strong>
              <span class="muted">{{ user.email }}</span>
            </div>
          </div>
          <a mat-menu-item routerLink="/account"><mat-icon svgIcon="settings" />{{ t('menu.settings') }}</a>
          <a mat-menu-item routerLink="/account/password"><mat-icon svgIcon="key" />{{ t('menu.changePassword') }}</a>
          <button mat-menu-item [matMenuTriggerFor]="languages">
            <span class="lang-item">{{ t('prefs.language') }}<span class="muted">{{ localeNames[prefs.locale()] }}</span></span>
          </button>
          <!-- Clicks on the theme switch must not close the menu. -->
          <div class="prefs" (click)="$event.stopPropagation()">
            <span class="muted">{{ t('prefs.theme') }}</span>
            <app-theme-toggle />
          </div>
          <button mat-menu-item class="danger" (click)="logout()"><mat-icon svgIcon="logout" />{{ t('nav.logout') }}</button>
        </mat-menu>
        <mat-menu #languages="matMenu">
          @for (code of locales; track code) {
            <button mat-menu-item [attr.lang]="code" (click)="prefs.setLocale(code)">
              @if (code === prefs.locale()) {
                <mat-icon svgIcon="check" />
              } @else {
                <mat-icon />
              }
              {{ localeNames[code] }}
            </button>
          }
        </mat-menu>
      </ng-container>
    }
  `,
  styles: `
    .toggle {
      display: grid;
      padding: 2px;
      border: 0;
      border-radius: 50%;
      background: none;
      cursor: pointer;
    }
    .toggle:focus-visible {
      outline: 2px solid var(--mat-sys-primary);
      outline-offset: 2px;
    }
    .who {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 10px 16px 12px;
      min-width: 260px;
    }
    .who-text {
      display: grid;
      min-width: 0;
    }
    .who-text span {
      overflow: hidden;
      text-overflow: ellipsis;
      font-size: 0.86rem;
    }
    .lang-item {
      display: flex;
      justify-content: space-between;
      gap: 16px;
      width: 100%;
    }
    .prefs {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      padding: 8px 16px;
      margin: 4px 0;
      border-block: 1px solid var(--app-border);
    }
    .danger {
      color: var(--app-danger);
      --mat-menu-item-icon-color: var(--app-danger);
    }
  `,
})
export class UserMenu {
  protected readonly auth = inject(AuthStore);
  protected readonly prefs = inject(PrefsStore);
  private readonly router = inject(Router);
  protected readonly locales = LOCALES;
  protected readonly localeNames = LOCALE_NAMES;

  protected async logout(): Promise<void> {
    await this.auth.logout().catch(() => {});
    await this.router.navigateByUrl('/surveys');
  }
}
