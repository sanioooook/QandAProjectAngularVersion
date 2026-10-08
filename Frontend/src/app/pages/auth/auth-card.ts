import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** The narrow centered card around the sign-in, sign-up and password pages. */
@Component({
  selector: 'app-auth-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="card">
      <header>
        <h1>{{ title() }}</h1>
        @if (subtitle()) {
          <p class="muted">{{ subtitle() }}</p>
        }
      </header>
      <ng-content />
    </div>
  `,
  styles: `
    :host {
      display: block;
      max-width: 440px;
      margin: clamp(0px, 4vh, 40px) auto 0;
    }
    .card {
      display: grid;
      gap: 18px;
      padding: clamp(20px, 4vw, 32px);
      border: 1px solid var(--app-border);
      border-radius: var(--app-radius);
      background: var(--mat-sys-surface-container-lowest);
      box-shadow: var(--app-shadow);
    }
    header {
      display: grid;
      gap: 6px;
    }
    h1 {
      font-size: 1.6rem;
    }
  `,
})
export class AuthCard {
  readonly title = input.required<string>();
  readonly subtitle = input<string>();
}
