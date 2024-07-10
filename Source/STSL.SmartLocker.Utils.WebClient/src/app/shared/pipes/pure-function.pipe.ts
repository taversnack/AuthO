import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'pureFunction',
  pure: true,
  standalone: true
})
export class PureFunctionPipe implements PipeTransform {
  transform(value: (...args: any[]) => string, ...args: any[]): string {
    return value(args);
  }
}
