import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, distinctUntilChanged, map } from 'rxjs';
import { arrayWithRemovedIndex, includesAll } from 'src/app/shared/lib/utilities';
import { environment } from 'src/environments/environment';
import { FeatureFlag } from '../feature-flag.type';
import { EnvironmentWithFeatureFlags } from '../helpers';

@Injectable({
  providedIn: 'root'
})
export class FeatureFlagService {

  private readonly featureFlagsSubject = new BehaviorSubject<FeatureFlag[]>(
    [...new Set((environment as EnvironmentWithFeatureFlags<typeof environment>)?.featureFlags ?? [])]
  );

  readonly featureFlags$ = this.featureFlagsSubject.asObservable();

  checkFlagsOnChange(checkFlags: FeatureFlag[]): Observable<boolean> {
    return this.featureFlags$.pipe(map(currentFlags => includesAll(currentFlags, checkFlags)), distinctUntilChanged());
  }

  observeFeatureFlagChanges(flag: FeatureFlag): Observable<boolean> {
    return this.featureFlags$.pipe(map(flags => flags.includes(flag)), distinctUntilChanged());
  }

  hasFeatureFlagSet(flag: FeatureFlag): boolean {
    this.featureFlagsSubject.getValue().includes(flag);
    return this.featureFlagsSubject.getValue().includes(flag);
  }

  hasAllFeatureFlagsSet(flags: FeatureFlag[]): boolean {
    return includesAll(this.featureFlagsSubject.getValue(), flags);
  }

  setFeatureFlag(flag: FeatureFlag, state: boolean = true) {
    const allFlags = this.featureFlagsSubject.getValue();
    const flagIndex = allFlags.findIndex(x => x === flag);

    if(state && flagIndex === -1) {
      this.featureFlagsSubject.next([...allFlags, flag]);
      return;
    }

    if(!state && flagIndex >= 0) {
      const newFlags = arrayWithRemovedIndex(allFlags, flagIndex);
      this.featureFlagsSubject.next(newFlags);
      return;
    }
  }

  toggleFeatureFlag(flag: FeatureFlag) {
    const allFlags = this.featureFlagsSubject.getValue();
    const flagIndex = allFlags.findIndex(x => x === flag);

    if(flagIndex === -1) {
      this.featureFlagsSubject.next([...allFlags, flag]);
    } else {
      const newFlags = arrayWithRemovedIndex(allFlags, flagIndex);
      this.featureFlagsSubject.next(newFlags);
    }
  }
}
