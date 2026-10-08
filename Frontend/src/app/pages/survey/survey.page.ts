import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatAnchor, MatButton } from '@angular/material/button';
import { MatCheckbox } from '@angular/material/checkbox';
import { MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatRadioButton } from '@angular/material/radio';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { ApiError } from '../../core/api/api-error';
import type { SurveyDetails, SurveyOption, SurveyStatus } from '../../core/api/types';
import { DateFormat } from '../../core/date-format';
import { ErrorTexts } from '../../core/i18n/error-texts';
import { LIMITS } from '../../core/limits';
import { injectNow } from '../../core/now';
import { AuthStore } from '../../core/stores/auth.store';
import { SurveysStore } from '../../core/stores/surveys.store';
import { Toasts } from '../../core/toasts';
import { clock, effectiveStatus, remaining } from '../../core/utils/deadline';
import { Confirm } from '../../shared/confirm-dialog';
import { ShareButton } from '../../shared/share-dialog';
import { SurveySkeleton } from '../../shared/skeletons';
import { StatusBadge } from '../../shared/status-badge';
import { UserAvatar } from '../../shared/user-avatar';
import { NotFoundPage } from '../not-found/not-found.page';

/** Questions longer than this get a smaller heading. */
const LONG_TITLE = 90;

@Component({
  selector: 'app-survey-page',
  imports: [
    TranslocoDirective,
    RouterLink,
    FormsModule,
    MatButton,
    MatAnchor,
    MatIcon,
    MatCheckbox,
    MatRadioButton,
    MatFormField,
    MatLabel,
    MatSuffix,
    MatInput,
    ShareButton,
    StatusBadge,
    UserAvatar,
    SurveySkeleton,
    NotFoundPage,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './survey.page.html',
  styleUrl: './survey.page.scss',
})
export class SurveyPage {
  /** From the route. */
  readonly id = input.required<string>();

  private readonly store = inject(SurveysStore);
  protected readonly auth = inject(AuthStore);
  protected readonly format = inject(DateFormat);
  private readonly errors = inject(ErrorTexts);
  private readonly toasts = inject(Toasts);
  private readonly transloco = inject(TranslocoService);
  private readonly confirm = inject(Confirm);
  private readonly router = inject(Router);

  protected readonly limits = LIMITS;
  protected readonly longTitle = LONG_TITLE;

  protected readonly survey = computed(() => this.store.survey(this.id()));
  protected readonly notFound = signal(false);
  protected readonly loadError = signal('');
  protected readonly busy = signal(false);

  // --- long texts -----------------------------------------------------------
  // Up to 200-character questions and 1000-character descriptions must not push the vote off screen.
  protected readonly descriptionOpen = signal(false);
  protected readonly descriptionLong = computed(() => {
    const text = this.survey()?.description ?? '';
    return text.length > 280 || text.split(/\r?\n/).length > 4;
  });

  // --- deadline -------------------------------------------------------------
  private readonly now = injectNow(1000);
  protected readonly status = computed(() => {
    const survey = this.survey();
    return survey ? effectiveStatus(survey.status, survey.deadline, this.now()) : 'draft';
  });
  protected readonly canVote = computed(() => !!this.survey()?.canVote && this.status() === 'active');
  protected readonly canAddOption = computed(() => !!this.survey()?.canAddOption && this.status() === 'active');
  protected readonly left = computed(() => {
    const deadline = this.survey()?.deadline;
    return deadline && this.status() === 'active' ? remaining(deadline, this.now()) : null;
  });
  protected readonly countdown = computed(() => {
    const left = this.left();
    if (!left) return '';
    const days = left.days > 0 ? this.transloco.translate('survey.days', { n: left.days }) : '';
    return [days, clock(left)].filter(Boolean).join(' ');
  });

  // --- voting ---------------------------------------------------------------
  protected readonly selection = signal<number[]>([]);
  protected readonly single = computed(() => this.survey()?.maxVotesPerUser === 1);
  protected readonly atLimit = computed(() => {
    const survey = this.survey();
    return !!survey && this.selection().length >= survey.maxVotesPerUser;
  });
  protected readonly hasVoted = computed(() => (this.survey()?.myVotes.length ?? 0) > 0);
  protected readonly dirty = computed(() => {
    const mine = [...(this.survey()?.myVotes ?? [])].sort();
    const current = [...this.selection()].sort();
    return mine.length !== current.length || mine.some((id, i) => id !== current[i]);
  });

  // --- new option -----------------------------------------------------------
  protected readonly newOption = signal('');
  protected readonly newOptionVote = signal(false);
  protected readonly newOptionError = signal('');
  protected readonly optionsLeft = computed(() => {
    const survey = this.survey();
    return survey ? survey.maxOptionsPerParticipant - survey.myAddedOptions : 0;
  });
  protected readonly canVoteForNew = computed(() => {
    const survey = this.survey();
    return !!survey && survey.myVotes.length < survey.maxVotesPerUser;
  });

  // The share link uses the address the app was opened from, so it works on any host without configuration.
  protected readonly shareUrl = computed(() => `${window.location.origin}/surveys/${this.id()}`);

  constructor() {
    effect(() => {
      const id = this.id();
      untracked(() => void this.load(id));
    });

    // The server's choice is the starting selection (again after every vote).
    effect(() => {
      const votes = this.survey()?.myVotes ?? [];
      untracked(() => this.selection.set([...votes]));
    });

    // The deadline passed while the page is open: voting is locked at once, then the server's view is loaded.
    let previous: SurveyStatus | null = null;
    effect(() => {
      const next = this.status();
      if (previous === 'active' && next === 'closed') {
        untracked(() => {
          this.store.invalidateLists(['active']);
          void this.store.fetchSurvey(this.id(), { force: true }).catch(() => {});
        });
      }
      previous = next;
    });
  }

  private async load(id: string): Promise<void> {
    this.notFound.set(false);
    this.loadError.set('');
    try {
      await this.store.fetchSurvey(id);
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) this.notFound.set(true);
      else this.loadError.set(this.errors.message(error));
    }
  }

  protected isSelected(option: SurveyOption): boolean {
    return this.selection().includes(option.id);
  }

  protected optionDisabled(option: SurveyOption): boolean {
    return !this.canVote() || this.busy() || (!this.isSelected(option) && this.atLimit() && !this.single());
  }

  protected toggle(option: SurveyOption): void {
    if (!this.canVote()) return;
    if (this.single()) {
      this.selection.set([option.id]);
      return;
    }
    this.selection.update((current) =>
      current.includes(option.id) ? current.filter((id) => id !== option.id) : this.atLimit() ? current : [...current, option.id],
    );
  }

  protected percent(survey: SurveyDetails, option: SurveyOption): number {
    return survey.totalVotes > 0 ? Math.round((option.votes / survey.totalVotes) * 100) : 0;
  }

  private async run<T>(action: () => Promise<T>, successKey?: string): Promise<T | undefined> {
    this.busy.set(true);
    try {
      const result = await action();
      if (successKey) this.toasts.success(this.transloco.translate(successKey));
      return result;
    } catch (error) {
      this.toasts.error(this.errors.message(error));
      // The survey may have changed under us (closed, option removed): show the current state.
      if (error instanceof ApiError && error.status === 409) {
        await this.store.fetchSurvey(this.id(), { force: true }).catch(() => {});
      }
      return undefined;
    } finally {
      this.busy.set(false);
    }
  }

  protected submitVote(): void {
    const selection = this.selection();
    void this.run(() => this.store.vote(this.id(), selection), selection.length ? 'toast.voted' : 'toast.voteWithdrawn');
  }

  protected withdraw(): void {
    void this.run(() => this.store.vote(this.id(), []), 'toast.voteWithdrawn');
  }

  protected async addOption(): Promise<void> {
    this.newOptionError.set('');
    const text = this.newOption().trim();
    if (!text) return;
    this.busy.set(true);
    try {
      await this.store.addOption(this.id(), text, this.newOptionVote() && this.canVoteForNew());
      this.newOption.set('');
      this.newOptionVote.set(false);
      this.toasts.success(this.transloco.translate('toast.optionAdded'));
    } catch (error) {
      this.newOptionError.set(this.errors.fields(error)['text'] ?? this.errors.message(error));
    } finally {
      this.busy.set(false);
    }
  }

  protected publish(): void {
    void this.run(() => this.store.publish(this.id()), 'toast.published');
  }

  protected async remove(): Promise<void> {
    const confirmed = await this.confirm.ask({
      message: this.transloco.translate('survey.confirmDelete'),
      confirm: this.transloco.translate('survey.delete'),
      cancel: this.transloco.translate('form.cancel'),
    });
    if (!confirmed) return;
    const done = await this.run(async () => {
      await this.store.remove(this.id());
      return true;
    }, 'toast.deleted');
    if (done) await this.router.navigate(['/my'], { replaceUrl: true });
  }
}
