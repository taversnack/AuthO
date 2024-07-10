import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { MatSelectChange } from '@angular/material/select';
import { Subject, distinctUntilChanged } from 'rxjs';
import { MaterialModule } from 'src/app/material/material.module';
import { camelCaseToSpacedPascalCase } from '../../lib/utilities';

export interface IFilter {
  name: string;
  display?: string;
  selected?: boolean;
}

@Component({
  selector: 'app-search-filters',
  standalone: true,
  imports: [CommonModule, MaterialModule],
  templateUrl: './search-filters.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchFiltersComponent implements OnInit, OnDestroy {
  @Input() allowMultiple = true;
  @Input() filters: (string | IFilter)[] = [];
  @Input() defaultSelection = false;
  @Input() defaultDisplayTransformer: (name: string) => string = camelCaseToSpacedPascalCase;

  @Output() filtersChanged = new EventEmitter<string[]>();

  private filtersChangedSubject = new Subject<string[]>();
  private filtersChangedSubscription = this.filtersChangedSubject.pipe(
    // safe to just compare length as during normal operation; user can only modify one at a time
    distinctUntilChanged((x, y) => x.length === y.length),
  ).subscribe(x => this.filtersChanged.emit(x));

  mappedFilters: Required<IFilter>[] = [];

  initialValue: string[] = [];

  ngOnInit(): void {
    this.mappedFilters = this.filters.map(
      filter => typeof filter === 'string' ?
      { name: filter, selected: this.allowMultiple && this.defaultSelection, display: this.defaultDisplayTransformer(filter) }
      :
      { selected: this.allowMultiple && this.defaultSelection, display: this.defaultDisplayTransformer(filter.name), ...filter }
    );

    this.initialValue = this.mappedFilters.filter(({ selected }) => selected).map(({ name }) => name);
  }

  ngOnDestroy(): void {
    this.filtersChangedSubscription.unsubscribe();
  }

  emitIfFiltersChanged({ value }: MatSelectChange) {
    this.filtersChangedSubject.next(value);
  }
}
