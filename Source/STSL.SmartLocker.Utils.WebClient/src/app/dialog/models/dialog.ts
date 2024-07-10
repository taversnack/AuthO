import { ComponentType } from "@angular/cdk/portal";

export interface IDialog<T, D = null> {
  title?: string;
  data: D;
  component: ComponentType<T>;
}
