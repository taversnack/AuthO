import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime, filter } from 'rxjs';
import { MaterialModule } from 'src/app/material/material.module';
import { SearchFilters } from 'src/app/shared/models/search-filters';
import { faMagnifyingGlass } from '@fortawesome/free-solid-svg-icons';

export interface IFilterSettings {
  allowMultiple?: boolean;
  defaultSelection?: boolean;
  defaultDisplayTransformer?: (name: string) => string;
}

@Component({
  selector: 'app-search-bar-v2',
  standalone: true,
  imports: [CommonModule, MaterialModule, ReactiveFormsModule, FormsModule],
  templateUrl: './search-bar.component.html',
  styleUrls: ['search-bar.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchBarComponentV2 implements OnInit {

  private readonly destroyRef = inject(DestroyRef);

  @Input() label = 'Search';
  @Input() maxLength = 256;
  @Input() useDebounceMilliseconds?: number;
  @Input() filters: SearchFilters = [];
  @Input() enableAutoSearch: boolean = false;

  @Output() searchSubmitted = new EventEmitter<{ query: string; filterBy?: string }>();

  formControl = new FormControl('', [Validators.maxLength(this.maxLength)]);

  // Font Awesome
  faMagnifyingGlass = faMagnifyingGlass

  ngOnInit(): void {
    if (this.enableAutoSearch) { // Only subscribe to automatic search if enabled
      this.formControl.valueChanges.pipe(
        takeUntilDestroyed(this.destroyRef),
        debounceTime(this.useDebounceMilliseconds || 0), // Use the provided debounce time or default to 0
        filter(value => value != null && typeof value === 'string' && value.length > 3) // Check that value is not null and length is greater than 3
      ).subscribe(value => {
        this.emitSearch();
      });
    }

    else if (this.useDebounceMilliseconds !== undefined) {
      this.formControl.valueChanges.pipe(
        takeUntilDestroyed(this.destroyRef),
        debounceTime(this.useDebounceMilliseconds)
      ).subscribe(() => this.emitSearch());
    }
  }

  clearInput() {
    this.formControl.setValue('');
    this.emitSearch()
  }

  submitSearch() {
    if(this.useDebounceMilliseconds === undefined) {
      this.emitSearch();
    }
  }

  private emitSearch() {
  const query = this.formControl.value ?? '';
  this.searchSubmitted.emit({ query: query });
  }
}
