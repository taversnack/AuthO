import { InjectionToken, TemplateRef } from "@angular/core";
import { camelCaseToSpacedPascalCase } from "../lib/utilities";

export enum StickyPosition {
  None,
  Start,
  End,
}

export enum HeadingPosition {
  Top,
  Bottom,
}

export interface ITableSettings {
  useStickyHeader?: boolean;
  useStickyFooter?: boolean;
  useHorizontalStripes?: boolean;
  useVerticalStripes?: boolean;
  useHoverHighlight?: boolean;
  a11yLabel: string;
  // EM units
  minimumHeight?: number;
  minimumColumnWidthEm?: number;
  disablePaging?: boolean;
}

export const defaultTableSettings: ITableSettings = {
  useStickyHeader: true,
  useStickyFooter: true,
  useHorizontalStripes: true,
  useHoverHighlight: true,
  a11yLabel: 'Table elements',
  minimumHeight: 16,
  disablePaging: false,
}

export type BaseColumnKeyType = string | number;
export type ColumnKeyType<T> = BaseColumnKeyType & keyof T;

export interface ITableColumnSettings {
  headingDisplay?: string | ((name: BaseColumnKeyType) => string);
  headingPosition?: HeadingPosition;
  stickyPosition?: StickyPosition;
  sortable?: boolean;
  overrideOrderPriority?: number;
}

export interface ITemplateTableColumn<T> extends ITableColumnSettings {
  name: BaseColumnKeyType;
  template?: TemplateRef<T>;
}

export interface IGetterTableColumn<T> extends ITableColumnSettings {
  name: BaseColumnKeyType;
  getValue?: (rowValue: T) => string | number | boolean | undefined;
}

const isGetterTableColumn = (x: any): x is IGetterTableColumn<any> => Object.hasOwn(x, 'getValue');
const isTemplateTableColumn = (x: any): x is ITemplateTableColumn<any> => Object.hasOwn(x, 'template');

type TemplateOrGetterTableColumn<T> = ITemplateTableColumn<T> | IGetterTableColumn<T>;

export type TableColumn<T> = ColumnKeyType<T> | TemplateOrGetterTableColumn<T>;

export const DATA_TABLE_COLUMN_SETTINGS = new InjectionToken<Required<ITableColumnSettings>>('Default settings for all data table columns');
export const defaultTableColumnSettings: Required<ITableColumnSettings> = {
  headingDisplay: name => typeof name === 'number' ? name.toString() : camelCaseToSpacedPascalCase(name),
  headingPosition: HeadingPosition.Top,
  stickyPosition: StickyPosition.None,
  sortable: false,
  overrideOrderPriority: 0,
};

export interface ITablePaginationSettings {
  defaultPageSize: number;
  pageSizeOptions: number[];
  firstAndLastButtonsEnabled: boolean;
}

export const defaultTablePaginationSettings: ITablePaginationSettings = {
  defaultPageSize: 100,
  pageSizeOptions: [100, 150, 200, 250],
  firstAndLastButtonsEnabled: true,
};
