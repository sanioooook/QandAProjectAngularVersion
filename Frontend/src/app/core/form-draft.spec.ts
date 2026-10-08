import { TestBed } from '@angular/core/testing';
import { FormBuilder } from '@angular/forms';
import { formDraft } from './form-draft';

const KEY = 'qanda.test.draft';

function draftFor(initial: { title: string }) {
  const form = new FormBuilder().nonNullable.group(initial);
  const draft = TestBed.runInInjectionContext(() =>
    formDraft({ key: KEY, value: () => form.getRawValue(), apply: (v) => form.setValue(v), changes: form.valueChanges }),
  );
  return { form, draft };
}

describe('formDraft', () => {
  afterEach(() => sessionStorage.clear());

  it('keeps edits and restores them in a new form', () => {
    const first = draftFor({ title: '' });
    first.draft.start();
    first.form.setValue({ title: 'Lunch?' });

    const second = draftFor({ title: '' });
    second.draft.start();

    expect(second.form.value.title).toBe('Lunch?');
    expect(second.draft.restored()).toBe(true);
  });

  it('does not keep a form that is back to its initial state', () => {
    const { form, draft } = draftFor({ title: '' });
    draft.start();
    form.setValue({ title: 'typed' });
    form.setValue({ title: '' });

    expect(sessionStorage.getItem(KEY)).toBeNull();
  });

  it('a fresh form restores nothing and clear removes the draft', () => {
    const { form, draft } = draftFor({ title: '' });
    draft.start();
    expect(draft.restored()).toBe(false);

    form.setValue({ title: 'x' });
    draft.clear();

    expect(sessionStorage.getItem(KEY)).toBeNull();
  });

  it('a stored draft equal to the initial form is dropped, not announced', () => {
    sessionStorage.setItem(KEY, JSON.stringify({ title: '' }));
    const { draft } = draftFor({ title: '' });

    draft.start();

    expect(draft.restored()).toBe(false);
    expect(sessionStorage.getItem(KEY)).toBeNull();
  });

  it('ignores a corrupt draft', () => {
    sessionStorage.setItem(KEY, '{not json');
    const { form, draft } = draftFor({ title: 'initial' });

    draft.start();

    expect(form.value.title).toBe('initial');
    expect(draft.restored()).toBe(false);
  });
});
