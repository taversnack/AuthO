import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, Subject, TimeoutError, catchError, filter, identity, map, mergeMap, of, retry, takeUntil, tap, timeout, timer, withLatestFrom } from 'rxjs';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { API_CONFIG, CreateSingle, GetSingle, IApiConfig, ICrudEndpoints, IJsonResponse, INestedResourceEndpoints, IPageFilterSort, IPagingResponse, IRequest, IRequestSettings, IRequestWithBody, NestedResourcePageFilterSortRequest, PageFilterSortRequest, PagingResponse, UpdateSingle, UrlProvider } from '../models/api';
import { EntityId } from '../models/common';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private readonly dialogService = inject(DialogService);
  private readonly http: HttpClient = inject(HttpClient);
  private config: IApiConfig = inject(API_CONFIG, { optional: true }) ?? {};

  setConfig(config: IApiConfig) {
    this.config = { ...config };
  }

  mergeConfig(config: IApiConfig) {
    this.config = { ...this.config, ...config };
  }

  generatePageFilterSortRequest<DTO>(url: UrlProvider, config?: IApiConfig): PageFilterSortRequest<DTO> {
    const isUsingCallbackUrl = typeof url === 'function';
    const cfg = config ? { ...this.config, ...config } : this.config;

    return ({ page, sort, filter, ...params }: IPageFilterSort = {}) => {
      const baseUrl = cfg.baseUrl ?? '';
      const endpoint = isUsingCallbackUrl ? url(baseUrl) : baseUrl + url;

      return !endpoint ? of(new PagingResponse<DTO>([])) : this.makeRequest(
        this.http.get<IPagingResponse<DTO>>(endpoint + this.useQueryParams(page, filter, sort, params), { observe: 'response' }), cfg)
        .pipe(map(({ success, data }) => success && data ? data : new PagingResponse([])));
    };
  }

  generatePageFilterSortRequestWithCustomReturn<DTO>(url: UrlProvider, config?: IApiConfig): (pageFilterSort?: IPageFilterSort) => Observable<DTO | null> {
    const isUsingCallbackUrl = typeof url === 'function';
    const cfg = config ? { ...this.config, ...config } : this.config;

    return ({ page, sort, filter, ...params }: IPageFilterSort = {}) => {
      const baseUrl = cfg.baseUrl ?? '';
      const endpoint = isUsingCallbackUrl ? url(baseUrl) : baseUrl + url;

      return !endpoint ? of(null) : this.makeRequest(
        this.http.get<DTO>(endpoint + this.useQueryParams(page, filter, sort, params), { observe: 'response' }), cfg)
        .pipe(map(({ success, data }) => success && data ? data : null));
    };
  }

  generateNestedResourcePageFilterSortRequest<DTO>(url: UrlProvider, nestedResourceFragment: string, config?: IApiConfig): NestedResourcePageFilterSortRequest<DTO> {
    const isUsingCallbackUrl = typeof url === 'function';
    const cfg = config ? { ...this.config, ...config } : this.config;

    return (entityId: EntityId, { page, sort, filter, ...params }: IPageFilterSort = {}) => {
      const baseUrl = cfg.baseUrl ?? '';
      const endpoint = isUsingCallbackUrl ? url(baseUrl) : baseUrl + url;

      return !endpoint ? of(new PagingResponse<DTO>([])) : this.makeRequest(
        this.http.get<IPagingResponse<DTO>>(`${endpoint}/${entityId}/${nestedResourceFragment}${this.useQueryParams(page, filter, sort, params)}`, { observe: 'response' }), cfg)
        .pipe(map(({ success, data }) => success && data ? data : new PagingResponse([])));
    };
  }

  generateNestedResource<DTO, CreateDTO>(baseUrl: UrlProvider, nestedResourceFragment: string, config?: IApiConfig): INestedResourceEndpoints<DTO, CreateDTO> {
    return {
      createMany: (entityId: EntityId, dtoList: CreateDTO[]) => {
        const [cfg, endpoint] = this.useConfigAndEndpoint(baseUrl, config);

        return !endpoint ? of(false) : this.makeRequest(this.http.post<DTO>(`${endpoint}/${entityId}/${nestedResourceFragment}`, dtoList, { observe: 'response' }), cfg)
        .pipe(map(({ success }) => success ?? false));
      },

      getMany: this.generateNestedResourcePageFilterSortRequest<DTO>(baseUrl, nestedResourceFragment, config)
    };
  }

  generateNestedUpdateMany<UpdateDTO>(url: UrlProvider, nestedResourceFragment: string, config?: IApiConfig): (entityId: EntityId, dtoList: UpdateDTO[]) => Observable<boolean> {
    return (entityId: EntityId, dtoList: UpdateDTO[]) => {
      const [cfg, endpoint] = this.useConfigAndEndpoint(url, config);

      return !endpoint ? of(false) : this.makeRequest(this.http.put(`${endpoint}/${entityId}/${nestedResourceFragment}`, dtoList, { observe: 'response' }), cfg)
      .pipe(map(({ success }) => success));
    };
  }

  generateNestedUpdateSingle<UpdateDTO>(url: UrlProvider, nestedResourceFragment: string, config?: IApiConfig): (entityId: EntityId, dto: UpdateDTO) => Observable<boolean> {
    return (entityId: EntityId, dto: UpdateDTO) => {
      const [cfg, endpoint] = this.useConfigAndEndpoint(url, config);

      return !endpoint ? of(false) : this.makeRequest(this.http.put(`${endpoint}/${entityId}/${nestedResourceFragment}`, dto, { observe: 'response' }), cfg)
      .pipe(map(({ success }) => success));
    };
  }

  generateNestedPartialUpdateSingle<UpdateDTO>(url: UrlProvider, nestedResourceFragment: string, config?: IApiConfig): (entityId: EntityId, dto: UpdateDTO) => Observable<boolean> {
    return (entityId: EntityId, dto: UpdateDTO) => {
      const [cfg, endpoint] = this.useConfigAndEndpoint(url, config);

      return !endpoint ? of(false) : this.makeRequest(this.http.patch(`${endpoint}/${entityId}/${nestedResourceFragment}`, dto, { observe: 'response' }), cfg)
      .pipe(map(({ success }) => success));
    };
  }

  generateNestedDelete(url: UrlProvider, nestedResourceFragment: string, config?: IApiConfig): (entityId: EntityId, nestedEntityId: EntityId) => Observable<boolean> {
    return (entityId: EntityId, nestedEntityId: EntityId) => {
      const [cfg, endpoint] = this.useConfigAndEndpoint(url, config);

      return !endpoint ? of(false) : this.makeRequest(this.http.delete(`${endpoint}/${entityId}/${nestedResourceFragment}/${nestedEntityId}`, { observe: 'response' }), cfg)
      .pipe(map(({ success }) => success));
    }
  }

  generateCreateSingle<DTO, CreateDTO>(url: UrlProvider, config?: IApiConfig): CreateSingle<DTO, CreateDTO> {
    return (dto: CreateDTO) => {
      const [cfg, endpoint] = this.useConfigAndEndpoint(url, config);

      return !endpoint ? of(null) : this.makeRequest(this.http.post<DTO>(endpoint, dto, { observe: 'response' }), cfg)
      .pipe(map(({ success, data }) => success && data ? data : null));
    };
  }

  generateGetSingle<DTO>(url: UrlProvider, config?: IApiConfig): GetSingle<DTO> {
    return (entityId: EntityId) => {
      const [cfg, endpoint] = this.useConfigAndEndpoint(url, config);

      return !endpoint ? of(null) : this.makeRequest(this.http.get<DTO>(`${endpoint}/${entityId}`, { observe: 'response' }), cfg)
      .pipe(map(({ success, data }) => success && data ? data : null));
    }
  }

  generateUpdateSingle<UpdateDTO>(url: UrlProvider, config?: IApiConfig): UpdateSingle<UpdateDTO> {
    return (entityId: EntityId, dto: UpdateDTO) => {
      const [cfg, endpoint] = this.useConfigAndEndpoint(url, config);

      return !endpoint ? of(false) : this.makeRequest(this.http.put(`${endpoint}/${entityId}`, dto, { observe: 'response' }), cfg)
      .pipe(map(({ success }) => success));
    };
  }

  generateCrudEndpoints<DTO, CreateDTO, UpdateDTO>(url: UrlProvider, config?: IApiConfig): ICrudEndpoints<DTO, CreateDTO, UpdateDTO> {
    return {
      getSingle: this.generateGetSingle<DTO>(url, config),

      getMany: this.generatePageFilterSortRequest<DTO>(url, config),

      createSingle: this.generateCreateSingle<DTO, CreateDTO>(url, config),

      updateSingle: this.generateUpdateSingle<UpdateDTO>(url, config),

      deleteSingle: (entityId: EntityId) => {
        const [cfg, endpoint] = this.useConfigAndEndpoint(url, config);

        return !endpoint ? of(false) : this.makeRequest(this.http.delete(`${endpoint}/${entityId}`, { observe: 'response' }), cfg)
        .pipe(map(({ success }) => success));
      }
    };
  }

  // TODO: Add the useConfigAndEndpointWithRequestSettings + IRequestSettings to all above generators and take in optional query parameters

  get<R = undefined>(url: UrlProvider, { config, requestSettings }: IRequest = {}): Observable<IJsonResponse<R> | null> {
    const [cfg, endpoint] = this.useConfigAndEndpointWithRequestSettings(url, config, requestSettings);
    return !endpoint ? of(null) : this.makeRequest(this.http.get<R>(endpoint, { observe: 'response' }), cfg);
  }

  post<T = undefined, R = undefined>(url: UrlProvider, { body, config, requestSettings }: IRequestWithBody<T> = {}): Observable<IJsonResponse<R> | null> {
    const [cfg, endpoint] = this.useConfigAndEndpointWithRequestSettings(url, config, requestSettings);
    return !endpoint ? of(null) : this.makeRequest(this.http.post<R>(endpoint, body, { observe: 'response' }), cfg);
  }

  patch<T = undefined, R = undefined>(url: UrlProvider, { body, config, requestSettings }: IRequestWithBody<T> = {}): Observable<IJsonResponse<R> | null> {
    const [cfg, endpoint] = this.useConfigAndEndpointWithRequestSettings(url, config, requestSettings);
    return !endpoint ? of(null) : this.makeRequest(this.http.patch<R>(endpoint, body, { observe: 'response' }), cfg);
  }

  put<T = undefined, R = undefined>(url: UrlProvider, { body, config, requestSettings }: IRequestWithBody<T> = {}): Observable<IJsonResponse<R> | null> {
    const [cfg, endpoint] = this.useConfigAndEndpointWithRequestSettings(url, config, requestSettings);
    return !endpoint ? of(null) : this.makeRequest(this.http.put<R>(endpoint, body, { observe: 'response' }), cfg);
  }

  delete<R = undefined>(url: UrlProvider, { config, requestSettings }: IRequest = {}): Observable<IJsonResponse<R> | null> {
    const [cfg, endpoint] = this.useConfigAndEndpointWithRequestSettings(url, config, requestSettings);
    return !endpoint ? of(null) : this.makeRequest(this.http.delete<R>(endpoint, { observe: 'response' }), cfg);
  }

  polling<T>(endpoint: Observable<T>, { interval }: { interval: number }): [stop: () => void, stream: Observable<T>] {
    const loading = new BehaviorSubject(false);
    // let loading = false;
    const stop = new Subject<void>();

    return [
      // () => stop.next(),
      () => stop.complete(),
      timer(0, interval).pipe(
        takeUntil(stop),
        withLatestFrom(loading),
        filter(([_, isLoading]) => isLoading),
        tap(() => loading.next(true)),
        mergeMap(() => endpoint.pipe(tap(() => loading.next(false))))
        // mergemap equivalent to:?
        // () => endpoint,
        // tap(() => loading.next(false))
      )
      // timer(0, interval).pipe(
      //   takeUntil(stop),
      //   filter(() => loading),
      //   tap(() => loading = true),
      //   mergeMap(() => endpoint.pipe(tap(() => loading = false))
      //   )
      // )
    ];
  }

  protected useConfigAndEndpoint(url: UrlProvider, config?: IApiConfig): [IApiConfig, string | undefined] {
    const cfg = config ? { ...this.config, ...config } : this.config;
    const isUsingCallbackUrl = typeof url === 'function';
    const baseUrl = cfg.baseUrl ?? '';
    const endpoint = isUsingCallbackUrl ? url(baseUrl) : baseUrl + url;

    return [cfg, endpoint];
  }

  protected useConfigAndEndpointWithRequestSettings(url: UrlProvider, config?: IApiConfig, requestSettings?: IRequestSettings): [IApiConfig, string | undefined] {
    const [cfg, endpoint] = this.useConfigAndEndpoint(url, config);
    if(!endpoint || !requestSettings) {
      return [cfg, endpoint]
    }

    const urlFragments = (requestSettings.urlFragments ?? []).join('/');
    const urlQueryParams = requestSettings.queryParameters ? this.useQueryParams(requestSettings.queryParameters) : '';

    const fullEndpoint = `${endpoint}${urlFragments ? '/' + urlFragments : ''}${urlQueryParams}`;
    return [cfg, fullEndpoint];
  }

  protected makeRequest<R = any>(request: Observable<HttpResponse<R>>, config: IApiConfig): Observable<IJsonResponse<R>> {
    return request.pipe(
      config.timeoutMilliseconds ? timeout(config.timeoutMilliseconds) : identity,
      // TODO: Move to below retry config
      map((response: HttpResponse<R>) => ({ success: true, data: response.body ?? undefined })),
      // retry with delays
      !config.retryCount ? identity : retry(
        {
          count: config.retryCount,
          delay: config.retryWaitMilliseconds ? (_err, retryCount) => timer(Math.pow(retryCount, config.retryWaitScale ?? 1) * config.retryWaitMilliseconds!) : undefined
        }
      ),
      !config.useGlobalErrorHandler ? identity : catchError(res => {
        const error = res?.error ?? res;

        if(error instanceof TimeoutError) {
          this.dialogService.error('Network timed out. Check your network settings or try again later', 'Timeout Error');
        }

        config.logger?.(error);
        throw error;
      })
    );
  }

  protected useQueryParams(...paramObjects: (object | undefined)[]): string {
    const arrayToQueryString = (array: any[], key: string): string => {
      if(!key) {
        return '';
      }
      const encodedKey = encodeURIComponent(key);
      var stringsToAppend: string[] = [];

      for (const value of array) {
        if (value !== null && value !== undefined) {
          if (typeof value === 'object') {
            if (Array.isArray(value)) {
              stringsToAppend.push(arrayToQueryString(value, encodedKey));
            } else {
              stringsToAppend.push(objectToQueryString(value, encodedKey));
            }
          } else if(typeof(value) !== 'string' || value) {
            stringsToAppend.push(`${encodedKey}=${encodeURIComponent(value)}`)
          }
        }
      }
      return stringsToAppend.filter(x => x).join('&');
    }

    // simple join
    const getPrefixedKey = (prefix: string, key: string) => prefix + key;

    // snake_case
    // const getPrefixedKey = (prefix: string, key: string) => prefix ? `${prefix}_${key}` : key;

    // kebab-case
    // const getPrefixedKey = (prefix: string, key: string) => prefix ? `${prefix}-${key}` : key;

    // camelCase
    // const getPrefixedKey = (prefix: string, key: string) =>
    //   prefix ? `${prefix}${[key[0].toUpperCase(), ...key.slice(1)].join('')}` : key;

    const objectToQueryString = (object?: object, prefix: string = ''): string => {
      if (object === null || object === undefined) {
        return '';
      }
      if (object instanceof Date) {
        if (!prefix) {
          throw new Error('Cannot use a date without a key as a top level query parameter');
        }
        return new URLSearchParams({ [prefix]: encodeURIComponent(object.toISOString()) }).toString();
      }
      let queryObject: Record<string, string> = {};
      var stringsToAppend: string[] = [];
      for (const [key, value] of Object.entries(object)) {
        if (value !== null && value !== undefined) {
          if (typeof value === 'object') {
            if (Array.isArray(value)) {
              stringsToAppend.push(arrayToQueryString(value, getPrefixedKey(prefix, key)));
            } else {
              stringsToAppend.push(objectToQueryString(value, getPrefixedKey(prefix, key)));
            }
          } else {
            const prefixedKey = getPrefixedKey(prefix, key);
            if(prefixedKey && (typeof(value) !== 'string' || value)) {
              queryObject[encodeURIComponent(prefixedKey)] = encodeURIComponent(value);
            }
          }
        }
      }
      stringsToAppend.unshift(new URLSearchParams(queryObject).toString());
      return stringsToAppend.filter(x => x).join('&');
    }
    const params = paramObjects.map(x => objectToQueryString(x)).filter(x => x).join('&');
    return params ? '?' + params : '';
  }
}
