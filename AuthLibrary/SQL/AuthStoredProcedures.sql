-- ===== Optimized Auth SPs =====

-- 1) sp_CreateUser
IF OBJECT_ID(N'dbo.sp_CreateUser','P') IS NOT NULL DROP PROCEDURE dbo.sp_CreateUser;
GO
CREATE PROCEDURE dbo.sp_CreateUser
    @Username NVARCHAR(150),
    @Password NVARCHAR(255),
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
    INSERT INTO dbo.auth_user (
        username, password, email, first_name, last_name,
        is_staff, is_superuser, is_active, date_joined, last_login
    )
    VALUES (
        @Username, @Password, @Email, @FirstName, @LastName,
        @IsStaff, @IsSuperuser, @IsActive, @DateJoined, @LastLogin
    );
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS UserId;
END
GO

-- 2) sp_ChangePassword
IF OBJECT_ID(N'dbo.sp_ChangePassword','P') IS NOT NULL DROP PROCEDURE dbo.sp_ChangePassword;
GO
CREATE PROCEDURE dbo.sp_ChangePassword
    @UserId INT,
    @NewPassword NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.auth_user
    SET password = @NewPassword
    WHERE id = @UserId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 3) sp_CreateAuthToken
IF OBJECT_ID(N'dbo.sp_CreateAuthToken','P') IS NOT NULL DROP PROCEDURE dbo.sp_CreateAuthToken;
GO
CREATE PROCEDURE dbo.sp_CreateAuthToken
    @TokenKey NVARCHAR(1000),
    @IdUser INT,
    @ExpireDate DATETIMEOFFSET,
    @SingleSession BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    IF @SingleSession = 1
        DELETE FROM dbo.auth_token WHERE idUser = @IdUser;
    
    INSERT INTO dbo.auth_token (token_key, idUser, expire_date)
    VALUES (@TokenKey, @IdUser, @ExpireDate);
    
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS TokenId;
END
GO

-- 4) sp_InvalidateToken
IF OBJECT_ID(N'dbo.sp_InvalidateToken','P') IS NOT NULL DROP PROCEDURE dbo.sp_InvalidateToken;
GO
CREATE PROCEDURE dbo.sp_InvalidateToken
    @TokenKey NVARCHAR(40)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.auth_token WHERE token_key = @TokenKey;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 5) sp_InvalidateUserTokens
IF OBJECT_ID(N'dbo.sp_InvalidateUserTokens','P') IS NOT NULL DROP PROCEDURE dbo.sp_InvalidateUserTokens;
GO
CREATE PROCEDURE dbo.sp_InvalidateUserTokens
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.auth_token WHERE idUser = @UserId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 6) sp_GetUserByUsername
IF OBJECT_ID(N'dbo.sp_GetUserByUsername','P') IS NOT NULL DROP PROCEDURE dbo.sp_GetUserByUsername;
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
IF OBJECT_ID(N'dbo.sp_UpdateLastLogin','P') IS NOT NULL DROP PROCEDURE dbo.sp_UpdateLastLogin;
GO
CREATE PROCEDURE dbo.sp_UpdateLastLogin
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.auth_user 
    SET last_login = SYSDATETIMEOFFSET() 
    WHERE id = @UserId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 8) sp_GetUserByEmail
IF OBJECT_ID(N'dbo.sp_GetUserByEmail','P') IS NOT NULL DROP PROCEDURE dbo.sp_GetUserByEmail;
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

-- 9) sp_CleanExpiredTokens
IF OBJECT_ID(N'dbo.sp_CleanExpiredTokens','P') IS NOT NULL DROP PROCEDURE dbo.sp_CleanExpiredTokens;
GO
CREATE PROCEDURE dbo.sp_CleanExpiredTokens
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.auth_token WHERE expire_date < SWITCHOFFSET(SYSDATETIMEOFFSET(), '+07:00');
    SELECT @@ROWCOUNT AS DeletedTokens;
END
GO

PRINT 'Optimized Auth SPs deployed.';