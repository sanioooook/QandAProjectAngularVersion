import { DOCUMENT, computed, effect, inject } from '@angular/core';
import { DateAdapter } from '@angular/material/core';
import { TranslocoService } from '@jsverse/transloco';
import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { Locale, detectLocale, isLocale } from '../i18n/i18n';
import { browserTimeZone, currentName, isTimeZone } from '../utils/time-zone';

export type Theme = 'light' | 'dark' | 'system';
/** 'auto' follows the device; otherwise an IANA zone such as 'Europe/Kyiv'. */
export type TimeZonePref = 'auto' | string;

export const THEME_KEY = 'qanda.theme';
export const LOCALE_KEY = 'qanda.locale';
export const TIME_ZONE_KEY = 'qanda.timeZone';

// Storage can throw (privacy mode, blocked site data): preferences then just are not remembered.
function read(key: string): string | null {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

function write(key: string, value: string): void {
  try {
    localStorage.setItem(key, value);
  } catch {
    // ignore
  }
}

interface PrefsState {
  theme: Theme;
  locale: Locale;
  timeZone: TimeZonePref;
}

function initialState(): PrefsState {
  const theme = read(THEME_KEY);
  const locale = read(LOCALE_KEY);
  const zone = read(TIME_ZONE_KEY);
  return {
    theme: theme === 'light' || theme === 'dark' ? theme : 'system',
    locale: isLocale(locale) ? locale : detectLocale(),
    timeZone: isTimeZone(zone) ? currentName(zone) : 'auto',
  };
}

export const PrefsStore = signalStore(
  { providedIn: 'root' },
  withState<PrefsState>(initialState),
  withComputed(({ timeZone }) => ({
    /** The zone all dates are shown in and deadline times are entered in. */
    effectiveTimeZone: computed(() => (timeZone() === 'auto' ? browserTimeZone() : timeZone())),
  })),
  withMethods((store) => ({
    setTheme: (theme: Theme) => patchState(store, { theme }),
    setLocale: (locale: Locale) => patchState(store, { locale }),
    setTimeZone: (timeZone: TimeZonePref) => patchState(store, { timeZone }),
  })),
  withHooks((store) => {
    const document = inject(DOCUMENT);
    const transloco = inject(TranslocoService);
    const dateAdapter = inject(DateAdapter, { optional: true });
    return {
      onInit() {
        effect(() => {
          const theme = store.theme();
          // `color-scheme` on <html> drives Material's light-dark() colors; 'system' leaves it to the OS.
          if (theme === 'system') delete document.documentElement.dataset['theme'];
          else document.documentElement.dataset['theme'] = theme;
          write(THEME_KEY, theme);
        });
        effect(() => {
          const locale = store.locale();
          transloco.setActiveLang(locale);
          dateAdapter?.setLocale(locale);
          document.documentElement.lang = locale;
          write(LOCALE_KEY, locale);
        });
        effect(() => write(TIME_ZONE_KEY, store.timeZone()));
      },
    };
  }),
);
