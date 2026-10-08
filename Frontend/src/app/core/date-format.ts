import { Injectable, inject } from '@angular/core';
import { PrefsStore } from './stores/prefs.store';
import { formatDate, formatRelative } from './utils/format';

/**
 * Dates in the UI language and the time zone chosen in the settings (the device's by default).
 * The methods read signals, so templates using them update when either setting changes.
 */
@Injectable({ providedIn: 'root' })
export class DateFormat {
  private readonly prefs = inject(PrefsStore);

  date(iso: string): string {
    return formatDate(iso, this.prefs.locale(), this.prefs.effectiveTimeZone());
  }

  /** With the offset ("… 23:59 GMT+3"): for deadlines, where the exact moment matters. */
  dateWithZone(iso: string): string {
    return formatDate(iso, this.prefs.locale(), this.prefs.effectiveTimeZone(), true);
  }

  /** Tooltip text naming the zone in full. */
  title(iso: string): string {
    return `${this.dateWithZone(iso)} · ${this.prefs.effectiveTimeZone().replace(/_/g, ' ')}`;
  }

  relative(iso: string, now = Date.now()): string {
    return formatRelative(iso, this.prefs.locale(), now);
  }
}
