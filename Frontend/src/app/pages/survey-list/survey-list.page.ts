import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { MatAnchor, MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { ListQuery } from '../../core/api/api';
import type { SurveyScope, SurveyStatus } from '../../core/api/types';
import { ErrorTexts } from '../../core/i18n/error-texts';
import { SurveysStore, listKey } from '../../core/stores/surveys.store';
import { EmptyState } from '../../shared/empty-state';
import { SurveyCardSkeleton } from '../../shared/skeletons';
import { SurveyCard } from './survey-card';

const PAGE_SIZE = 12;
const STATUSES: SurveyStatus[] = ['draft', 'active', 'closed'];

/** Active surveys (public), my surveys and the ones I voted in. Filter and page live in the URL. */
@Component({
  selector: 'app-survey-list-page',
  imports: [TranslocoDirective, RouterLink, MatButton, MatAnchor, MatIcon, EmptyState, SurveyCard, SurveyCardSkeleton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './survey-list.page.html',
  styleUrl: './survey-list.page.scss',
})
export class SurveyListPage {
  /** From the route data. */
  readonly scope = input.required<SurveyScope>();
  /** From the query string. */
  readonly statusParam = input<string | undefined>(undefined, { alias: 'status' });
  readonly pageParam = input<string | undefined>(undefined, { alias: 'page' });

  private readonly store = inject(SurveysStore);
  private readonly errors = inject(ErrorTexts);

  protected readonly statuses = STATUSES;
  protected readonly status = computed<SurveyStatus | null>(() => {
    const value = this.statusParam() as SurveyStatus;
    return this.scope() === 'mine' && STATUSES.includes(value) ? value : null;
  });
  protected readonly page = computed(() => Math.max(1, Number(this.pageParam()) || 1));
  private readonly query = computed<ListQuery>(() => ({ scope: this.scope(), status: this.status(), page: this.page(), pageSize: PAGE_SIZE }));

  // Cached data is shown at once; fetchList only hits the network when the cache is stale.
  protected readonly data = computed(() => this.store.list(this.query()));
  protected readonly pages = computed(() => Math.max(1, Math.ceil((this.data()?.total ?? 0) / PAGE_SIZE)));
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly emptyKey = computed(() => {
    if (this.status()) return 'list.emptyFiltered';
    const scope = this.scope();
    return `list.empty${scope.charAt(0).toUpperCase()}${scope.slice(1)}`;
  });

  constructor() {
    effect(() => {
      const query = this.query();
      untracked(() => void this.load(query));
    });
    // A list that went stale (after a vote or a new survey elsewhere) is refetched when shown.
    effect(() => {
      const query = this.query();
      const entry = this.store.lists()[listKey(query)];
      if (entry?.fetchedAt === 0) untracked(() => void this.load(query));
    });
  }

  protected async load(query = this.query(), force = false): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      await this.store.fetchList(query, { force });
    } catch (e) {
      this.error.set(this.errors.message(e));
    } finally {
      this.loading.set(false);
    }
  }

  protected pageParams(value: number) {
    return { status: this.status() ?? undefined, page: value > 1 ? value : undefined };
  }
}
