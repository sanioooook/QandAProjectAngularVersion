import { describe, expect, it } from 'vitest';
import { clock, composeDeadline, effectiveStatus, remaining, splitDeadline, todayInput } from './deadline';

const KYIV = 'Europe/Kyiv';
const NEW_YORK = 'America/New_York';

describe('deadline', () => {
  it('a date without a time means the end of that day in the chosen zone', () => {
    expect(composeDeadline('2026-10-22', '', KYIV)!.toISOString()).toBe('2026-10-22T20:59:59.999Z');
    expect(composeDeadline('2026-10-22', '', NEW_YORK)!.toISOString()).toBe('2026-10-23T03:59:59.999Z');
    expect(composeDeadline('2026-10-22', '', 'UTC')!.toISOString()).toBe('2026-10-22T23:59:59.999Z');
  });

  it('a date with a time uses that wall time in the chosen zone', () => {
    expect(composeDeadline('2026-10-22', '18:30', KYIV)!.toISOString()).toBe('2026-10-22T15:30:00.000Z');
    // Winter time in Kyiv is UTC+2.
    expect(composeDeadline('2026-12-01', '18:30', KYIV)!.toISOString()).toBe('2026-12-01T16:30:00.000Z');
  });

  it('no or malformed date means no deadline', () => {
    expect(composeDeadline('', '18:30', KYIV)).toBeNull();
    expect(composeDeadline('not-a-date', '', KYIV)).toBeNull();
    expect(composeDeadline('2026-10-22', '25', KYIV)).toBeNull();
  });

  it('round-trips through the form fields in any zone', () => {
    for (const zone of [KYIV, NEW_YORK, 'Asia/Tokyo', 'UTC']) {
      expect(splitDeadline(composeDeadline('2026-10-22', '', zone)!.toISOString(), zone)).toEqual({ date: '2026-10-22', time: '' });
      expect(splitDeadline(composeDeadline('2026-10-22', '09:05', zone)!.toISOString(), zone)).toEqual({ date: '2026-10-22', time: '09:05' });
    }
    expect(splitDeadline(null, KYIV)).toEqual({ date: '', time: '' });
  });

  it('shows the same moment as the matching wall time of another zone', () => {
    const kyivEndOfDay = composeDeadline('2026-10-22', '', KYIV)!.toISOString();

    expect(splitDeadline(kyivEndOfDay, NEW_YORK)).toEqual({ date: '2026-10-22', time: '16:59' });
  });

  it('today depends on the zone around midnight', () => {
    const lateEveningInNewYork = Date.parse('2026-10-23T02:00:00Z');

    expect(todayInput(NEW_YORK, lateEveningInNewYork)).toBe('2026-10-22');
    expect(todayInput(KYIV, lateEveningInNewYork)).toBe('2026-10-23');
  });

  it('counts down and never goes negative', () => {
    const now = Date.parse('2026-10-20T10:00:00Z');

    const left = remaining('2026-10-22T12:34:56Z', now);

    expect(left).toMatchObject({ days: 2, hours: 2, minutes: 34, seconds: 56 });
    expect(clock(left)).toBe('02:34:56');
    expect(remaining('2026-10-19T00:00:00Z', now).totalMs).toBe(0);
  });

  it('an active survey whose deadline passed on the client is shown as closed', () => {
    const now = Date.parse('2026-10-20T10:00:00Z');

    expect(effectiveStatus('active', '2026-10-20T09:59:59Z', now)).toBe('closed');
    expect(effectiveStatus('active', '2026-10-20T10:00:01Z', now)).toBe('active');
    expect(effectiveStatus('active', null, now)).toBe('active');
    expect(effectiveStatus('draft', '2026-10-01T00:00:00Z', now)).toBe('draft');
  });
});
