import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslocoService } from '@jsverse/transloco';

/** Short confirmations and errors at the bottom of the screen. */
@Injectable({ providedIn: 'root' })
export class Toasts {
  private readonly snackBar = inject(MatSnackBar);
  private readonly transloco = inject(TranslocoService);

  success(message: string): void {
    this.snackBar.open(message, undefined, { duration: 4000, panelClass: 'toast-success' });
  }

  error(message: string): void {
    this.snackBar.open(message, this.transloco.translate('common.close'), {
      duration: 6000,
      panelClass: 'toast-error',
      politeness: 'assertive',
    });
  }
}
