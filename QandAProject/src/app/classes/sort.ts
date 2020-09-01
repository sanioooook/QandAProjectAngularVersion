import { SortDirection } from './sort-direction.enum';

export class Sort<T> {
  sortBy: T;
  sortDirection: SortDirection;
}
