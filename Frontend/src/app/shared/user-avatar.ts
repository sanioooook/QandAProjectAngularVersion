import { ChangeDetectionStrategy, Component, computed, effect, input, signal } from '@angular/core';
import { avatarHue, initials } from '../core/utils/image';

/** The user's photo, or their initials on a color derived from the name. */
@Component({
  selector: 'app-user-avatar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    'aria-hidden': 'true',
    '[style.width.px]': 'size()',
    '[style.height.px]': 'size()',
    '[style.font-size.px]': 'size() * 0.4',
    '[style.--hue]': 'hue()',
  },
  template: `
    @if (url() && !failed()) {
      <img [src]="url()" alt="" loading="lazy" (error)="failed.set(true)" />
    } @else {
      <span>{{ letters() }}</span>
    }
  `,
  styles: `
    :host {
      display: inline-grid;
      place-items: center;
      flex: none;
      overflow: hidden;
      border-radius: 50%;
      background: light-dark(hsl(var(--hue) 55% 88%), hsl(var(--hue) 45% 28%));
      color: light-dark(hsl(var(--hue) 45% 28%), hsl(var(--hue) 55% 88%));
      font-weight: 650;
      line-height: 1;
      user-select: none;
    }
    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
  `,
})
export class UserAvatar {
  readonly name = input.required<string>();
  readonly url = input<string | null>(null);
  readonly size = input(32);

  // A broken image (deleted meanwhile, offline) falls back to the letters.
  protected readonly failed = signal(false);
  protected readonly letters = computed(() => initials(this.name()));
  protected readonly hue = computed(() => avatarHue(this.name()));

  constructor() {
    effect(() => {
      this.url();
      this.failed.set(false);
    });
  }
}
