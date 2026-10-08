import { describe, expect, it } from 'vitest';
import { avatarHue, initials } from './image';

describe('letter avatar', () => {
  it.each([
    ['Alice', 'A'],
    ['alice smith', 'AS'],
    ['  Олена   Петрівна  Ковальчук ', 'ОК'],
    ['😀 Happy', '😀H'],
    ['', '?'],
  ])('initials of %j are %s', (name, expected) => {
    expect(initials(name)).toBe(expected);
  });

  it('gives the same name the same colour and spreads different names', () => {
    expect(avatarHue('Alice')).toBe(avatarHue('Alice'));
    const hues = new Set(['Alice', 'Bob', 'Carol', 'Dana', 'Eve'].map(avatarHue));
    expect(hues.size).toBeGreaterThan(3);
    for (const hue of hues) expect(hue).toBeGreaterThanOrEqual(0);
  });
});
