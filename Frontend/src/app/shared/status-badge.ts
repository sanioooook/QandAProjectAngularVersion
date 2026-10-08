import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import type { SurveyStatus } from '../core/api/types';

@Component({
  selector: 'app-status-badge',
  imports: [TranslocoPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'status()' },
  template: `{{ 'survey.status.' + status() | transloco }}`,
  styles: `
    :host {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 2px 10px 2px 8px;
      border-radius: 999px;
      font-size: 0.78rem;
      font-weight: 600;
      white-space: nowrap;
    }
    :host::before {
      content: '';
      width: 7px;
      height: 7px;
      border-radius: 50%;
      background: currentColor;
    }
    :host(.active) {
      background: var(--app-success-soft);
      color: var(--app-success);
    }
    :host(.draft) {
      background: var(--app-warning-soft);
      color: var(--app-warning);
    }
    :host(.closed) {
      background: var(--mat-sys-surface-container-high);
      color: var(--app-muted);
    }
  `,
})
export class StatusBadge {
  readonly status = input.required<SurveyStatus>();
}
