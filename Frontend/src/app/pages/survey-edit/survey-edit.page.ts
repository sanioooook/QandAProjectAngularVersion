import {
  ChangeDetectionStrategy,
  Component,
  Injector,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  runInInjectionContext,
  signal,
  untracked,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCheckbox } from '@angular/material/checkbox';
import { MatDatepicker, MatDatepickerInput, MatDatepickerToggle } from '@angular/material/datepicker';
import { MatError, MatFormField, MatHint, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatTimepicker, MatTimepickerInput, MatTimepickerToggle } from '@angular/material/timepicker';
import { MatTooltip } from '@angular/material/tooltip';
import { Location } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { map } from 'rxjs';
import { ApiError } from '../../core/api/api-error';
import type { SurveyDetails, SurveyInput } from '../../core/api/types';
import { DateFormat } from '../../core/date-format';
import { formDraft } from '../../core/form-draft';
import { errorMessage, showErrors } from '../../core/form-errors';
import { ErrorTexts } from '../../core/i18n/error-texts';
import { LIMITS } from '../../core/limits';
import { PrefsStore } from '../../core/stores/prefs.store';
import { SurveysStore } from '../../core/stores/surveys.store';
import { Toasts } from '../../core/toasts';
import { composeDeadline, splitDeadline, todayInput } from '../../core/utils/deadline';
import { zoneName } from '../../core/utils/time-zone';
import { CharCount } from '../../shared/char-count';
import { FormSkeleton } from '../../shared/skeletons';
import { NotFoundPage } from '../not-found/not-found.page';

/** What is kept as a draft: the form value with the deadline as plain strings. */
interface DraftValue {
  title: string;
  description: string;
  options: string[];
  deadlineDate: string;
  deadlineTime: string;
  maxVotesPerUser: number;
  allowParticipantOptions: boolean;
  maxOptionsPerParticipant: number;
}

const pad = (n: number) => String(n).padStart(2, '0');

// The pickers work with Date objects in the device's zone; only their calendar date and clock time
// are used, and those are read in the zone chosen in the settings (see composeDeadline).
function dateToInput(date: Date | null): string {
  return date ? `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` : '';
}

function timeToInput(time: Date | null): string {
  return time ? `${pad(time.getHours())}:${pad(time.getMinutes())}` : '';
}

function inputToDate(value: string): Date | null {
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  return m ? new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3])) : null;
}

function inputToTime(value: string): Date | null {
  const m = /^(\d{2}):(\d{2})$/.exec(value);
  return m ? new Date(2000, 0, 1, Number(m[1]), Number(m[2])) : null;
}

