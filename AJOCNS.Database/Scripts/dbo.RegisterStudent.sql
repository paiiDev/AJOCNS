/*
   Atomic Student registration. PasswordHash must be created in the application
   with ASP.NET Core PasswordHasher; never accept a plain-text password here.
*/
CREATE OR ALTER PROCEDURE dbo.RegisterStudent
    @Srn            nvarchar(100),
    @Email          nvarchar(255),
    @PasswordHash   nvarchar(255),
    @MajorId        int,
    @Phone          nvarchar(30) = NULL,
    @FatherName     nvarchar(255) = NULL,
    @Address        nvarchar(500) = NULL,
    @UserId         int OUTPUT,
    @StudentId      int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @GRecordId int;
    SET @Email = LOWER(LTRIM(RTRIM(@Email)));
    SET @Srn = LTRIM(RTRIM(@Srn));

    BEGIN TRANSACTION;
    BEGIN TRY
        SELECT @GRecordId = GRecord_Id
        FROM dbo.Graduation_Records WITH (UPDLOCK, HOLDLOCK)
        WHERE GRN = @Srn AND Acc_Status = 'Approved';

        IF @GRecordId IS NULL
            THROW 50001, 'SRN was not found or is not approved for registration.', 1;

        IF EXISTS (SELECT 1 FROM dbo.Students WITH (UPDLOCK, HOLDLOCK) WHERE GRecord_Id = @GRecordId)
           OR EXISTS (SELECT 1 FROM dbo.Mentors WITH (UPDLOCK, HOLDLOCK) WHERE GRecord_Id = @GRecordId)
            THROW 50002, 'This graduation record has already been claimed.', 1;

        IF EXISTS (SELECT 1 FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE Email = @Email)
            THROW 50003, 'This email address is already registered.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Majors WHERE Major_ID = @MajorId)
            THROW 50004, 'The selected major does not exist.', 1;

        INSERT dbo.Users (Email, PasswordHash, Role, Status, isFirstLogin, isDeleted)
        VALUES (@Email, @PasswordHash, 'Student', 'Active', 0, 0);
        SET @UserId = CONVERT(int, SCOPE_IDENTITY());

        INSERT dbo.Students (User_ID, Name, Phone, FatherName, Address, GRecord_Id, Major_ID)
        SELECT @UserId, OfficialName, @Phone, @FatherName, @Address, GRecord_Id, @MajorId
        FROM dbo.Graduation_Records
        WHERE GRecord_Id = @GRecordId;
        SET @StudentId = CONVERT(int, SCOPE_IDENTITY());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
