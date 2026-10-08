import { Injectable, inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { ApiError } from '../api/api-error';
import { LIMITS } from '../limits';

/** Values interpolated into error messages that mention limits. */
export const ERROR_PARAMS: Record<string, Record<string, number>> = {
  name_length: { min: LIMITS.displayNameMin, max: LIMITS.displayNameMax },
  password_length: { min: LIMITS.passwordMin, max: LIMITS.passwordMax },
  title_length: { max: LIMITS.titleMax },
  description_length: { max: LIMITS.descriptionMax },
  option_length: { max: LIMITS.optionTextMax },
  options_min: { min: LIMITS.optionsMin },
  options_max: { max: LIMITS.optionsMax },
  max_votes_range: { max: LIMITS.maxVotesPerUserCap },
  max_options_range: { max: LIMITS.maxOptionsPerParticipantCap },
};

/** Turns API error codes (the contract with the backend) into texts in the current language. */
@Injectable({ providedIn: 'root' })
export class ErrorTexts {
  private readonly transloco = inject(TranslocoService);

  /** Translated text for an error code; unknown codes fall back to a generic message. */
  code(code: string): string {
    const key = `errors.${code}`;
    const known = key in this.transloco.getTranslation(this.transloco.getActiveLang());
    return this.transloco.translate(known ? key : 'errors.generic', ERROR_PARAMS[code] ?? {});
  }

  message(error: unknown): string {
    return error instanceof ApiError ? this.code(error.code) : this.transloco.translate('errors.generic');
  }

  /** Field name -> translated message, from an API validation error. */
  fields(error: unknown): Record<string, string> {
    if (!(error instanceof ApiError)) return {};
    return Object.fromEntries(
      Object.entries(error.fields).map(([field, codes]) => [field, this.code(codes[0] ?? error.code)]),
    );
  }
}
