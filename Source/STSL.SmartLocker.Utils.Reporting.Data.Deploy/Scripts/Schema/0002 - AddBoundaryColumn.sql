IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE [name] = N'LastBoundaryAddress'AND Object_ID = Object_ID(N'slkmart.Locks'))
BEGIN
    ALTER TABLE slkmart.Locks ADD LastBoundaryAddress int;
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE [name] = N'BoundaryAddress'AND Object_ID = Object_ID(N'slkmart.LockAudits'))
BEGIN
    ALTER TABLE slkmart.LockAudits ADD BoundaryAddress int;
END

GO
