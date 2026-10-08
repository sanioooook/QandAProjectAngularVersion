import { Routes } from '@angular/router';
import { RouteAccess, authGuard } from './core/auth.guard';
// Where almost every visit lands (lists, shared links): bundled with the app to save a round trip.
// The other pages load on demand.
import { SurveyListPage } from './pages/survey-list/survey-list.page';
import { SurveyPage } from './pages/survey/survey.page';

const requiresAuth: RouteAccess = { requiresAuth: true };
const guestOnly: RouteAccess = { guestOnly: true };

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'surveys' },
  {
    path: '',
    canActivateChild: [authGuard],
    children: [
      { path: 'login', loadComponent: () => import('./pages/auth/auth.page').then((m) => m.AuthPage), data: { ...guestOnly, mode: 'login' } },
      { path: 'register', loadComponent: () => import('./pages/auth/auth.page').then((m) => m.AuthPage), data: { ...guestOnly, mode: 'register' } },
      { path: 'forgot-password', loadComponent: () => import('./pages/auth/forgot-password.page').then((m) => m.ForgotPasswordPage) },
      { path: 'reset-password', loadComponent: () => import('./pages/auth/reset-password.page').then((m) => m.ResetPasswordPage) },
      { path: 'confirm-email', loadComponent: () => import('./pages/auth/confirm-email.page').then((m) => m.ConfirmEmailPage) },
      // Published surveys and results are public; voting asks for an account.
      { path: 'surveys', component: SurveyListPage, data: { scope: 'active' } },
      { path: 'my', component: SurveyListPage, data: { ...requiresAuth, scope: 'mine' } },
      { path: 'voted', component: SurveyListPage, data: { ...requiresAuth, scope: 'voted' } },
      { path: 'surveys/new', loadComponent: () => import('./pages/survey-edit/survey-edit.page').then((m) => m.SurveyEditPage), data: requiresAuth },
      { path: 'surveys/:id/edit', loadComponent: () => import('./pages/survey-edit/survey-edit.page').then((m) => m.SurveyEditPage), data: requiresAuth },
      { path: 'surveys/:id', component: SurveyPage },
      { path: 'account', loadComponent: () => import('./pages/account/account.page').then((m) => m.AccountPage), data: requiresAuth },
      { path: 'account/password', loadComponent: () => import('./pages/account/change-password.page').then((m) => m.ChangePasswordPage), data: requiresAuth },
      { path: '**', loadComponent: () => import('./pages/not-found/not-found.page').then((m) => m.NotFoundPage) },
    ],
  },
];
