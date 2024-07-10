-- Drop the existing ProperCase function if it exists
IF OBJECT_ID('dbo.ProperCase', 'FN') IS NOT NULL
  DROP FUNCTION dbo.ProperCase;
GO

CREATE FUNCTION dbo.ProperCase(@inputString NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS
BEGIN
  DECLARE @outputString NVARCHAR(MAX) = '';
  DECLARE @nextCapital BIT = 1; -- Flag for next character capitalization
  DECLARE @previousChar NVARCHAR(1) = ''; -- Track the previous character
  DECLARE @secondPreviousChar NVARCHAR(1) = ''; -- Track the character before the previous

  IF LEN(@inputString) > 0
  BEGIN
    DECLARE @i INT = 1;

    WHILE @i <= LEN(@inputString)
    BEGIN
      DECLARE @currentChar NVARCHAR(1) = SUBSTRING(@inputString, @i, 1);

      IF @nextCapital = 1
      BEGIN
        -- Capitalize current letter
        SET @outputString += UPPER(@currentChar);
        SET @nextCapital = 0;
      END
      ELSE
      BEGIN
        -- Keep letter as is
        SET @outputString += LOWER(@currentChar);
      END

      -- Check for "Mc" and "Mac" patterns and set capitalization for the next character
      IF (@previousChar IN ('M', 'm') AND @currentChar = 'c') OR
         (@secondPreviousChar IN ('M', 'm') AND @previousChar = 'a' AND @currentChar = 'c')
      BEGIN
        SET @nextCapital = 1;
      END

      -- Determine if the next letter should be capitalized based on separators
      IF @currentChar IN (' ', '-', '(', ')', '''')
      BEGIN
        SET @nextCapital = 1;
      END

      -- Update the character trackers
      SET @secondPreviousChar = @previousChar;
      SET @previousChar = @currentChar;
      SET @i = @i + 1;
    END
  END

  RETURN @outputString;
END
GO



-- Update the CardHolders table with ProperCase Change CardHolder Table with you table and column names
--UPDATE [slk].[CardHolders]
--SET 
--    [FirstName] = dbo.ProperCase([FirstName]),
--    [LastName] = dbo.ProperCase([LastName]);