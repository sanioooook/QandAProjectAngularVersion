import { DestroyRef, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';

export interface FormDraftSource<T> {
  /** sessionStorage key, one per form (e.g. per edited survey). */
  key: string;
  /** The form's current value. */
  value: () => T;
  /** Puts a saved value back into the form. */
  apply: (value: T) => void;
  /** Emits on every edit. */
  changes: Observable<unknown>;
}

/**
 * Keeps what the user typed in sessionStorage (this tab only), so it survives an expired session
 * (sign in again, come back to the same page) or an accidental reload. Removed once saved.
 * Call from an injection context.
 */
export function formDraft<T>(source: FormDraftSource<T>) {
  const restored = signal(false);
  let active = false;
  /** The form as it was loaded; a form equal to it has nothing worth keeping. */
  let pristine = '';

  function read(): T | null {
    try {
      const raw = sessionStorage.getItem(source.key);
      return raw ? (JSON.parse(raw) as T) : null;
    } catch {
      return null;
    }
  }

  function clear(): void {
    restored.set(false);
    try {
      sessionStorage.removeItem(source.key);
    } catch {
      // Storage unavailable: nothing was saved either.
    }
  }

  const subscription = source.changes.subscribe(() => {
    if (!active) return;
    const json = JSON.stringify(source.value());
    try {
      if (json === pristine) sessionStorage.removeItem(source.key);
      else sessionStorage.setItem(source.key, json);
    } catch {
      // Storage full or blocked: the form still works, it just is not remembered.
    }
  });
  inject(DestroyRef).onDestroy(() => subscription.unsubscribe());

  return {
    restored: restored.asReadonly(),
    clear,
    /** Call once the form holds its initial values; applies a saved draft on top, then starts saving. */
    start(): void {
      pristine = JSON.stringify(source.value());
      const saved = read();
      if (saved) {
        source.apply(saved);
        // A draft identical to the loaded form holds nothing to restore.
        if (JSON.stringify(source.value()) === pristine) clear();
        else restored.set(true);
      }
      active = true;
    },
  };
}
