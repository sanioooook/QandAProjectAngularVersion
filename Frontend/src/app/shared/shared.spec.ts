import { render, screen } from '@testing-library/angular';
import { CharCount } from './char-count';
import { UserAvatar } from './user-avatar';

describe('CharCount', () => {
  it.each([
    ['a'.repeat(159), ''],
    ['a'.repeat(160), '160/200'],
    ['a'.repeat(200), '200/200'],
  ])('for %#: shows %j', async (value, text) => {
    const { container } = await render(CharCount, { inputs: { value, max: 200 } });
    expect(container.textContent?.trim()).toBe(text);
  });

  it.each([
    [200, true],
    [190, false],
  ])('at %i of 200 characters marks the limit: %s', async (length, marked) => {
    const { container } = await render(CharCount, { inputs: { value: 'a'.repeat(length), max: 200 } });
    expect(container.querySelector('.full') !== null).toBe(marked);
  });
});

describe('UserAvatar', () => {
  it('shows initials without a photo', async () => {
    await render(UserAvatar, { inputs: { name: 'Alice Smith' } });
    expect(screen.getByText('AS')).toBeTruthy();
  });

  it('falls back to initials when the photo fails to load', async () => {
    const { container, fixture } = await render(UserAvatar, { inputs: { name: 'Bob', url: '/missing.webp' } });
    container.querySelector('img')!.dispatchEvent(new Event('error'));
    await fixture.whenStable();
    expect(screen.getByText('B')).toBeTruthy();
  });
});
