import type { SurveyStatus } from '../api/types';
import { wallTime, zonedToInstant } from './time-zone';

const pad = (n: number) => String(n).padStart(2, '0');

/**
 * Deadline from the form, read in `timeZone`: the date is required, the time optional.
 * Without a time the survey runs to the very end of that day (23:59:59.999 in that zone).
 */
export function composeDeadline(date: string, time: string, timeZone: string): Date | null {
  const d = /^(\d{4})-(\d{2})-(\d{2})$/.exec(date);
  if (!d) return null;
  const t = /^(\d{2}):(\d{2})$/.exec(time);
  if (time && !t) return null;
  const [year, month, day] = [Number(d[1]), Number(d[2]), Number(d[3])];
  const instant = t
    ? zonedToInstant({ year, month, day, hour: Number(t[1]), minute: Number(t[2]), second: 0 }, timeZone)
    : zonedToInstant({ year, month, day, hour: 23, minute: 59, second: 59, ms: 999 }, timeZone);
  return Number.isNaN(instant) ? null : new Date(instant);
}

/** Inverse of composeDeadline: an end-of-day deadline is shown as a date without a time. */
export function splitDeadline(iso: string | null, timeZone: string): { date: string; time: string } {
  if (!iso) return { date: '', time: '' };
  const w = wallTime(new Date(iso).getTime(), timeZone);
  const date = `${w.year}-${pad(w.month)}-${pad(w.day)}`;
  const endOfDay = w.hour === 23 && w.minute === 59 && w.second === 59;
  return { date, time: endOfDay ? '' : `${pad(w.hour)}:${pad(w.minute)}` };
}

/** `yyyy-mm-dd` of today in `timeZone`, for the `min` of the date input. */
export function todayInput(timeZone: string, now = Date.now()): string {
  const w = wallTime(now, timeZone);
  return `${w.year}-${pad(w.month)}-${pad(w.day)}`;
}

export interface Remaining {
  days: number;
  hours: number;
  minutes: number;
  seconds: number;
  totalMs: number;
}

export function remaining(deadline: string, now: number): Remaining {
  const totalMs = Math.max(0, new Date(deadline).getTime() - now);
  const totalSeconds = Math.floor(totalMs / 1000);
  return {
    days: Math.floor(totalSeconds / 86_400),
    hours: Math.floor((totalSeconds % 86_400) / 3600),
    minutes: Math.floor((totalSeconds % 3600) / 60),
    seconds: totalSeconds % 60,
    totalMs,
  };
}

export function clock({ hours, minutes, seconds }: Remaining): string {
  return `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
}

/**
 * The server decides the status when the data is fetched; a survey can close while it is on screen.
 * Treat an active survey whose deadline has passed on the client clock as closed.
 */
export function effectiveStatus(status: SurveyStatus, deadline: string | null, now: number): SurveyStatus {
  return status === 'active' && deadline !== null && new Date(deadline).getTime() <= now ? 'closed' : status;
}
