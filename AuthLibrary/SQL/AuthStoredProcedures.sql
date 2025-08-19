-- ===== Auth SPs (simple + safe) =====

-- 1) sp_CreateUser
IF OBJECT_ID(N'dbo.sp_CreateUser','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CreateUser;
GO
CREATE PROCEDURE dbo.sp_CreateUser
    @Username NVARCHAR(150),
    @Password NVARCHAR(255),            -- hashed by app
    @Email NVARCHAR(254),
    @FirstName NVARCHAR(150),
    @LastName NVARCHAR(150),
    @IsStaff BIT,
    @IsSuperuser BIT,
    @IsActive BIT,
    @DateJoined DATETIMEOFFSET,
    @LastLogin DATETIMEOFFSET = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO dbo.auth_user (
            username, password, email, first_name, last_name,
            is_staff, is_superuser, is_active, date_joined, last_login
        )
        VALUES (
            @Username, @Password, @Email, @FirstName, @LastName,
            @IsStaff, @IsSuperuser, @IsActive, @DateJoined, @LastLogin
        );
        DECLARE @UserId INT = CAST(SCOPE_IDENTITY() AS INT);
        COMMIT TRAN;
        SELECT @UserId AS UserId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO

-- 2) sp_ChangePassword
IF OBJECT_ID(N'dbo.sp_ChangePassword','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ChangePassword;
GO
CREATE PROCEDURE dbo.sp_ChangePassword
    @UserId INT,
    @NewPassword NVARCHAR(255)     -- hashed by app
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;
        UPDATE dbo.auth_user
        SET password = @NewPassword
        WHERE id = @UserId;
        DECLARE @Rows INT = @@ROWCOUNT;
        COMMIT TRAN;
        SELECT @Rows AS RowsAffected;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO

-- 3) sp_CreateAuthToken
IF OBJECT_ID(N'dbo.sp_CreateAuthToken','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CreateAuthToken;
GO
CREATE PROCEDURE dbo.sp_CreateAuthToken
    @TokenKey NVARCHAR(40),
    @IdUser INT,
    @ExpireDate DATETIME,
    @SingleSession BIT = 0    -- 0 = allow many devices; 1 = keep only one token per user
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;
        IF @SingleSession = 1
        BEGIN
            DELETE FROM dbo.auth_token WHERE idUser = @IdUser;
        END
        INSERT INTO dbo.auth_token (token_key, idUser, expire_date)
        VALUES (@TokenKey, @IdUser, @ExpireDate);
        DECLARE @TokenId BIGINT = CAST(SCOPE_IDENTITY() AS BIGINT);
        COMMIT TRAN;
        SELECT @TokenId AS TokenId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO

-- 4) sp_InvalidateToken
IF OBJECT_ID(N'dbo.sp_InvalidateToken','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InvalidateToken;
GO
CREATE PROCEDURE dbo.sp_InvalidateToken
    @TokenKey NVARCHAR(40)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;
        DELETE FROM dbo.auth_token WHERE token_key = @TokenKey;
        DECLARE @Rows INT = @@ROWCOUNT;
        COMMIT TRAN;
        SELECT @Rows AS RowsAffected;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO

-- 5) sp_InvalidateUserTokens
IF OBJECT_ID(N'dbo.sp_InvalidateUserTokens','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InvalidateUserTokens;
GO
CREATE PROCEDURE dbo.sp_InvalidateUserTokens
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;
        DELETE FROM dbo.auth_token WHERE idUser = @UserId;
        DECLARE @Rows INT = @@ROWCOUNT;
        COMMIT TRAN;
        SELECT @Rows AS RowsAffected;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO

-- 6) sp_GetUserByUsername
IF OBJECT_ID(N'dbo.sp_GetUserByUsername','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserByUsername;
GO
CREATE PROCEDURE dbo.sp_GetUserByUsername
    @Username NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, username, password, email, first_name, last_name,
           is_staff, is_superuser, is_active, date_joined, last_login
    FROM dbo.auth_user
    WHERE username = @Username AND is_active = 1;
END
GO

-- 7) sp_UpdateLastLogin
IF OBJECT_ID(N'dbo.sp_UpdateLastLogin','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdateLastLogin;
GO
CREATE PROCEDURE dbo.sp_UpdateLastLogin
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.auth_user SET last_login = SYSDATETIMEOFFSET() WHERE id = @UserId;
        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 8) sp_GetUserByEmail
IF OBJECT_ID(N'dbo.sp_GetUserByEmail','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserByEmail;
GO
CREATE PROCEDURE dbo.sp_GetUserByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, username, password, email, first_name, last_name,
           is_staff, is_superuser, is_active, date_joined, last_login
    FROM dbo.auth_user
    WHERE email = @Email AND is_active = 1;
END
GO

-- 9) sp_GetTokenByUserId - GET ACTIVE TOKEN BY USER ID
IF OBJECT_ID(N'dbo.sp_GetTokenByUserId','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetTokenByUserId;
GO
CREATE PROCEDURE dbo.sp_GetTokenByUserId
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 
        token_key,
        expire_date,
        idUser
    FROM dbo.auth_token 
    WHERE idUser = @UserId 
      AND expire_date > GETDATE()  -- Chỉ lấy token chưa hết hạn
    ORDER BY expire_date DESC;     -- Lấy token mới nhất
END
GO

-- 10) sp_CleanExpiredTokens - CLEANUP EXPIRED TOKENS
IF OBJECT_ID(N'dbo.sp_CleanExpiredTokens','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CleanExpiredTokens;
GO
CREATE PROCEDURE dbo.sp_CleanExpiredTokens
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @DeletedCount INT;
        
        DELETE FROM dbo.auth_token 
        WHERE expire_date < GETDATE();  -- Sử dụng expire_date thay vì expires_at
        
        SET @DeletedCount = @@ROWCOUNT;
        
        SELECT 
            @DeletedCount AS DeletedTokens,
            'Expired tokens cleaned successfully' AS Message;
    END TRY
    BEGIN CATCH
        SELECT 
            0 AS DeletedTokens,
            'Error cleaning tokens: ' + ERROR_MESSAGE() AS Message;
    END CATCH
END
GO

PRINT 'Auth SPs deployed.';
