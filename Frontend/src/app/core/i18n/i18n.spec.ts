import { TestBed } from '@angular/core/testing';
import { TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { en } from './en';
import { detectLocale, provideI18n } from './i18n';
import { ru } from './ru';
import { uk } from './uk';

function keys(object: object, prefix = ''): string[] {
  return Object.entries(object).flatMap(([key, value]) =>
    typeof value === 'object' && value !== null ? keys(value, `${prefix}${key}.`) : [`${prefix}${key}`],
  );
}

describe('i18n', () => {
  it('every locale has exactly the same keys', () => {
    expect(keys(uk).sort()).toEqual(keys(en).sort());
    expect(keys(ru).sort()).toEqual(keys(en).sort());
  });

  it('no message uses an ASCII apostrophe, which ICU treats as an escape', () => {
    for (const messages of [en, uk, ru]) {
      expect(JSON.stringify(messages)).not.toContain("'");
    }
  });

  describe('plural forms', () => {
    let transloco: TranslocoService;

    beforeEach(async () => {
      TestBed.configureTestingModule({ providers: provideI18n() });
      transloco = TestBed.inject(TranslocoService);
      await Promise.all(['uk', 'en', 'ru'].map((lang) => firstValueFrom(transloco.load(lang))));
    });

    it.each([
      [1, '1 голос'],
      [3, '3 голоси'],
      [11, '11 голосів'],
      [21, '21 голос'],
      [22, '22 голоси'],
      [25, '25 голосів'],
    ])('Ukrainian: %i -> %s', (n, text) => {
      expect(transloco.translate('survey.votes', { n }, 'uk')).toBe(text);
    });

    it('Russian uses its own forms', () => {
      expect(transloco.translate('survey.voters', { n: 2 }, 'ru')).toBe('2 участника');
      expect(transloco.translate('survey.voters', { n: 5 }, 'ru')).toBe('5 участников');
    });

    it('English has a zero form', () => {
      expect(transloco.translate('survey.votes', { n: 0 }, 'en')).toBe('no votes');
      expect(transloco.translate('survey.votes', { n: 1 }, 'en')).toBe('1 vote');
      expect(transloco.translate('survey.votes', { n: 2 }, 'en')).toBe('2 votes');
    });

    it('fills in named values', () => {
      expect(transloco.translate('errors.name_length', { min: 2, max: 50 }, 'en')).toBe('Name must be 2–50 characters.');
    });
  });

  it.each([
    [['ru-RU', 'en'], 'ru'],
    [['de-DE', 'en-US'], 'en'],
    [['uk'], 'uk'],
    [['de', 'fr'], 'en'],
    [['pl-PL'], 'en'],
    [[], 'uk'],
  ])('detects %j as %s', (languages, expected) => {
    expect(detectLocale(languages)).toBe(expected);
  });
});
