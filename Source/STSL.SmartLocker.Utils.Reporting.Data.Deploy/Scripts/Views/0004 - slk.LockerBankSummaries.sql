DROP VIEW IF EXISTS slk.LockerBankSummaries;
GO

CREATE VIEW slk.LockerBankSummaries
AS
SELECT
    lb.LockerBankId,
    lb.[Name],
    lb.[Description],
    lb.Behaviour,
    COUNT(lo.LockerId) AS TotalLockers,
    COUNT(CASE WHEN lws.AssignedTo IS NULL THEN 1 ELSE NULL END) AS VacantLockers,
    CAST(MAX(CASE 
        WHEN lws.LastAuditTime <= DATEADD(MONTH, -1, GETDATE()) OR lws.BatteryVoltage < 3.4 THEN 1
        ELSE 0
    END) AS BIT) AS HasWarnings /* TODO: Remove hard-coding and improve warning capabilities */
FROM 
    slk.LockerBanks lb 
INNER JOIN 
    slk.Lockers lo ON lo.LockerBankId = lb.LockerBankId
LEFT JOIN 
    slk.LockersWithStatus lws ON lws.LockerId = lo.LockerId
GROUP BY
    lb.LockerBankId, lb.[Name], lb.[Description], lb.Behaviour
GO