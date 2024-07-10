export const enum FeatureFlag {
  DisableSuperUserRole          = 1 << 0,
  DisableInstallerRole          = 1 << 1,
  DisableLockerBankAdminRole    = 1 << 2,
  DisableTenantAdminRole        = 1 << 3,

  ShowAllLockerStatusFields     = 1 << 16,
  ShowAllLockerAuditFields      = 1 << 17,
  DisableShiftLockerBanks         = 1 << 18,
}
