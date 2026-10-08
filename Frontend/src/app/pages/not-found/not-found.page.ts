import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatAnchor } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-not-found-page',
  imports: [TranslocoDirective, RouterLink, MatAnchor],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section *transloco="let t">
      <p class="code" aria-hidden="true">404</p>
      <h1>{{ t('notFound.title') }}</h1>
      <p class="muted">{{ t('notFound.text') }}</p>
      <a mat-flat-button routerLink="/surveys">{{ t('notFound.home') }}</a>
    </section>
  `,
  styles: `
    section {
      display: grid;
      justify-items: center;
      gap: 12px;
      padding: 56px 16px;
      text-align: center;
    }
    .code {
      font-size: 4rem;
      font-weight: 800;
      line-height: 1;
      color: var(--mat-sys-primary);
      opacity: 0.35;
    }
    a {
      margin-top: 8px;
    }
  `,
})
export class NotFoundPage {}
