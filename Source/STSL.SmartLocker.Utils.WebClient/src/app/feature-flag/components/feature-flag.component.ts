import {
  Component,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  TemplateRef,
  inject
} from '@angular/core';
import { FeatureFlag } from '../feature-flag.type';
import { FeatureFlagService } from '../services/feature-flag.service';

@Component({
  selector: 'app-feature-flag',
  templateUrl: './feature-flag.component.html',
  host: { 'class': 'contents' },
})
export class FeatureFlagComponent implements OnDestroy, OnChanges {

  private readonly featureFlagService = inject(FeatureFlagService);

  @Input() flag?: FeatureFlag;
  @Input() flags: FeatureFlag[] = [];
  @Input('else') elseTemplate: TemplateRef<any> | null = null;

  private allFlags: FeatureFlag[] = [];

  areFlagsSetAndTrue = false;
  hasTemplateAndFlagNotSet = false;

  private featureFlagsChangedSubscription = this.featureFlagService.featureFlags$.subscribe(() => this.checkFlagsStatus());

  ngOnDestroy(): void {
    this.featureFlagsChangedSubscription.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges) {
    const flag = changes['flag']?.currentValue;
    const flags = changes['flags']?.currentValue ?? [];

    this.allFlags = flag === undefined ? flags : [...new Set([...flags, flag])];

    this.checkFlagsStatus();
  }

  private checkFlagsStatus() {
    this.areFlagsSetAndTrue = this.featureFlagService.hasAllFeatureFlagsSet(this.allFlags);
    this.hasTemplateAndFlagNotSet = !this.areFlagsSetAndTrue && this.elseTemplate !== null;
  }
}
