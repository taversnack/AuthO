DROP PROCEDURE IF EXISTS slk.ProcessMSIData;
GO

CREATE PROCEDURE slk.ProcessMSIData
    @tenantId UNIQUEIDENTIFIER,
    @json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

         -- Table to store MSI
		DECLARE @MSITable TABLE (
			[ObjectID] NVARCHAR(256),
			FirstName NVARCHAR(256),
			LastName NVARCHAR(256),
			StaffEmail NVARCHAR(256),
			TerminationDate DATE,
			IsTerminated BIT,
			IsDuplicateEmail BIT DEFAULT 0
		);

        -- Table to output any inserted, updated or duplicate records
		DECLARE @OutputRecords TABLE (
		    [Action] NVARCHAR(50), -- To store the action type (INSERT, UPDATE, DUPLICATE, NOT INSERTED)
			ActionNotes NVARCHAR(256), -- To store additional info about the record
			FirstName NVARCHAR(256),
			LastName NVARCHAR(256),
			Email NVARCHAR(256),
			ObjectID NVARCHAR(256),
			NewObjectID NVARCHAR(256),
			IsTerminated BIT,
			TerminationDate DATE,
			IsDuplicateEmail BIT DEFAULT 0
		);

		-- Parse JSON into CTE
		WITH ProcessedData AS (
			SELECT 
				[ObjectID], 
				FirstName, 
				LastName, 
				NULLIF(StaffEmail, '') AS StaffEmail,
				NULLIF(TerminationDate, '') AS TerminationDate,
				CASE 
					WHEN NULLIF(TerminationDate, '') <= GETDATE() 
					THEN 1
					ELSE 0
				END AS IsTerminated,
				COUNT(CASE WHEN NULLIF(StaffEmail, '') IS NOT NULL THEN 1 END) 
					OVER (PARTITION BY NULLIF(StaffEmail, '')) 
				AS EmailCount -- Count email occurences to identify duplicates
			FROM OPENJSON(@json)
			WITH
			(
				[ObjectID] NVARCHAR(256),-- '$."Object ID"',
				FirstName NVARCHAR(256),-- '$."First Name"',
				LastName NVARCHAR(256),-- '$."Last Name"',
				StaffEmail NVARCHAR(256),-- '$."Staff Email"',
				TerminationDate DATE --'$."Termination Date"'
			)
		)
		INSERT INTO @MSITable
		SELECT 
			[ObjectID],
			FirstName,
			LastName,
			StaffEmail,
			TerminationDate,
			IsTerminated,
			CASE 
				WHEN EmailCount > 1
				THEN 1
				ELSE 0
			END AS IsDuplicatedEmail
		FROM ProcessedData;

		-- Insert duplicate records into @OutputRecords
		INSERT INTO @OutputRecords(
			[Action],
			ActionNotes,
			FirstName,
			LastName,
			Email,
			ObjectID,
			IsTerminated,
			TerminationDate,
			IsDuplicateEmail)
		SELECT 
			'DUPLICATE EMAIL',
			'Email address must be unique',
			FirstName,
			LastName,
			StaffEmail,
			[ObjectID],
			IsTerminated,
			TerminationDate,
			IsDuplicateEmail
		FROM @MSITable
		WHERE IsDuplicateEmail = 1;

        BEGIN TRANSACTION;

            -- Merge MSI into Card Holders Table
            MERGE INTO slk.CardHolders AS Target
                USING (SELECT * FROM @MSITable WHERE IsDuplicateEmail = 0) AS Source
            ON Target.[UniqueIdentifier] = Source.[ObjectID]
				AND Target.TenantId = @tenantId
            WHEN MATCHED AND 
                (
                    Target.FirstName <> Source.FirstName OR 
                    Target.LastName <> Source.LastName OR 
                    Target.Email <> Source.StaffEmail OR
                    Target.IsTerminated <> Source.IsTerminated
                )
            THEN
                -- Update if ObjectID exists and there are differences
                UPDATE SET 
                    Target.FirstName = Source.FirstName,
                    Target.LastName = Source.LastName,
                    Target.Email = Source.StaffEmail,
                    Target.IsTerminated = Source.IsTerminated,
                    Target.TerminationDate = Source.TerminationDate
            WHEN NOT MATCHED BY Target
                AND Source.IsTerminated = 0 -- Ignore terminated card holders
				-- Additional check for the email - If the ObjectID of an already existing email has changed it would attempt an INSERT
				AND NOT EXISTS (SELECT 1 FROM slk.CardHolders WHERE Email = Source.StaffEmail AND TenantId = @tenantId)
            THEN
                -- Insert if ObjectID does not exist
                INSERT (CardHolderId, FirstName, LastName, Email, [UniqueIdentifier], IsVerified, TenantId)
                VALUES (NEWID(), Source.FirstName, Source.LastName, Source.StaffEmail, Source.[ObjectID], 0, @tenantId)
            OUTPUT 
                $action,
				NULL,
				INSERTED.FirstName,
				INSERTED.LastName,
				INSERTED.Email,
				INSERTED.[UniqueIdentifier],
				NULL,
				INSERTED.IsTerminated,
				INSERTED.TerminationDate,
				0
            INTO @OutputRecords;

        COMMIT TRANSACTION;

		-- Insert non-inserted (due to existing email) records into @OutputRecords
		INSERT INTO @OutputRecords(
			[Action],
			ActionNotes,
			FirstName,
			LastName,
			Email,
			ObjectID,
			NewObjectID,
			IsTerminated,
			TerminationDate,
			IsDuplicateEmail)
		SELECT 
			'NOT INSERTED',
			'ObjectID for user has been changed. Please update record with new ObjectID',
			Source.FirstName,
			Source.LastName,
			Source.StaffEmail,
			Target.[UniqueIdentifier],
			Source.[ObjectID],
			Source.IsTerminated,
			Source.TerminationDate,
			0
		FROM @MSITable AS Source
			INNER JOIN slk.CardHolders AS Target ON Source.StaffEmail = Target.Email AND Target.TenantId = @tenantId
		WHERE Source.IsDuplicateEmail = 0
			AND Source.[ObjectID] <> Target.[UniqueIdentifier];

		SELECT 
			* 
		FROM @OutputRecords
		WHERE IsDuplicateEmail = 1 
			OR IsTerminated = 1
			OR [Action] = 'NOT INSERTED';

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        DECLARE @ErrorMessage NVARCHAR(4000);  
        DECLARE @ErrorSeverity INT;  
        DECLARE @ErrorState INT;  

        SELECT   
           @ErrorMessage = ERROR_MESSAGE(),  
           @ErrorSeverity = ERROR_SEVERITY(),  
           @ErrorState = ERROR_STATE();  

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO