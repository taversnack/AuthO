DROP PROCEDURE IF EXISTS slkmart.GetWithCreateCardCredentialId;
GO

CREATE PROCEDURE slkmart.GetWithCreateCardCredentialId
    @cardCredential char(16)
    ,@cardCredentialId int OUTPUT
AS
BEGIN
    IF @cardCredential IS NULL
    BEGIN
        -- return null if null passed in
        RETURN;
    END

    SELECT
        @cardCredentialId = CardCredentialId
    FROM
        slkmart.CardCredentials
    WHERE
        CardCredential = @cardCredential;

    IF @cardCredentialId IS NULL
    BEGIN
        INSERT INTO CardCredentials
        (
            CardCredential
        )
        VALUES
        (
            @cardCredential
        );
        SET @cardCredentialId = SCOPE_IDENTITY();
    END
END
GO