import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldDefaultOptions } from '@angular/material/form-field';
import { provideRouter, withComponentInputBinding, withInMemoryScrolling } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { routes } from './app.routes';
import { apiInterceptor } from './core/api/api.interceptor';
import { provideI18n } from './core/i18n/i18n';
import { PrefsStore } from './core/stores/prefs.store';
import { provideIcons } from './shared/icons';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding(), withInMemoryScrolling({ scrollPositionRestoration: 'enabled' })),
    provideHttpClient(withFetch(), withInterceptors([apiInterceptor])),
    provideNativeDateAdapter(),
    // Outlined fields: the filled ones read as grey slabs, especially in the dark theme.
    { provide: MAT_FORM_FIELD_DEFAULT_OPTIONS, useValue: { appearance: 'outline' } satisfies MatFormFieldDefaultOptions },
    provideI18n(),
    provideIcons(),
    // Applies the saved theme and language, and waits for that language's texts (a few KB), so the
    // first screen never shows translation keys. Until then index.html shows its boot skeleton.
    provideAppInitializer(() => {
      const prefs = inject(PrefsStore);
      return firstValueFrom(inject(TranslocoService).load(prefs.locale()));
    }),
  ],
};
