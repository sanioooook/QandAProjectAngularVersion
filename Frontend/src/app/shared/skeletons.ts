import { ChangeDetectionStrategy, Component } from '@angular/core';

// Loading placeholders shaped like the content they stand for, so nothing jumps when it arrives.

@Component({
  selector: 'app-survey-card-skeleton',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true' },
  template: `
    <span class="skeleton" style="width: 72px; height: 22px; border-radius: 999px"></span>
    <span class="skeleton" style="width: 92%; height: 18px"></span>
    <span class="skeleton" style="width: 64%; height: 18px"></span>
    <span class="row"><span class="skeleton circle"></span><span class="skeleton" style="width: 55%; height: 13px"></span></span>
    <span class="skeleton" style="width: 70%; height: 13px; margin-top: auto"></span>
  `,
  styles: `
    :host {
      display: flex;
      flex-direction: column;
      gap: 12px;
      min-height: 172px;
      padding: 18px;
      border: 1px solid var(--app-border);
      border-radius: var(--app-radius);
      background: var(--mat-sys-surface-container-lowest);
    }
    .row {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .circle {
      width: 20px;
      height: 20px;
      border-radius: 50%;
    }
  `,
})
export class SurveyCardSkeleton {}

@Component({
  selector: 'app-survey-skeleton',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true' },
  template: `
    <span class="row">
      <span class="skeleton" style="width: 76px; height: 22px; border-radius: 999px"></span>
      <span class="skeleton" style="width: 160px; height: 14px"></span>
    </span>
    <span class="skeleton" style="width: 80%; height: 30px"></span>
    <span class="row"><span class="skeleton circle"></span><span class="skeleton" style="width: 240px; height: 14px"></span></span>
    <span class="skeleton" style="width: 100%; height: 56px; border-radius: 12px"></span>
    @for (i of [1, 2, 3]; track i) {
      <span class="skeleton" style="width: 100%; height: 52px; border-radius: 12px"></span>
    }
    <span class="skeleton" style="width: 150px; height: 40px; border-radius: 20px"></span>
  `,
  styles: `
    :host {
      display: grid;
      gap: 16px;
      padding: clamp(18px, 3vw, 28px);
      border: 1px solid var(--app-border);
      border-radius: var(--app-radius);
      background: var(--mat-sys-surface-container-lowest);
    }
    .row {
      display: flex;
      align-items: center;
      gap: 10px;
    }
    .circle {
      width: 22px;
      height: 22px;
      border-radius: 50%;
    }
  `,
})
export class SurveySkeleton {}

@Component({
  selector: 'app-form-skeleton',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true' },
  template: `
    @for (width of ['100%', '100%', '70%', '70%']; track $index) {
      <span class="field">
        <span class="skeleton" style="width: 120px; height: 13px"></span>
        <span class="skeleton" [style.width]="width" style="height: 52px; border-radius: 8px"></span>
      </span>
    }
    <span class="skeleton" style="width: 220px; height: 40px; border-radius: 20px; justify-self: end"></span>
  `,
  styles: `
    :host {
      display: grid;
      gap: 22px;
      padding: clamp(18px, 3vw, 28px);
      border: 1px solid var(--app-border);
      border-radius: var(--app-radius);
      background: var(--mat-sys-surface-container-lowest);
    }
    .field {
      display: grid;
      gap: 8px;
    }
  `,
})
export class FormSkeleton {}

/** First visit to a page that waits for the session: the page outline until the server answers. */
@Component({
  selector: 'app-page-skeleton',
  imports: [SurveyCardSkeleton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-busy': 'true' },
  template: `
    <span class="skeleton" style="width: 240px; height: 28px"></span>
    <span class="skeleton" style="width: 360px; max-width: 80%; height: 14px; margin-top: 12px"></span>
    <div class="cards">
      @for (i of [1, 2, 3]; track i) {
        <app-survey-card-skeleton />
      }
    </div>
  `,
  styles: `
    :host {
      display: block;
    }
    .cards {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(min(100%, 300px), 1fr));
      gap: 16px;
      margin-top: 24px;
    }
  `,
})
export class PageSkeleton {}
