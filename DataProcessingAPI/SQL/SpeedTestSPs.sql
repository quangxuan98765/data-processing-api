-- ==========================================
-- 🚀 SPEED TEST RESULTS - STORED PROCEDURES
-- ==========================================

-- 1) GET ALL SPEED TEST RESULTS WITH DATE FILTERING
IF OBJECT_ID(N'dbo.sp_Get_ICT_SpeedTestResults','P') IS NOT NULL 
    DROP PROCEDURE dbo.sp_Get_ICT_SpeedTestResults;
GO
CREATE PROCEDURE [dbo].[sp_Get_ICT_SpeedTestResults]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Nếu không truyền tham số, mặc định lấy năm hiện tại
    IF @StartDate IS NULL
        SET @StartDate = CAST(CAST(YEAR(GETDATE()) AS VARCHAR(4)) + '-01-01' AS DATETIME)
    
    IF @EndDate IS NULL
        SET @EndDate = CAST(CAST(YEAR(GETDATE()) AS VARCHAR(4)) + '-12-31 23:59:59' AS DATETIME)
    ELSE
        -- Đảm bảo EndDate bao gồm cả ngày cuối (23:59:59)
        SET @EndDate = DATEADD(DAY, 1, CAST(@EndDate AS DATE))
    
    SELECT 
        ID,
        ThoiGianDo,
        DiaDiem,
        TocDoTaiXuong_Mbps,
        TocDoTaiLen_Mbps,
        Ping_ms,
        NguoiNhap,
        ThoiGianNhap,
        IDNguoiDung
    FROM ICT_SpeedTestResults
    WHERE ThoiGianDo >= @StartDate 
      AND ThoiGianDo <= @EndDate
    ORDER BY ThoiGianDo DESC
END
GO

-- 2) CREATE NEW SPEED TEST RESULT
IF OBJECT_ID(N'dbo.sp_Insert_ICT_SpeedTestResults','P') IS NOT NULL 
    DROP PROCEDURE dbo.sp_Insert_ICT_SpeedTestResults;
GO
CREATE PROCEDURE [dbo].[sp_Insert_ICT_SpeedTestResults]
    @ThoiGianDo DATETIME,
    @DiaDiem NVARCHAR(255),
    @TocDoTaiXuong_Mbps FLOAT,
    @TocDoTaiLen_Mbps FLOAT,
    @Ping_ms INT,
    @NguoiNhap NVARCHAR(100),
    @IDNguoiDung NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @result BIGINT;
    
    BEGIN TRAN
    BEGIN TRY
        INSERT INTO [dbo].[ICT_SpeedTestResults] (
            ThoiGianDo,
            DiaDiem,
            TocDoTaiXuong_Mbps,
            TocDoTaiLen_Mbps,
            Ping_ms,
            NguoiNhap,
            ThoiGianNhap,
            IDNguoiDung
        )
        VALUES (
            @ThoiGianDo,
            @DiaDiem,
            @TocDoTaiXuong_Mbps,
            @TocDoTaiLen_Mbps,
            @Ping_ms,
            @NguoiNhap,
            GETDATE(),
            @IDNguoiDung
        );
        
        SET @result = SCOPE_IDENTITY();
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        SET @result = -1;
        ROLLBACK TRAN;
    END CATCH

    SELECT @result AS ReturnCode;
    RETURN @result;
END
GO

-- 3) UPDATE EXISTING SPEED TEST RESULT
IF OBJECT_ID(N'dbo.sp_Update_ICT_SpeedTestResults','P') IS NOT NULL 
    DROP PROCEDURE dbo.sp_Update_ICT_SpeedTestResults;
GO
CREATE PROCEDURE [dbo].[sp_Update_ICT_SpeedTestResults]
    @ID BIGINT,
    @ThoiGianDo DATETIME,
    @DiaDiem NVARCHAR(255),
    @TocDoTaiXuong_Mbps FLOAT,
    @TocDoTaiLen_Mbps FLOAT,
    @Ping_ms INT,
    @NguoiNhap NVARCHAR(100),
    @IDNguoiDung NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @result BIGINT;
    
    BEGIN TRAN
    BEGIN TRY
        -- 1. Lấy owner của record
        DECLARE @OwnerID NVARCHAR(100);
        SELECT @OwnerID = IDNguoiDung
        FROM dbo.ICT_SpeedTestResults
        WHERE ID = @ID;

        -- 2. Kiểm tra quyền: nếu khác, trả về lỗi
        IF @OwnerID IS NULL OR @OwnerID <> @IDNguoiDung
        BEGIN
            SET @result = -2;  -- mã lỗi "không được phép"
            ROLLBACK TRAN;
            SELECT @result AS ReturnCode;
            RETURN @result;
        END
        
        -- 3. Thực hiện update
        UPDATE [dbo].[ICT_SpeedTestResults]
        SET
            ThoiGianDo = @ThoiGianDo,
            DiaDiem = @DiaDiem,
            TocDoTaiXuong_Mbps = @TocDoTaiXuong_Mbps,
            TocDoTaiLen_Mbps = @TocDoTaiLen_Mbps,
            Ping_ms = @Ping_ms,
            NguoiNhap = @NguoiNhap,
            IDNguoiDung = @IDNguoiDung,
            ThoiGianNhap = GETDATE()  -- Cập nhật lại thời gian nhập
        WHERE ID = @ID;
        
        SET @result = @ID;
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        SET @result = -1;
        ROLLBACK TRAN;
    END CATCH

    SELECT @result AS ReturnCode;
    RETURN @result;
END
GO

-- 4) DELETE SPEED TEST RESULT WITH PERMISSION CHECK
IF OBJECT_ID(N'dbo.sp_Delete_ICT_SpeedTestResults','P') IS NOT NULL 
    DROP PROCEDURE dbo.sp_Delete_ICT_SpeedTestResults;
GO
CREATE PROCEDURE [dbo].[sp_Delete_ICT_SpeedTestResults]
    @ID BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @result INT;
    
    BEGIN TRAN
    BEGIN TRY
        DELETE FROM dbo.ICT_SpeedTestResults WHERE ID = @ID;
        SET @result = 1;            -- 1 = xóa thành công
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN;
        SET @result = ERROR_NUMBER();  -- ví dụ 547 nếu lỗi FK
    END CATCH

    SELECT @result AS ReturnCode;
    RETURN @result;
END
GO

PRINT '✅ Speed Test Stored Procedures deployed successfully!';