export interface IFilter {
  name: string;
  display?: string;
  selected?: boolean;
}

export type SearchFilters = (string | IFilter)[];
