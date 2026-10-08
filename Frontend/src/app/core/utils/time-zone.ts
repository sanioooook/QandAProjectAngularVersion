// Time zone helpers on top of Intl (no date library). Instants are epoch milliseconds (UTC).

export interface WallTime {
  year: number;
  month: number; // 1-12
  day: number;
  hour: number;
  minute: number;
  second: number;
}

/** The device's zone under its current name (browsers may still report e.g. Europe/Kiev). */
export function browserTimeZone(): string {
  return currentName(Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC');
}

export function isTimeZone(value: unknown): value is string {
  if (typeof value !== 'string' || !value) return false;
  try {
    new Intl.DateTimeFormat('en', { timeZone: value });
    return true;
  } catch {
    return false;
  }
}

// Some ICU versions still list zones under their old names; show the current one where it is understood.
const RENAMED: Record<string, string> = {
  'Europe/Kiev': 'Europe/Kyiv',
  'Europe/Uzhgorod': 'Europe/Kyiv',
  'Europe/Zaporozhye': 'Europe/Kyiv',
  'Asia/Calcutta': 'Asia/Kolkata',
  'Asia/Saigon': 'Asia/Ho_Chi_Minh',
  'Asia/Rangoon': 'Asia/Yangon',
  'Asia/Katmandu': 'Asia/Kathmandu',
  'Atlantic/Faeroe': 'Atlantic/Faroe',
  'America/Godthab': 'America/Nuuk',
  'Pacific/Ponape': 'Pacific/Pohnpei',
  'Pacific/Truk': 'Pacific/Chuuk',
};

/** The current name of a renamed zone, when this environment understands it. */
export function currentName(zone: string): string {
  const renamed = RENAMED[zone];
  return renamed && isTimeZone(renamed) ? renamed : zone;
}

export function allTimeZones(): string[] {
  const zones = typeof Intl.supportedValuesOf === 'function' ? Intl.supportedValuesOf('timeZone') : [];
  return [...new Set([...zones.map(currentName), browserTimeZone(), 'UTC'])].sort();
}

/** Calendar date and clock time of `instant` as seen in `timeZone`. */
export function wallTime(instant: number, timeZone: string): WallTime {
  const parts = new Intl.DateTimeFormat('en-US', {
    timeZone,
    hourCycle: 'h23',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  }).formatToParts(new Date(instant));
  const get = (type: Intl.DateTimeFormatPartTypes) => Number(parts.find((p) => p.type === type)?.value);
  return { year: get('year'), month: get('month'), day: get('day'), hour: get('hour'), minute: get('minute'), second: get('second') };
}

/** How far `timeZone` is ahead of UTC at `instant`, in ms (Kyiv in summer: +3 h). */
export function zoneOffsetMs(instant: number, timeZone: string): number {
  const w = wallTime(instant, timeZone);
  const asIfUtc = Date.UTC(w.year, w.month - 1, w.day, w.hour, w.minute, w.second);
  return asIfUtc - Math.floor(instant / 1000) * 1000;
}

/** The instant at which the clocks in `timeZone` show the given wall time (DST aware). */
export function zonedToInstant(w: WallTime & { ms?: number }, timeZone: string): number {
  const guess = Date.UTC(w.year, w.month - 1, w.day, w.hour, w.minute, w.second, w.ms ?? 0);
  const first = guess - zoneOffsetMs(guess, timeZone);
  // Near a DST switch the offset at the guess and at the result can differ: use the latter.
  const offset = zoneOffsetMs(first, timeZone);
  return guess - offset;
}

/** Short offset label such as "GMT+3" or "GMT-4" (or "UTC"). */
export function zoneOffsetLabel(timeZone: string, instant = Date.now()): string {
  const part = new Intl.DateTimeFormat('en-US', { timeZone, timeZoneName: 'shortOffset' })
    .formatToParts(new Date(instant))
    .find((p) => p.type === 'timeZoneName');
  if (!part) return timeZone;
  // ICU spells a zero offset as "GMT" or "GMT+0" depending on the version.
  return part.value === 'GMT' || part.value === 'GMT+0' ? 'UTC' : part.value;
}

/** "Europe/Kyiv (GMT+3)" for pickers and hints. */
export function zoneName(timeZone: string, instant = Date.now()): string {
  return `${timeZone.replace(/_/g, ' ')} (${zoneOffsetLabel(timeZone, instant)})`;
}
