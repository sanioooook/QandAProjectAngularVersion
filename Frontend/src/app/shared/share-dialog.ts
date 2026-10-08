import { ChangeDetectionStrategy, Component, ElementRef, inject, input, signal, viewChild } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { Toasts } from '../core/toasts';

interface ShareData {
  url: string;
  title: string;
}

@Component({
  selector: 'app-share-dialog',
  imports: [TranslocoDirective, MatDialogTitle, MatDialogContent, MatDialogClose, MatIconButton, MatButton, MatIcon, MatTooltip],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ng-container *transloco="let t">
      <div class="head">
        <h2 mat-dialog-title>{{ t('survey.share') }}</h2>
        <button mat-icon-button mat-dialog-close [attr.aria-label]="t('common.close')" [matTooltip]="t('common.close')">
          <mat-icon svgIcon="x" />
        </button>
      </div>
      <mat-dialog-content>
        <p class="muted hint">{{ t('survey.shareHint') }}</p>
        <div class="link-box">
          <code #link class="url" (click)="selectLink()">{{ data.url }}</code>
          <button
            mat-icon-button
            class="copy"
            [class.done]="copied()"
            [attr.aria-label]="copied() ? t('toast.linkCopied') : t('survey.copyLink')"
            [matTooltip]="copied() ? t('toast.linkCopied') : t('survey.copyLink')"
            (click)="copy()"
          >
            <mat-icon [svgIcon]="copied() ? 'check' : 'copy'" />
          </button>
          <span class="sr-only" aria-live="polite">{{ copied() ? t('toast.linkCopied') : '' }}</span>
        </div>
        @if (canNativeShare) {
          <button mat-stroked-button class="native" (click)="nativeShare()">
            <mat-icon svgIcon="share" />{{ t('survey.shareVia') }}
          </button>
        }
      </mat-dialog-content>
    </ng-container>
  `,
  styles: `
    .head {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding-right: 12px;
    }
    .hint {
      margin-bottom: 14px;
    }
    /* The link is shown whole and scrolls sideways: a long link must not be cut with an ellipsis. */
    .link-box {
      display: flex;
      align-items: center;
      gap: 4px;
      padding: 4px 4px 4px 14px;
      border: 1px solid var(--app-border);
      border-radius: 12px;
      background: var(--mat-sys-surface-container);
    }
    .url {
      flex: 1;
      min-width: 0;
      overflow-x: auto;
      white-space: nowrap;
      padding: 8px 0;
      font: 0.9rem/1.4 ui-monospace, 'Cascadia Code', Consolas, monospace;
      user-select: all;
      scrollbar-width: thin;
    }
    .copy.done {
      color: var(--app-success);
    }
    .native {
      width: 100%;
      margin-top: 14px;
    }
  `,
})
export class ShareDialogContent {
  protected readonly data = inject<ShareData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef);
  private readonly toasts = inject(Toasts);
  private readonly transloco = inject(TranslocoService);
  private readonly link = viewChild.required<ElementRef<HTMLElement>>('link');

  protected readonly copied = signal(false);
  protected readonly canNativeShare = typeof navigator !== 'undefined' && typeof navigator.share === 'function';
  private copiedTimer: ReturnType<typeof setTimeout> | undefined;

  protected selectLink(): void {
    const range = document.createRange();
    range.selectNodeContents(this.link().nativeElement);
    window.getSelection()?.removeAllRanges();
    window.getSelection()?.addRange(range);
  }

  protected async copy(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.data.url);
      this.copied.set(true);
      clearTimeout(this.copiedTimer);
      this.copiedTimer = setTimeout(() => this.copied.set(false), 2000);
    } catch {
      // Clipboard API unavailable (e.g. plain http on a LAN address): select the link for Ctrl+C.
      this.selectLink();
      this.toasts.error(this.transloco.translate('survey.copyManually'));
    }
  }

  protected async nativeShare(): Promise<void> {
    try {
      await navigator.share({ title: this.data.title, url: this.data.url });
      this.dialogRef.close();
    } catch {
      // Cancelled by the user.
    }
  }
}

/** The share icon in the corner of a survey; opens the dialog with the link. */
@Component({
  selector: 'app-share-button',
  imports: [TranslocoDirective, MatIconButton, MatIcon, MatTooltip],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ng-container *transloco="let t">
      <button mat-icon-button [attr.aria-label]="t('survey.share')" [matTooltip]="t('survey.share')" (click)="open()">
        <mat-icon svgIcon="share" />
      </button>
    </ng-container>
  `,
})
export class ShareButton {
  readonly url = input.required<string>();
  readonly title = input.required<string>();
  private readonly dialog = inject(MatDialog);

  open(): void {
    this.dialog.open(ShareDialogContent, {
      data: { url: this.url(), title: this.title() } satisfies ShareData,
      width: '480px',
      maxWidth: 'calc(100vw - 32px)',
      autoFocus: '.copy',
    });
  }
}
