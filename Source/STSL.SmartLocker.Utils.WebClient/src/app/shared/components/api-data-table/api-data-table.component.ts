import { CdkTable, CdkTableModule } from '@angular/cdk/table';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, EventEmitter, Input, OnInit, Output, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSortModule, Sort } from '@angular/material/sort';
import { Observable, map, of, tap } from 'rxjs';
import { IPagingRequest, IPagingResponse, ISortedRequest, SortOrder } from '../../models/api';
import { DATA_TABLE_COLUMN_SETTINGS, ITableColumnSettings, ITablePaginationSettings, ITableSettings, StickyPosition, TableColumn, defaultTableColumnSettings, defaultTablePaginationSettings, defaultTableSettings } from '../../models/data-table';
import { PureFunctionPipe } from "../../pipes/pure-function.pipe";
import { MatTooltipModule } from '@angular/material/tooltip';

interface IMappedTableColumn<C> extends Required<ITableColumnSettings> {
  headingDisplay: string;
  name: string;
  tooltip: string;
  template?: TemplateRef<C>;
  getValue?: (rowValue: C) => string;
}

@Component({
  selector: 'app-api-data-table',
  standalone: true,
  templateUrl: './api-data-table.component.html',
  styleUrls: ['./api-data-table.component.css'],
  host: { 'class': 'contents' },
  imports: [CommonModule, CdkTableModule, MatPaginatorModule, MatSortModule, MatProgressSpinnerModule, PureFunctionPipe, MatTooltipModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApiDataTableComponent<T> implements OnInit {
  private readonly defaultColumnSettings: Required<ITableColumnSettings> = inject(DATA_TABLE_COLUMN_SETTINGS, { optional: true }) ?? defaultTableColumnSettings;
  private readonly destroyRef = inject(DestroyRef);

  @Input({ required: true }) data$?: Observable<IPagingResponse<T> | undefined>;
  @Input({ required: true }) columns?: TableColumn<T>[] | Observable<TableColumn<T>[]>;
  @Input() showLoadingIndicator$: Observable<boolean> = of(false);
  @Input() paginationSettings: ITablePaginationSettings = defaultTablePaginationSettings;
  @Input() tableSettings: ITableSettings = defaultTableSettings;

  @Output() pagingEvent = new EventEmitter<IPagingRequest>();
  @Output() sortingEvent = new EventEmitter<ISortedRequest | undefined>();

  @ViewChild(CdkTable, { read: ElementRef }) tableElementRef?: ElementRef;

  columnNames: string[] = [];

  mappedColumns$?: Observable<IMappedTableColumn<T>[]>;

  readonly StickyPosition = StickyPosition;

  ngOnInit(): void {
    this.data$?.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(data => {
      if(data && this.tableElementRef?.nativeElement) {
        this.tableElementRef.nativeElement.scrollIntoView();
      }
    });

    let columnsInput = this.columns;
    if(Array.isArray(columnsInput)) {
      columnsInput = of(columnsInput);
    }

    this.mappedColumns$ = columnsInput?.pipe(
      map(columns => columns.map(column => {
        let columnName: string;
        let headingDisplay: string;
        let tooltipText: string;
    
        if (typeof column === 'string' || typeof column === 'number') {
          columnName = column.toString();
          headingDisplay = columnName;
          tooltipText = this.getTooltipText(columnName);
        } else {
          columnName = column.name.toString();
          headingDisplay = column.headingDisplay ? (typeof column.headingDisplay !== 'string' ? column.headingDisplay(columnName) : column.headingDisplay) : columnName;
        }
        headingDisplay = this.camelCaseToTitleCase(columnName);
        tooltipText = this.getTooltipText(columnName);
        
        return {
          ...this.defaultColumnSettings,
          ...(typeof column === 'object' ? column : {}),
          name: columnName,
          headingDisplay,
          tooltip: tooltipText
        } as IMappedTableColumn<T>;
      }).sort((a, b) => b.overrideOrderPriority - a.overrideOrderPriority)),
      tap(columns => this.columnNames = columns.map(column => column.name))
    );
    
  }

  emitPagingEvent(event: PageEvent) {
    const { pageIndex, pageSize: recordsPerPage } = event;
    this.pagingEvent.emit({ pageIndex, recordsPerPage });
  }

  emitSortingEvent({ active, direction }: Sort) {
    this.sortingEvent.emit(direction ?
      {
        sortBy: active,
        sortOrder: direction === 'desc' ? SortOrder.Descending : SortOrder.Ascending,
      }
      :
      undefined)
  }

  private getTooltipText(columnName: string): string {
    switch (columnName) {
      case 'serviceTag':
        return 'Unique service tag for the locker';
      case 'label':
        return 'Label as marked on the locker (not unique)';
      case 'lastAuditDescription':
        return 'The time of the last reported event';
      case 'lastAuditTime':
        return 'The most recent event reported by the lock';
      case 'battery':
        return 'Estimated remaining battery life (%)';
      case 'lockOnline':
        return 'Displays red warning indicator if lock has not communicated within the last hour.';
      default:
        return '';
    }
  }

  private camelCaseToTitleCase(text: string): string {
    return text.replace(/([A-Z])/g, ' $1')
      .trim()
      .toLowerCase()
      .replace(/^\w/, (c) => c.toUpperCase());
  }

}
