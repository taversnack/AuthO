import { CdkTableDataSourceInput } from '@angular/cdk/table';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTable } from '@angular/material/table';
import { Observable, map, of, tap } from 'rxjs';
import { MaterialModule } from 'src/app/material/material.module';
import { DATA_TABLE_COLUMN_SETTINGS, ITableColumnSettings, StickyPosition, TableColumn, defaultTableColumnSettings } from '../../models/data-table';

interface IMappedTableColumn<C> extends Required<ITableColumnSettings> {
  headingDisplay: string;
  name: string;
  template?: TemplateRef<C>;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, MaterialModule],
  templateUrl: './data-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'class': 'contents' }
})
export class DataTableComponent<T> implements OnInit {
  private readonly defaultColumnOptions: Required<ITableColumnSettings> = inject(DATA_TABLE_COLUMN_SETTINGS, { optional: true }) ?? defaultTableColumnSettings;

  @Input() data?: CdkTableDataSourceInput<T>;
  @Input() columns?: TableColumn<T>[] | Observable<TableColumn<T>[]>;
  @Input() useStickyHeader: boolean = true;
  @Input() useStickyFooter: boolean = true;
  @Input() a11yLabel: string = 'Table elements';
  @Input() minimumColumnWidth = 12;
  @Input() minimumTableHeight = 16;

  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;
  @ViewChild(MatTable) table?: MatTable<T>;

  columnNames: string[] = [];

  mappedColumns$?: Observable<IMappedTableColumn<T>[]>;

  readonly StickyPosition = StickyPosition;

  ngOnInit(): void {
    let columnsInput = this.columns;
    if(Array.isArray(columnsInput)) {
      columnsInput = of(columnsInput);
    }

    this.mappedColumns$ = columnsInput?.pipe(
      map(columns => columns.map(column => {
          let mappedColumn = (typeof column === 'string' || typeof column === 'number') ? { ...this.defaultColumnOptions, name: column.toString() } : { ...this.defaultColumnOptions, ...column };

          if(typeof mappedColumn.headingDisplay !== 'string') {
            mappedColumn.headingDisplay = mappedColumn.headingDisplay(mappedColumn.name);
          }

          return mappedColumn as IMappedTableColumn<T>;
      })),
      tap(columns => this.columnNames = columns.map(column => column.name))
    );
  }
}
