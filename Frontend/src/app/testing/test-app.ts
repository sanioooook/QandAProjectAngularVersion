import { HttpBackend } from '@angular/common/http';
import { Provider } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { render } from '@testing-library/angular';
import { App } from '../app';
import { appConfig } from '../app.config';

/** The app's own providers, with the network replaced by `backend`. */
export function appProviders(backend: HttpBackend, extra: Provider[] = []) {
  return [...appConfig.providers, { provide: HttpBackend, useValue: backend }, ...extra];
}

/**
 * The whole app (router, stores, i18n, every page) rendered in jsdom against `backend`, opened at `path`.
 * Elements are then found the way a user sees them (roles, labels, texts), not by CSS classes, so
 * layout changes do not break the tests.
 */
export async function startApp(path: string, backend: HttpBackend) {
  const result = await render(App, { providers: appProviders(backend) });
  const router = TestBed.inject(Router);
  await router.navigateByUrl(path);
  await result.fixture.whenStable();
  return { ...result, router };
}
