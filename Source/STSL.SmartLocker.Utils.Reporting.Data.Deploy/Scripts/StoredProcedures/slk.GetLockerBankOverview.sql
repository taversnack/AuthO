DROP PROCEDURE IF EXISTS slk.GetLockerBankOverview;
GO

CREATE PROCEDURE slk.GetLockerBankOverview
    @tenantId uniqueidentifier
    ,@lockerBankId uniqueidentifier
AS
BEGIN
    DECLARE @results TABLE
    (
        VacantLockers int NOT NULL,
        AverageBatteryVoltage float NOT NULL,
        LowestBatteryVoltage float NOT NULL,
        HighestBatteryVoltage float NOT NULL
    );
END

GO