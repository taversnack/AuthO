import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime } from 'rxjs';
import { MaterialModule } from 'src/app/material/material.module';
import { camelCaseToSpacedPascalCase } from '../../lib/utilities';
import { IFilter, SearchFilters } from '../../models/search-filters';

export interface IFilterSettings {
  allowMultiple?: boolean;
  defaultSelection?: boolean;
  defaultDisplayTransformer?: (name: string) => string;
}

const defaultFilterSettings: Required<IFilterSettings> = {
  allowMultiple: true,
  defaultSelection: true,
  defaultDisplayTransformer: camelCaseToSpacedPascalCase,
};

@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [CommonModule, MaterialModule, ReactiveFormsModule, FormsModule],
  templateUrl: './search-bar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchBarComponent implements OnInit {

  private readonly destroyRef = inject(DestroyRef);

  @Input() label = 'Search';
  @Input() maxLength = 256;
  @Input() useDebounceMilliseconds?: number;
  @Input() filters: SearchFilters = [];
  // @Input() filters$?: Observable<SearchFilters>;
  @Input() filterSettings?: IFilterSettings;
  @Input() searchField: string = 'firstName';

  @Output() searchSubmitted = new EventEmitter<{ query: string; filterBy?: string }>();
  @Output() filtersChanged = new EventEmitter<string[]>();

  formControl = new FormControl('', [Validators.maxLength(this.maxLength)]);

  mappedFilters: Required<IFilter>[] = [];

  initialFiltersValue: string[] = [];

  ngOnInit(): void {
    if(this.useDebounceMilliseconds !== undefined) {
      this.formControl.valueChanges.pipe(
        takeUntilDestroyed(this.destroyRef),
        debounceTime(this.useDebounceMilliseconds)
      ).subscribe(() => this.emitSearch());
    }

    const { allowMultiple, defaultSelection, defaultDisplayTransformer }: Required<IFilterSettings> = { ...defaultFilterSettings, ...this.filterSettings };

    this.mappedFilters = this.filters.map(
      filter => typeof filter === 'string' ?
      { name: filter, selected: allowMultiple && defaultSelection, display: defaultDisplayTransformer(filter) }
      :
      { selected: allowMultiple && defaultSelection, display: defaultDisplayTransformer(filter.name), ...filter }
    );

    this.initialFiltersValue = this.mappedFilters.filter(({ selected }) => selected).map(({ name }) => name);

    // this.filters$?.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(filters => {
    //   this.mappedFilters = filters.map(
    //     filter => typeof filter === 'string' ?
    //     { name: filter, selected: allowMultiple && defaultSelection, display: defaultDisplayTransformer(filter) }
    //     :
    //     { selected: allowMultiple && defaultSelection, display: defaultDisplayTransformer(filter.name), ...filter }
    //   );
    //   this.initialFiltersValue = this.mappedFilters.filter(({ selected }) => selected).map(({ name }) => name);
    // });
  }

  submitSearch() {
    if(this.useDebounceMilliseconds === undefined) {
      this.emitSearch();
    }
  }

  filterValueChanged(filter: Required<IFilter>) {
    filter.selected = !filter.selected;
    this.filtersChanged.emit(this.mappedFilters.filter(({ selected }) => selected).map(({ name }) => name));
  }

  private emitSearch() {
  const query = this.formControl.value ?? '';
  this.searchSubmitted.emit({ query: query, filterBy: this.searchField });
  }
}
