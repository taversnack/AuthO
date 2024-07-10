import { inject } from '@angular/core';
import { CanMatchFn } from '@angular/router';
import { FeatureFlag } from '../feature-flag.type';
import { FeatureFlagService } from '../services/feature-flag.service';

export const canMatchWhenFeatureFlagIsSet = (flag: FeatureFlag): CanMatchFn =>
  () => inject(FeatureFlagService).hasFeatureFlagSet(flag);

export const canMatchWhenAllFeatureFlagsAreSet = (flags: FeatureFlag[]): CanMatchFn =>
  () => inject(FeatureFlagService).hasAllFeatureFlagsSet(flags);