/** Without an id this is the "new survey" page, with an id it edits a draft. */
@Component({
  selector: 'app-survey-edit-page',
  imports: [
    TranslocoDirective,
    ReactiveFormsModule,
    RouterLink,
    MatFormField,
    MatLabel,
    MatInput,
    MatHint,
    MatError,
    MatSuffix,
    MatButton,
    MatIconButton,
    MatIcon,
    MatCheckbox,
    MatTooltip,
    MatDatepicker,
    MatDatepickerInput,
    MatDatepickerToggle,
    MatTimepicker,
    MatTimepickerInput,
    MatTimepickerToggle,
    CharCount,
    FormSkeleton,
    NotFoundPage,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './survey-edit.page.html',
  styleUrl: './survey-edit.page.scss',
})
export class SurveyEditPage {
  readonly id = input<string | undefined>();

  private readonly store = inject(SurveysStore);
  private readonly prefs = inject(PrefsStore);
  private readonly router = inject(Router);
  private readonly location = inject(Location);
  private readonly toasts = inject(Toasts);
  private readonly errors = inject(ErrorTexts);
  private readonly transloco = inject(TranslocoService);
  private readonly injector = inject(Injector);
  protected readonly format = inject(DateFormat);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly limits = LIMITS;

  protected readonly form = this.fb.group({
    title: '',
    description: '',
    options: this.fb.array([this.fb.control(''), this.fb.control('')]),
    deadlineDate: new FormControl<Date | null>(null),
    deadlineTime: new FormControl<Date | null>(null),
    maxVotesPerUser: 1,
    allowParticipantOptions: false,
    maxOptionsPerParticipant: 1,
  });
  private readonly initial = this.draftValue();

  /** The form value as a signal, for everything derived from it. */
  protected readonly value = toSignal(this.form.valueChanges.pipe(map(() => this.form.getRawValue())), {
    initialValue: this.form.getRawValue(),
  });

  protected readonly formError = signal('');
  protected readonly optionsError = signal('');
  protected readonly saving = signal(false);
  protected readonly notFound = signal(false);
  protected readonly loaded = signal(false);
  private serverSurvey: SurveyDetails | null = null;

  // Deadline dates and times are entered in the zone chosen in the settings.
  protected readonly zone = computed(() => this.prefs.effectiveTimeZone());
  protected readonly zoneLabel = computed(() => zoneName(this.zone()));
  protected readonly minDate = computed(() => inputToDate(todayInput(this.zone())));
  protected readonly deadline = computed(() => {
    const { deadlineDate, deadlineTime } = this.value();
    return composeDeadline(dateToInput(deadlineDate), timeToInput(deadlineTime), this.zone());
  });
  private readonly filledOptions = computed(() => this.value().options.map((o) => o.trim()).filter(Boolean));
  protected readonly maxVotesLimit = computed(() =>
    this.value().allowParticipantOptions ? LIMITS.maxVotesPerUserCap : Math.max(1, this.filledOptions().length),
  );

  // What was typed survives an expired session or a reload of this tab.
  private draft: ReturnType<typeof formDraft<DraftValue>> | null = null;

  constructor() {
    effect(() => {
      const id = this.id();
      untracked(() => void this.init(id));
    });
    // The time only refines a date: without a date there is nothing to refine.
    effect(() => {
      const time = this.form.controls.deadlineTime;
      if (this.value().deadlineDate) time.enable({ emitEvent: false });
      else time.disable({ emitEvent: false });
    });
  }

  protected get options() {
    return this.form.controls.options;
  }

  private draftValue(): DraftValue {
    const raw = this.form.getRawValue();
    return { ...raw, deadlineDate: dateToInput(raw.deadlineDate), deadlineTime: timeToInput(raw.deadlineTime) };
  }

  private applyDraft(value: DraftValue): void {
    this.setOptionCount(value.options.length);
    this.form.setValue({ ...value, deadlineDate: inputToDate(value.deadlineDate), deadlineTime: inputToTime(value.deadlineTime) });
  }

  private setOptionCount(count: number): void {
    while (this.options.length < count) this.options.push(this.fb.control(''));
    while (this.options.length > count) this.options.removeAt(this.options.length - 1);
  }

  private async init(id: string | undefined): Promise<void> {
    if (!id) {
      this.draft = this.newDraft('qanda.surveyForm.new');
      this.draft.start();
      this.loaded.set(true);
      return;
    }
    try {
      const survey = await this.store.fetchSurvey(id);
      if (!survey.isAuthor || survey.status !== 'draft') {
        // Published surveys are read-only: show them instead.
        await this.router.navigate(['/surveys', id], { replaceUrl: true });
        return;
      }
      this.serverSurvey = survey;
      this.fill(survey);
      this.draft = this.newDraft(`qanda.surveyForm.${id}`);
      this.draft.start();
      this.loaded.set(true);
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) this.notFound.set(true);
      else this.formError.set(this.errors.message(error));
    }
  }

  private newDraft(key: string) {
    // Created after the survey has loaded, i.e. outside the constructor's injection context.
    return runInInjectionContext(this.injector, () =>
      formDraft<DraftValue>({
        key,
        value: () => this.draftValue(),
        apply: (value) => this.applyDraft(value),
        changes: this.form.valueChanges,
      }),
    );
  }

  protected restored(): boolean {
    return this.draft?.restored() ?? false;
  }

  private fill(survey: SurveyDetails): void {
    const { date, time } = splitDeadline(survey.deadline, this.zone());
    this.applyDraft({
      title: survey.title,
      description: survey.description ?? '',
      options: survey.options.map((o) => o.text),
      deadlineDate: date,
      deadlineTime: time,
      maxVotesPerUser: survey.maxVotesPerUser,
      allowParticipantOptions: survey.allowParticipantOptions,
      maxOptionsPerParticipant: survey.maxOptionsPerParticipant,
    });
  }

  /** Throws away the restored input and goes back to the empty form (or the saved draft). */
  protected startOver(): void {
    this.draft?.clear();
    if (this.serverSurvey) this.fill(this.serverSurvey);
    else this.applyDraft(this.initial);
    this.formError.set('');
    this.optionsError.set('');
  }

  protected error(name: 'title' | 'description' | 'maxVotesPerUser' | 'maxOptionsPerParticipant' | 'deadlineDate'): string {
    return errorMessage(this.form.controls[name]);
  }

  protected addOption(): void {
    if (this.options.length >= LIMITS.optionsMax) return;
    this.options.push(this.fb.control(''));
    const index = this.options.length - 1;
    afterNextRender(() => document.getElementById(`option-${index}`)?.focus(), { injector: this.injector });
  }

  protected removeOption(index: number): void {
    this.options.removeAt(index);
  }

  protected onOptionEnter(event: Event, index: number): void {
    event.preventDefault();
    if (index === this.options.length - 1 && this.options.at(index).value.trim()) this.addOption();
    else document.getElementById(`option-${index + 1}`)?.focus();
  }

  protected clearDeadline(): void {
    this.form.patchValue({ deadlineDate: null, deadlineTime: null });
  }

  private validate(): Record<string, string> {
    const value = this.form.getRawValue();
    const result: Record<string, string> = {};
    if (!value.title.trim()) result['title'] = this.errors.code('title_required');
    const options = this.filledOptions();
    if (options.length < LIMITS.optionsMin) result['options'] = this.errors.code('options_min');
    else if (new Set(options.map((o) => o.toLowerCase())).size !== options.length) result['options'] = this.errors.code('option_duplicate');
    if (value.maxVotesPerUser < 1 || value.maxVotesPerUser > LIMITS.maxVotesPerUserCap) {
      result['maxVotesPerUser'] = this.errors.code('max_votes_range');
    } else if (!value.allowParticipantOptions && value.maxVotesPerUser > options.length) {
      result['maxVotesPerUser'] = this.errors.code('max_votes_exceeds_options');
    }
    if (
      value.allowParticipantOptions &&
      (value.maxOptionsPerParticipant < 1 || value.maxOptionsPerParticipant > LIMITS.maxOptionsPerParticipantCap)
    ) {
      result['maxOptionsPerParticipant'] = this.errors.code('max_options_range');
    }
    const deadline = this.deadline();
    if (deadline && deadline.getTime() <= Date.now()) result['deadline'] = this.errors.code('deadline_past');
    return result;
  }

  /** Shows errors on the fields; `options` and `deadline` are not single controls. */
  private showProblems(problems: Record<string, string>): void {
    const { options, deadline, ...fields } = problems;
    this.optionsError.set(options ?? '');
    if (options) {
      for (const control of this.options.controls) {
        control.setErrors({ group: true });
        control.markAsTouched();
      }
    }
    showErrors(this.form, { ...fields, ...(deadline ? { deadlineDate: deadline } : {}) });
  }

  /** After a failed save the problem may be off screen (the buttons are at the bottom): bring it into view. */
  private revealProblem(): void {
    afterNextRender(
      () => {
        const target =
          document.querySelector<HTMLElement>('form .mat-form-field-invalid input, form .mat-form-field-invalid textarea') ??
          document.getElementById('form-error');
        if (!target) return;
        const reduceMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;
        target.scrollIntoView({ block: 'center', behavior: reduceMotion ? 'auto' : 'smooth' });
        target.focus({ preventScroll: true });
      },
      { injector: this.injector },
    );
  }

  protected async save(publish: boolean): Promise<void> {
    this.formError.set('');
    this.optionsError.set('');
    const problems = this.validate();
    if (Object.keys(problems).length > 0) {
      this.showProblems(problems);
      this.formError.set(this.transloco.translate('errors.validation'));
      this.revealProblem();
      return;
    }

    const value = this.form.getRawValue();
    const input: SurveyInput = {
      title: value.title.trim(),
      description: value.description.trim() || null,
      options: this.filledOptions(),
      deadline: this.deadline()?.toISOString() ?? null,
      maxVotesPerUser: value.maxVotesPerUser,
      allowParticipantOptions: value.allowParticipantOptions,
      maxOptionsPerParticipant: value.maxOptionsPerParticipant,
      publish,
    };

    this.saving.set(true);
    const id = this.id();
    let survey: SurveyDetails;
    try {
      survey = id ? await this.store.update(id, input) : await this.store.create(input);
    } catch (error) {
      this.showProblems(this.errors.fields(error));
      this.formError.set(this.errors.message(error));
      this.revealProblem();
      return;
    } finally {
      this.saving.set(false);
    }
    this.draft?.clear();
    this.toasts.success(this.transloco.translate(publish ? 'toast.published' : id ? 'toast.saved' : 'toast.created'));
    await this.router.navigate(['/surveys', survey.id]);
  }

  protected cancel(): void {
    this.draft?.clear();
    this.location.back();
  }
}
