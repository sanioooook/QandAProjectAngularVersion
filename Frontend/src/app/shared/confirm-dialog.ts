import { ChangeDetectionStrategy, Component, Injectable, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogActions, MatDialogClose, MatDialogContent } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';

interface ConfirmData {
  message: string;
  confirm: string;
  cancel: string;
}

@Component({
  selector: 'app-confirm-dialog',
  imports: [MatDialogContent, MatDialogActions, MatDialogClose, MatButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button [mat-dialog-close]="false">{{ data.cancel }}</button>
      <button mat-flat-button class="danger" [mat-dialog-close]="true" cdkFocusInitial>{{ data.confirm }}</button>
    </mat-dialog-actions>
  `,
  styles: `
    p {
      padding-top: 8px;
    }
    .danger {
      --mat-button-filled-container-color: var(--mat-sys-error);
      --mat-button-filled-label-text-color: var(--mat-sys-on-error);
    }
  `,
})
export class ConfirmDialogContent {
  protected readonly data = inject<ConfirmData>(MAT_DIALOG_DATA);
}

/** Asks before something that cannot be undone. Resolves to true when confirmed. */
@Injectable({ providedIn: 'root' })
export class Confirm {
  private readonly dialog = inject(MatDialog);

  ask(data: ConfirmData): Promise<boolean> {
    const ref = this.dialog.open<ConfirmDialogContent, ConfirmData, boolean>(ConfirmDialogContent, {
      data,
      width: '420px',
      maxWidth: 'calc(100vw - 32px)',
      role: 'alertdialog',
    });
    return firstValueFrom(ref.afterClosed()).then((result) => result === true);
  }
}
