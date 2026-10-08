import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/** "185/200" next to a text field, shown only near the limit (maxlength silently cuts pasted text). */
@Component({
  selector: 'app-char-count',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (visible()) {
      <span [class.full]="length() >= max()" aria-live="polite">{{ length() }}/{{ max() }}</span>
    }
  `,
  styles: `
    span {
      font-size: 0.8rem;
      font-variant-numeric: tabular-nums;
      color: var(--app-muted);
    }
    .full {
      color: var(--app-warning);
      font-weight: 600;
    }
  `,
})
export class CharCount {
  readonly value = input.required<string>();
  readonly max = input.required<number>();
  /** Share of the limit from which the counter appears. */
  readonly from = input(0.8);

  protected readonly length = computed(() => this.value().length);
  protected readonly visible = computed(() => this.length() >= this.max() * this.from());
}
