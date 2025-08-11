-- =============================================
-- Authentication Library - Required SQL Stored Procedures
-- =============================================

-- 1. Create User
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CreateUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_CreateUser]
GO

CREATE PROCEDURE [dbo].[sp_CreateUser]
    @Username NVARCHAR(150),
    @Password NVARCHAR(255),
    @Email NVARCHAR(254),
    @FirstName NVARCHAR(150),
    @LastName NVARCHAR(150),
    @IsStaff BIT,
    @IsSuperuser BIT,
    @IsActive BIT,
    @DateJoined DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO auth_user (username, password, email, first_name, last_name, is_staff, is_superuser, is_active, date_joined)
    VALUES (@Username, @Password, @Email, @FirstName, @LastName, @IsStaff, @IsSuperuser, @IsActive, @DateJoined);
    
    SELECT SCOPE_IDENTITY() AS UserId;
END
GO

-- 2. Change Password
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_ChangePassword]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_ChangePassword]
GO

CREATE PROCEDURE [dbo].[sp_ChangePassword]
    @UserId INT,
    @NewPassword NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE auth_user 
    SET password = @NewPassword,
        last_login = GETUTCDATE()
    WHERE id = @UserId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 3. Create Auth Token
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CreateAuthToken]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_CreateAuthToken]
GO

CREATE PROCEDURE [dbo].[sp_CreateAuthToken]
    @TokenKey NVARCHAR(MAX),
    @IdUser INT,
    @ExpireDate DATETIME2,
    @CreatedDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Remove existing tokens for this user (optional - keep only one active token per user)
    DELETE FROM auth_token WHERE idUser = @IdUser;
    
    INSERT INTO auth_token (token_key, idUser, expire_date, created_date)
    VALUES (@TokenKey, @IdUser, @ExpireDate, @CreatedDate);
    
    SELECT SCOPE_IDENTITY() AS TokenId;
END
GO

-- 4. Invalidate Token
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_InvalidateToken]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_InvalidateToken]
GO

CREATE PROCEDURE [dbo].[sp_InvalidateToken]
    @TokenKey NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM auth_token 
    WHERE token_key = @TokenKey;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 5. Invalidate User Tokens
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_InvalidateUserTokens]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_InvalidateUserTokens]
GO

CREATE PROCEDURE [dbo].[sp_InvalidateUserTokens]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM auth_token 
    WHERE idUser = @UserId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 6. Clean Expired Tokens (Utility procedure)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CleanExpiredTokens]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_CleanExpiredTokens]
GO

CREATE PROCEDURE [dbo].[sp_CleanExpiredTokens]
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM auth_token 
    WHERE expire_date <= GETUTCDATE();
    
    SELECT @@ROWCOUNT AS TokensRemoved;
END
GO

-- 7. Get User by Username (for verification)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetUserByUsername]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_GetUserByUsername]
GO

CREATE PROCEDURE [dbo].[sp_GetUserByUsername]
    @Username NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT id, username, password, email, first_name, last_name, 
           is_staff, is_superuser, is_active, date_joined, last_login
    FROM auth_user 
    WHERE username = @Username AND is_active = 1;
END
GO

-- 8. Update Last Login
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpdateLastLogin]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_UpdateLastLogin]
GO

CREATE PROCEDURE [dbo].[sp_UpdateLastLogin]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE auth_user 
    SET last_login = GETUTCDATE()
    WHERE id = @UserId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'Authentication stored procedures created successfully!'
