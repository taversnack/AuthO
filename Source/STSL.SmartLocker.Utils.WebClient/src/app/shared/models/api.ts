import { InjectionToken } from "@angular/core";
import { Observable } from "rxjs";
import { EntityId } from "./common";
import { ErrorType } from "./error";
import { SearchFilters } from './search-filters';

export type CreateSingle<DTO, CreateDTO> = (dto: CreateDTO) => Observable<DTO | null>;
export type GetSingle<DTO> = (entityId: EntityId) => Observable<DTO | null>;
export type UpdateSingle<UpdateDTO> = (entityId: EntityId, dto: UpdateDTO) => Observable<boolean>;

export interface ICrudEndpoints<DTO, CreateDTO, UpdateDTO> {
  readonly getSingle: GetSingle<DTO>;
  readonly getMany: PageFilterSortRequest<DTO>;
  readonly createSingle: CreateSingle<DTO, CreateDTO>;
  readonly updateSingle: UpdateSingle<UpdateDTO>;
  readonly deleteSingle: (entityId: EntityId) => Observable<boolean>;
}

export interface IPageFilterSort {
  page?: IPagingRequest;
  filter?: IFilteredRequest;
  sort?: ISortedRequest;
}

// TODO: Make query parameters 'type safeable', should be explicit that you can pass additional query params
// when making a get request, not just the page, filter, & sort params.
// export interface IQueryParams<T extends {} = any> {
//   params?: T;
// }

// export type PageFilterSortRequest<DTO> = (pageFilterSort?: IPageFilterSort & IQueryParams) => Observable<IPagingResponse<DTO>>;
// export type NestedResourcePageFilterSortRequest<DTO> = (entityId: EntityId, pageFilterSort?: IPageFilterSort & IQueryParams) => Observable<IPagingResponse<DTO>>;

export type PageFilterSortRequest<DTO> = (pageFilterSort?: IPageFilterSort) => Observable<IPagingResponse<DTO>>;
export type NestedResourcePageFilterSortRequest<DTO> = (entityId: EntityId, pageFilterSort?: IPageFilterSort) => Observable<IPagingResponse<DTO>>;

export interface INestedResourceEndpoints<DTO, CreateDTO> {
  readonly createMany: (entityId: EntityId, dtoList: CreateDTO[]) => Observable<boolean>;
  readonly getMany: NestedResourcePageFilterSortRequest<DTO>
}

export interface IRequest {
  config?: IApiConfig;
  requestSettings?: IRequestSettings;
}

export interface IRequestWithBody<T> extends IRequest {
  body?: T;
}

export interface IRequestSettings {
  urlFragments?: (string | number)[];
  queryParameters?: (object | undefined)[];
}

export interface IJsonResponse<T = null> {
  success: boolean;
  error?: ErrorType;
  data?: T;
}

// NOTE: [0] Minor network optimization to avoid strings
// export const enum SortOrder {
//   Ascending,
//   Descending,
// };

export const enum SortOrder {
  Ascending = 'Ascending',
  Descending = 'Descending',
};

export interface IPagingResponse<T> {
  pageIndex: number; // 0 based
  recordsPerPage: number;
  recordCount: number;
  totalRecords: number;
  totalPages: number;
  results: T[];
}

export interface IPagingRequest {
  pageIndex: number;
  recordsPerPage: number;
}

export interface IFilteredRequest {
  filterProperties?: string[];
  filterValue?: string;
  startsWith?: boolean;
}

export interface ISortedRequest {
  sortBy?: string;
  sortOrder: SortOrder;
}

export interface IApiConfig {
  logger?: (error: ErrorType) => void;
  baseUrl?: string;
  useGlobalErrorHandler?: boolean;
  timeoutMilliseconds?: number;
  retryCount?: number;
  retryWaitMilliseconds?: number;
  retryWaitScale?: number;
}

export type UrlProvider = string | ((baseUrl: string) => string | undefined);

export const API_CONFIG = new InjectionToken<IApiConfig>('Default API configuration');

export class PagingResponse<T> implements IPagingResponse<T> {
  constructor(results: T[] = []) {
    this.totalRecords = results.length;
    this.results = results;
  }
  pageIndex: number = 0;
  recordsPerPage: number = 0;
  recordCount: number = 0;
  totalRecords: number = 0;
  totalPages: number = 0;
  results: T[] = [];
}

export class PagingRequest implements Required<IPagingRequest> {
  // pageIndex: number = 0;
  // recordsPerPage: number = 50;
  constructor(public recordsPerPage = 100, public pageIndex = 0) {}
}

export class FilteredRequest implements Required<IFilteredRequest> {
  constructor(public filterProperties: string[] = [], public filterValue: string = '', public startsWith: boolean = false) {}
}

export class SortedRequest implements Required<ISortedRequest> {
  sortBy: string = '';
  sortOrder: SortOrder = SortOrder.Ascending;
}

export class PageFilterSort implements Required<IPageFilterSort> {
  page: IPagingRequest = new PagingRequest();
  filter: IFilteredRequest = new FilteredRequest();
  sort: ISortedRequest = new SortedRequest();

  constructor(settings: IPageFilterSort = {}) {
    this.page = settings.page ?? new PagingRequest();
    this.filter = settings.filter ?? new FilteredRequest();
    this.sort = settings.sort ?? new SortedRequest();
  }

  static usingFilter(properties: SearchFilters, filterValue: string = ''): PageFilterSort {
    const filterProperties = properties.map(x => typeof x === 'string' ? x : x.name);
    return new PageFilterSort({ filter: { filterValue, filterProperties } });
  }

  resetQuery(query: string = '') {
    this.filter.filterValue = query;
    this.page.pageIndex = 0;
  }

  resetQueryAndFilter(query: string = '', properties: string[] = [] )
  {
    this.filter.filterValue = query;
    this.filter.filterProperties = properties
    this.page.pageIndex = 0;
  }
}
