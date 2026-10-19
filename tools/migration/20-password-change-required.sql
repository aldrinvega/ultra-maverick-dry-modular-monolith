SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @passwordSentinel nvarchar(200) =
    'PBKDF2;SHA256;210000;AAAAAAAAAAAAAAAAAAAAAA==;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=';

BEGIN TRANSACTION;

UPDATE [Identity].[Users]
SET [MustChangePassword] = 1
WHERE [PasswordHash] = @passwordSentinel
  AND [MustChangePassword] = 0;

PRINT CONCAT('Password change required for ', @@ROWCOUNT, ' migrated user(s).');

COMMIT TRANSACTION;
