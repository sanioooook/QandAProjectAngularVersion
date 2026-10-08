import { EnvironmentProviders, Injectable, Provider, isDevMode } from '@angular/core';
import { Translation as TranslocoTranslation, TranslocoLoader, provideTransloco } from '@jsverse/transloco';
import { provideTranslocoMessageformat } from '@jsverse/transloco-messageformat';
import type { Translation } from './en';

export const LOCALES = ['uk', 'en', 'ru'] as const;
export type Locale = (typeof LOCALES)[number];

export const LOCALE_NAMES: Record<Locale, string> = { uk: 'Українська', en: 'English', ru: 'Русский' };

export function isLocale(value: unknown): value is Locale {
  return typeof value === 'string' && (LOCALES as readonly string[]).includes(value);
}

/**
 * First supported language from the browser preferences. A browser in some other language gets
 * English; Ukrainian only when the browser reports no languages at all.
 */
export function detectLocale(languages: readonly string[] = navigator.languages ?? []): Locale {
  for (const language of languages) {
    const base = language.toLowerCase().split('-')[0];
    if (isLocale(base)) return base;
  }
  return languages.length > 0 ? 'en' : 'uk';
}

// Each language is its own chunk: only the one in use is downloaded.
const MESSAGES: Record<Locale, () => Promise<Translation>> = {
  en: () => import('./en').then((m) => m.en),
  uk: () => import('./uk').then((m) => m.uk),
  ru: () => import('./ru').then((m) => m.ru),
};

@Injectable({ providedIn: 'root' })
export class MessagesLoader implements TranslocoLoader {
  getTranslation(lang: string): Promise<TranslocoTranslation> {
    return MESSAGES[isLocale(lang) ? lang : 'en']();
  }
}

export function provideI18n(): (Provider | EnvironmentProviders)[] {
  return [
    ...provideTransloco({
      config: {
        availableLangs: [...LOCALES],
        defaultLang: 'en',
        fallbackLang: 'en',
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
        missingHandler: { useFallbackTranslation: true, logMissingKey: isDevMode() },
      },
      loader: MessagesLoader,
    }),
    // ICU MessageFormat: plural forms per language ("1 голос / 3 голоси / 5 голосів").
    provideTranslocoMessageformat({ locales: [...LOCALES] }),
  ];
}
