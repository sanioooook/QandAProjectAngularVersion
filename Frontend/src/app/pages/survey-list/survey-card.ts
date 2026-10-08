import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import type { SurveySummary } from '../../core/api/types';
import { DateFormat } from '../../core/date-format';
import { injectNow } from '../../core/now';
import { effectiveStatus } from '../../core/utils/deadline';
import { StatusBadge } from '../../shared/status-badge';
import { UserAvatar } from '../../shared/user-avatar';

@Component({
  selector: 'app-survey-card',
  imports: [TranslocoDirective, RouterLink, MatIcon, StatusBadge, UserAvatar],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @let s = survey();
    <a class="card" [routerLink]="['/surveys', s.id]" *transloco="let t">
      <div class="top">
        <app-status-badge [status]="status()" />
        @if (s.hasVoted) {
          <span class="voted"><mat-icon svgIcon="check" class="sm" />{{ t('survey.youVoted') }}</span>
        }
      </div>
      <h2 class="title" [title]="s.title">{{ s.title }}</h2>
      <p class="meta muted">
        <app-user-avatar [name]="s.author.name" [url]="s.author.avatarUrl" [size]="20" />
        <span [title]="format.title(s.publishedAt ?? s.createdAt)">
          {{ t('survey.by', { author: s.author.name }) }} ·
          {{ s.publishedAt ? t('survey.published', { date: format.date(s.publishedAt) }) : t('survey.created', { date: format.date(s.createdAt) }) }}
        </span>
      </p>
      <div class="stats muted">
        <span><mat-icon svgIcon="users" class="sm" />{{ t('survey.voters', { n: s.voterCount }) }}</span>
        <span>{{ t('survey.options', { n: s.optionCount }) }}</span>
        @if (s.deadline) {
          <span [title]="format.title(s.deadline)">
            <mat-icon svgIcon="clock" class="sm" />
            {{ status() === 'closed' ? t('survey.closedAt', { date: format.dateWithZone(s.deadline) }) : t('survey.closesIn', { relative: format.relative(s.deadline, now()) }) }}
          </span>
        }
      </div>
    </a>
  `,
  styles: `
    .card {
      display: flex;
      flex-direction: column;
      gap: 10px;
      height: 100%;
      padding: 18px;
      border: 1px solid var(--app-border);
      border-radius: var(--app-radius);
      background: var(--mat-sys-surface-container-lowest);
      box-shadow: var(--app-shadow);
      color: inherit;
      text-decoration: none;
      transition: border-color 0.15s, transform 0.15s;
    }
    .card:hover {
      border-color: var(--mat-sys-primary);
    }
    .card:focus-visible {
      outline: 2px solid var(--mat-sys-primary);
      outline-offset: 2px;
    }
    @media (prefers-reduced-motion: no-preference) {
      .card:hover {
        transform: translateY(-2px);
      }
    }
    .top {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 8px;
    }
    .voted {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--app-success);
    }
    /* Long questions are cut after three lines; the full text is in the tooltip. */
    .title {
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
      overflow-wrap: anywhere;
      font-size: 1.08rem;
    }
    .meta {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 0.86rem;
    }
    .stats {
      display: flex;
      flex-wrap: wrap;
      gap: 6px 14px;
      margin-top: auto;
      padding-top: 6px;
      font-size: 0.84rem;
    }
    .stats span {
      display: inline-flex;
      align-items: center;
      gap: 5px;
    }
  `,
})
export class SurveyCard {
  readonly survey = input.required<SurveySummary>();
  protected readonly format = inject(DateFormat);
  // Refresh "closes in 5 minutes" and flip to closed when the deadline passes while the list is open.
  protected readonly now = injectNow(30_000);
  protected readonly status = computed(() => effectiveStatus(this.survey().status, this.survey().deadline, this.now()));
}
