import { afterEach, describe, expect, it, vi } from 'vitest';
import { formatDate } from './format';
import { allTimeZones, browserTimeZone, currentName, isTimeZone, zonedToInstant, zoneName, zoneOffsetLabel, zoneOffsetMs } from './time-zone';

const HOUR = 3_600_000;

describe('time zones', () => {
  it('knows summer and winter offsets', () => {
    expect(zoneOffsetMs(Date.parse('2026-07-01T12:00:00Z'), 'Europe/Kyiv')).toBe(3 * HOUR);
    expect(zoneOffsetMs(Date.parse('2026-12-01T12:00:00Z'), 'Europe/Kyiv')).toBe(2 * HOUR);
    expect(zoneOffsetMs(Date.parse('2026-07-01T12:00:00Z'), 'America/New_York')).toBe(-4 * HOUR);
    expect(zoneOffsetMs(Date.parse('2026-07-01T12:00:00Z'), 'Asia/Kolkata')).toBe(5.5 * HOUR);
  });

  it('converts wall time on the days clocks change', () => {
    // Kyiv moves from UTC+3 to UTC+2 at 04:00 local on 2026-10-25.
    expect(new Date(zonedToInstant({ year: 2026, month: 10, day: 25, hour: 2, minute: 0, second: 0 }, 'Europe/Kyiv')).toISOString())
      .toBe('2026-10-24T23:00:00.000Z');
    expect(new Date(zonedToInstant({ year: 2026, month: 10, day: 25, hour: 12, minute: 0, second: 0 }, 'Europe/Kyiv')).toISOString())
      .toBe('2026-10-25T10:00:00.000Z');
  });

  it('labels offsets the way people read them', () => {
    const summer = Date.parse('2026-07-01T12:00:00Z');

    expect(zoneOffsetLabel('Europe/Kyiv', summer)).toBe('GMT+3');
    expect(zoneOffsetLabel('America/New_York', summer)).toBe('GMT-4');
    expect(zoneOffsetLabel('UTC', summer)).toBe('UTC');
    expect(zoneName('America/Los_Angeles', summer)).toBe('America/Los Angeles (GMT-7)');
  });

  it('formats a deadline with its offset, in the chosen zone', () => {
    const deadline = '2026-10-22T20:59:59.999Z';

    expect(formatDate(deadline, 'en', 'Europe/Kyiv', true)).toBe('Oct 22, 2026, 11:59 PM GMT+3');
    expect(formatDate(deadline, 'en', 'America/New_York', true)).toBe('Oct 22, 2026, 4:59 PM GMT-4');
    expect(formatDate(deadline, 'uk', 'Europe/Kyiv')).toContain('23:59');
  });

  describe('a browser that reports an old zone name', () => {
    afterEach(() => vi.restoreAllMocks());

    it('is shown under the current name, and listed once', () => {
      const original = Intl.DateTimeFormat.prototype.resolvedOptions;
      vi.spyOn(Intl.DateTimeFormat.prototype, 'resolvedOptions').mockImplementation(function (this: Intl.DateTimeFormat) {
        return { ...original.call(this), timeZone: 'Europe/Kiev' };
      });

      expect(browserTimeZone()).toBe('Europe/Kyiv');
      expect(allTimeZones().filter((z) => z === 'Europe/Kiev' || z === 'Europe/Kyiv')).toEqual(['Europe/Kyiv']);
    });
  });

  it('maps renamed zones to their current names', () => {
    expect(currentName('Europe/Kiev')).toBe('Europe/Kyiv');
    expect(currentName('Asia/Calcutta')).toBe('Asia/Kolkata');
    expect(currentName('Europe/Paris')).toBe('Europe/Paris');
  });

  it('validates zone names and lists them', () => {
    expect(isTimeZone('Europe/Kyiv')).toBe(true);
    expect(isTimeZone('Mars/Olympus_Mons')).toBe(false);
    expect(isTimeZone('')).toBe(false);
    expect(isTimeZone(null)).toBe(false);
    expect(allTimeZones()).toContain('Europe/Kyiv');
    expect(allTimeZones()).toContain('UTC');
    expect(allTimeZones()).not.toContain('Europe/Kiev');
  });
});
