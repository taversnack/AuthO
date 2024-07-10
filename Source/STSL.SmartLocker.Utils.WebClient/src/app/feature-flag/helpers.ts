import { FeatureFlag } from "./feature-flag.type";

export interface IHasFeatureFlags {
  featureFlags: FeatureFlag[]
}

export type EnvironmentWithFeatureFlags<T extends object = any> = IHasFeatureFlags & T;
