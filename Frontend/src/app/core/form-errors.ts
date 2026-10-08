import { AbstractControl, FormGroup } from '@angular/forms';

/**
 * Forms are checked on submit with the same rules as the API, and the API's own field errors are
 * shown the same way: the translated message goes into the control's errors as `{ message }`, which
 * <mat-error> displays. Editing the field clears it (the control re-validates on every change).
 */
export function showErrors(form: FormGroup, messages: Record<string, string>): void {
  for (const [name, message] of Object.entries(messages)) {
    const control = form.get(name);
    if (!control) continue;
    control.setErrors({ message });
    control.markAsTouched();
  }
}

export function errorMessage(control: AbstractControl): string {
  return (control.errors?.['message'] as string | undefined) ?? '';
}
