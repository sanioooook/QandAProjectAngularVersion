import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-empty-state',
  imports: [MatIcon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-icon svgIcon="inbox" />
    <p>{{ text() }}</p>
    <ng-content />
  `,
  styles: `
    :host {
      display: grid;
      justify-items: center;
      gap: 14px;
      padding: 48px 20px;
      border: 1px dashed var(--app-border);
      border-radius: var(--app-radius);
      text-align: center;
      color: var(--app-muted);
    }
    mat-icon {
      width: 40px;
      height: 40px;
      opacity: 0.7;
    }
  `,
})
export class EmptyState {
  readonly text = input.required<string>();
}
