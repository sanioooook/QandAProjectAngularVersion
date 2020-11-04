import { Injectable } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { TranslocoService } from '@ngneat/transloco';

const ITEMS_PER_PAGE = 'matPaginator.itemsPerPageLabel';
const NEXT_PAGE = 'matPaginator.nextPageLabel';
const PREV_PAGE = 'matPaginator.previousPageLabel';
const FIRST_PAGE = 'matPaginator.firstPageLabel';
const LAST_PAGE = 'matPaginator.lastPageLabel';

@Injectable()
export class MatPaginatorIntlCro extends MatPaginatorIntl {

  constructor(private translocoService: TranslocoService) {
    super();

    this.translocoService.langChanges$.subscribe(() => {
      this.getAndInitTranslations();
    });

    this.getAndInitTranslations();
  }

  // tslint:disable-next-line: only-arrow-functions
  getRangeLabel = function(page: number, pageSize: number, length: number): string {
    if (length === 0 || pageSize === 0) {
      return this.translocoService.translate('matPaginator.nullOf', { length });
    }
    length = Math.max(length, 0);
    const startIndex = page * pageSize;
    // If the start index exceeds the list length, do not try and fix the end index to the end.
    const endIndex = startIndex < length ?
      Math.min(startIndex + pageSize, length) :
      startIndex + pageSize;
    return this.translocoService.translate('matPaginator.indexOf',
      { length, startIndex: startIndex + 1, endIndex });
  };

  public getAndInitTranslations(): void {
    this.translocoService.selectTranslate([
      ITEMS_PER_PAGE,
      NEXT_PAGE,
      PREV_PAGE,
      FIRST_PAGE,
      LAST_PAGE,
    ])
      .subscribe((translation: any) => {
        this.itemsPerPageLabel = translation[0];
        this.nextPageLabel = translation[1];
        this.previousPageLabel = translation[2];
        this.firstPageLabel = translation[3];
        this.lastPageLabel = translation[4];

        this.changes.next();
      });
  }
}
