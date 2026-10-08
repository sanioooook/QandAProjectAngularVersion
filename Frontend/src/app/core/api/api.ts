import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import type {
  Account,
  AuthConfig,
  Credentials,
  Paged,
  Registration,
  SurveyDetails,
  SurveyInput,
  SurveyScope,
  SurveyStatus,
  SurveySummary,
} from './types';

export interface ListQuery {
  scope: SurveyScope;
  status?: SurveyStatus | null;
  page?: number;
  pageSize?: number;
}

export interface ProfileChanges {
  displayName?: string;
  locale?: string;
}

/** The backend's endpoints, one method per call. Stores await them, so they return promises. */
@Injectable({ providedIn: 'root' })
export class Api {
  private readonly http = inject(HttpClient);

  private call<T>(request: Observable<T>): Promise<T> {
    return firstValueFrom(request);
  }

  readonly auth = {
    config: () => this.call(this.http.get<AuthConfig>('/api/auth/config')),
    me: () => this.call(this.http.get<Account>('/api/auth/me')),
    login: (credentials: Credentials) => this.call(this.http.post<Account>('/api/auth/login', credentials)),
    register: (registration: Registration) => this.call(this.http.post<Account>('/api/auth/register', registration)),
    logout: () => this.call(this.http.post<void>('/api/auth/logout', {})),
    confirmEmail: (token: string) => this.call(this.http.post<void>('/api/auth/confirm-email', { token })),
    resendConfirmation: () => this.call(this.http.post<void>('/api/auth/resend-confirmation', {})),
    forgotPassword: (email: string, locale: string) =>
      this.call(this.http.post<void>('/api/auth/forgot-password', { email, locale })),
    resetPassword: (token: string, password: string) =>
      this.call(this.http.post<Account>('/api/auth/reset-password', { token, password })),
    changePassword: (currentPassword: string, newPassword: string) =>
      this.call(this.http.post<Account>('/api/auth/change-password', { currentPassword, newPassword })),
  };

  readonly account = {
    updateProfile: (changes: ProfileChanges) => this.call(this.http.put<Account>('/api/account/profile', changes)),
    uploadAvatar: (image: Blob) => {
      const form = new FormData();
      form.append('file', image, 'avatar');
      return this.call(this.http.put<Account>('/api/account/avatar', form));
    },
    removeAvatar: () => this.call(this.http.delete<Account>('/api/account/avatar')),
  };

  readonly surveys = {
    list: ({ scope, status, page = 1, pageSize = 20 }: ListQuery) => {
      let params = new HttpParams().set('scope', scope).set('page', page).set('pageSize', pageSize);
      if (status) params = params.set('status', status);
      return this.call(this.http.get<Paged<SurveySummary>>('/api/surveys', { params }));
    },
    get: (id: string) => this.call(this.http.get<SurveyDetails>(`/api/surveys/${id}`)),
    create: (input: SurveyInput) => this.call(this.http.post<SurveyDetails>('/api/surveys', input)),
    update: (id: string, input: SurveyInput) => this.call(this.http.put<SurveyDetails>(`/api/surveys/${id}`, input)),
    publish: (id: string) => this.call(this.http.post<SurveyDetails>(`/api/surveys/${id}/publish`, {})),
    remove: (id: string) => this.call(this.http.delete<void>(`/api/surveys/${id}`)),
    vote: (id: string, optionIds: number[]) =>
      this.call(this.http.put<SurveyDetails>(`/api/surveys/${id}/votes`, { optionIds })),
    addOption: (id: string, text: string, vote: boolean) =>
      this.call(this.http.post<SurveyDetails>(`/api/surveys/${id}/options`, { text, vote })),
  };
}
