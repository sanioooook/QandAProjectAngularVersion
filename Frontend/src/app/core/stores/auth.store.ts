import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withProps, withState } from '@ngrx/signals';
import { Api, ProfileChanges } from '../api/api';
import type { Account, AuthConfig, Credentials, Registration } from '../api/types';
import { SurveysStore } from './surveys.store';

interface AuthState {
  user: Account | null;
  /** What the server supports; without email there is no confirmation and no password reset. */
  config: AuthConfig;
  /** The session is known (signed in or guest). */
  initialized: boolean;
}

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState<AuthState>({
    user: null,
    config: { emailEnabled: false, confirmationRequired: false },
    initialized: false,
  }),
  withComputed(({ user, config }) => ({
    isLoggedIn: computed(() => user() !== null),
    /** Signed in, but voting and creating stay locked until the email is confirmed. */
    needsConfirmation: computed(() => {
      const me = user();
      return !!me && config().confirmationRequired && !me.emailConfirmed;
    }),
  })),
  withProps(() => ({
    _api: inject(Api),
    _surveys: inject(SurveysStore),
    _init: null as Promise<void> | null,
  })),
  withMethods((store) => {
    const loadConfig = () =>
      store._api.auth.config().then(
        (config) => patchState(store, { config }),
        () => {}, // email features just stay off
      );

    function setUser(user: Account | null): void {
      if (user?.id !== store.user()?.id) {
        // Cached surveys contain per-user data (my votes, author view).
        store._surveys.reset();
      }
      patchState(store, { user, initialized: true });
      store._init ??= loadConfig();
    }

    /** Cached surveys show the old name or avatar, so they are dropped after a profile change. */
    function setProfile(user: Account): Account {
      store._surveys.reset();
      setUser(user);
      return user;
    }

    return {
      /** Resolves the session and the server config once; later calls reuse the same requests. */
      init(): Promise<void> {
        store._init ??= Promise.all([
          store._api.auth.me().then(
            (user) => patchState(store, { user }),
            () => patchState(store, { user: null }),
          ),
          loadConfig(),
        ]).then(() => patchState(store, { initialized: true }));
        return store._init;
      },

      setUser,

      async login(credentials: Credentials): Promise<Account> {
        const user = await store._api.auth.login(credentials);
        setUser(user);
        return user;
      },

      async register(registration: Registration): Promise<Account> {
        const user = await store._api.auth.register(registration);
        setUser(user);
        return user;
      },

      async logout(): Promise<void> {
        try {
          await store._api.auth.logout();
        } finally {
          setUser(null);
        }
      },

      async confirmEmail(token: string): Promise<void> {
        await store._api.auth.confirmEmail(token);
        // The link may be opened in a browser where this user is signed in: unlock right away.
        if (store.user()) setUser(await store._api.auth.me());
      },

      async resetPassword(token: string, password: string): Promise<Account> {
        const user = await store._api.auth.resetPassword(token, password);
        setUser(user);
        return user;
      },

      async changePassword(currentPassword: string, newPassword: string): Promise<Account> {
        const user = await store._api.auth.changePassword(currentPassword, newPassword);
        setUser(user);
        return user;
      },

      async updateProfile(changes: ProfileChanges): Promise<Account> {
        const user = await store._api.account.updateProfile(changes);
        if (changes.displayName === undefined) {
          patchState(store, { user });
          return user;
        }
        return setProfile(user);
      },

      uploadAvatar: async (image: Blob) => setProfile(await store._api.account.uploadAvatar(image)),
      removeAvatar: async () => setProfile(await store._api.account.removeAvatar()),
      resendConfirmation: () => store._api.auth.resendConfirmation(),
      forgotPassword: (email: string, locale: string) => store._api.auth.forgotPassword(email, locale),

      /** The server rejected our cookie (expired, password changed elsewhere, user gone). */
      sessionExpired(): void {
        setUser(null);
      },
    };
  }),
);
