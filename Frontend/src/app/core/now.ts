import { DestroyRef, Signal, inject, signal } from '@angular/core';

/** Current time (ms) that ticks every `intervalMs` while the calling component lives. */
export function injectNow(intervalMs = 1000): Signal<number> {
  const now = signal(Date.now());
  const timer = setInterval(() => now.set(Date.now()), intervalMs);
  inject(DestroyRef).onDestroy(() => clearInterval(timer));
  return now.asReadonly();
}
