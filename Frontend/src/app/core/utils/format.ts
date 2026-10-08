import { zoneOffsetLabel } from './time-zone';

const UNITS: [Intl.RelativeTimeFormatUnit, number][] = [
  ['year', 365 * 24 * 3600],
  ['month', 30 * 24 * 3600],
  ['week', 7 * 24 * 3600],
  ['day', 24 * 3600],
  ['hour', 3600],
  ['minute', 60],
  ['second', 1],
];

export function formatRelative(iso: string, locale: string, now = Date.now()): string {
  const seconds = (new Date(iso).getTime() - now) / 1000;
  const [unit, size] = UNITS.find(([, s]) => Math.abs(seconds) >= s) ?? ['second', 1];
  return new Intl.RelativeTimeFormat(locale, { numeric: 'auto' }).format(Math.round(seconds / size), unit);
}

export function formatDate(iso: string, locale: string, timeZone: string, withZone = false): string {
  const text = new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short', timeZone }).format(new Date(iso));
  return withZone ? `${text} ${zoneOffsetLabel(timeZone, new Date(iso).getTime())}` : text;
}
