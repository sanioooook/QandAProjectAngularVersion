import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './stores/auth.store';

export interface RouteAccess {
  /** Only for signed-in users; guests are sent to the login page and brought back afterwards. */
  requiresAuth?: boolean;
  /** Login / registration: signed-in users are sent home. */
  guestOnly?: boolean;
}

/** Only same-app paths are accepted as a post-login redirect target (no open redirects). */
export function safeRedirect(value: unknown): string | null {
  return typeof value === 'string' && value.startsWith('/') && !value.startsWith('//') ? value : null;
}

export const authGuard: CanActivateFn = async (route, state) => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  const access = route.data as RouteAccess;
  const session = auth.init();
  // Public pages do not depend on who is signed in to load (the cookie goes with every request),
  // so they open at once and show their skeletons instead of a blank page on a slow first load.
  if (!access.requiresAuth && !access.guestOnly) return true;
  await session;
  if (access.requiresAuth && !auth.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { redirect: state.url } });
  }
  if (access.guestOnly && auth.isLoggedIn()) {
    return router.parseUrl(safeRedirect(route.queryParamMap.get('redirect')) ?? '/surveys');
  }
  return true;
};
