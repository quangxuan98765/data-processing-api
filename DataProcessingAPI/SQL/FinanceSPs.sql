-- ===== OPTIMIZED FINANCE STORED PROCEDURES =====

-- 1) Get Thu/Chi data
IF OBJECT_ID(N'dbo.sp_Get_ThuChiTaiChinh','P') IS NOT NULL DROP PROCEDURE dbo.sp_Get_ThuChiTaiChinh;
GO
CREATE PROCEDURE dbo.sp_Get_ThuChiTaiChinh
    @ThangTaiChinh INT = 0,
    @NamTaiChinh INT = 0,
    @IdNguon INT = 0,
    @LoaiNguon INT = 1 -- 1=Thu, 2=Chi
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @LoaiNguon = 2 -- Chi
    BEGIN
        SELECT 
            c.ID, c.ThangTaiChinh, c.NamTaiChinh, c.IdNguon,
            c.LoaiChi as LoaiThuChi, c.SoTien, c.MoTa, c.GhiChu,
            c.ThoiGianNhap, c.IDNguoiDung, c.NguoiNhap
        FROM TaiChinh_ChiHoatDong c
        INNER JOIN TaiChinh_Nguon n ON c.IdNguon = n.Id
        WHERE (@ThangTaiChinh = 0 OR c.ThangTaiChinh = @ThangTaiChinh)
          AND (@NamTaiChinh = 0 OR c.NamTaiChinh = @NamTaiChinh)
          AND (@IdNguon = 0 OR c.IdNguon = @IdNguon)
          AND n.Loai = 2
        ORDER BY c.ThoiGianNhap DESC;
    END
    ELSE -- Thu
    BEGIN
        SELECT 
            t.ID, t.ThangTaiChinh, t.NamTaiChinh, t.IdNguon,
            t.LoaiThu as LoaiThuChi, t.SoTien, t.MoTa, t.GhiChu,
            t.ThoiGianNhap, t.IDNguoiDung, t.NguoiNhap
        FROM TaiChinh_ThuHoatDong t
        INNER JOIN TaiChinh_Nguon n ON t.IdNguon = n.Id
        WHERE (@ThangTaiChinh = 0 OR t.ThangTaiChinh = @ThangTaiChinh)
          AND (@NamTaiChinh = 0 OR t.NamTaiChinh = @NamTaiChinh)
          AND (@IdNguon = 0 OR t.IdNguon = @IdNguon)
          AND n.Loai = 1
        ORDER BY t.ThoiGianNhap DESC;
    END
END
GO

-- 2) Insert Thu/Chi
IF OBJECT_ID(N'dbo.sp_Insert_ThuChiTaiChinh','P') IS NOT NULL DROP PROCEDURE dbo.sp_Insert_ThuChiTaiChinh;
GO
CREATE PROCEDURE dbo.sp_Insert_ThuChiTaiChinh
    @ThangTaiChinh INT,
    @NamTaiChinh INT,
    @IdNguon INT,
    @SoTien DECIMAL(18,2),
    @MoTa NVARCHAR(500) = NULL,
    @GhiChu NVARCHAR(500) = NULL,
    @IDNguoiDung NVARCHAR(100),
    @NguoiNhap NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LoaiNguon INT;
    
    -- Validate
    SELECT @LoaiNguon = Loai FROM TaiChinh_Nguon WHERE Id = @IdNguon;
    
    IF @LoaiNguon IS NULL
        RAISERROR('IdNguon không tồn tại', 16, 1);
        
    IF @ThangTaiChinh NOT BETWEEN 1 AND 12 OR @NamTaiChinh < 1900 OR @SoTien <= 0
        RAISERROR('Dữ liệu không hợp lệ', 16, 1);
    
    -- Insert based on type
    IF @LoaiNguon = 1 -- Thu
    BEGIN
        INSERT INTO TaiChinh_ThuHoatDong
            (ThangTaiChinh, NamTaiChinh, IdNguon, LoaiThu, SoTien, MoTa, GhiChu, ThoiGianNhap, IDNguoiDung, NguoiNhap)
        VALUES
            (@ThangTaiChinh, @NamTaiChinh, @IdNguon, N'Thu hoạt động', @SoTien, @MoTa, @GhiChu, GETDATE(), @IDNguoiDung, @NguoiNhap);
    END
    ELSE IF @LoaiNguon = 2 -- Chi
    BEGIN
        INSERT INTO TaiChinh_ChiHoatDong
            (ThangTaiChinh, NamTaiChinh, IdNguon, LoaiChi, SoTien, MoTa, GhiChu, ThoiGianNhap, IDNguoiDung, NguoiNhap)
        VALUES
            (@ThangTaiChinh, @NamTaiChinh, @IdNguon, N'Chi hoạt động', @SoTien, @MoTa, @GhiChu, GETDATE(), @IDNguoiDung, @NguoiNhap);
    END
    ELSE
        RAISERROR('Loại nguồn không hợp lệ', 16, 1);
    
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO

-- 3) Update Thu/Chi
IF OBJECT_ID(N'dbo.sp_Update_ThuChiTaiChinh','P') IS NOT NULL DROP PROCEDURE dbo.sp_Update_ThuChiTaiChinh;
GO
CREATE PROCEDURE dbo.sp_Update_ThuChiTaiChinh
    @Id INT,
    @ThangTaiChinh INT,
    @NamTaiChinh INT,
    @IdNguon INT,
    @SoTien DECIMAL(18,2),
    @MoTa NVARCHAR(500) = NULL,
    @GhiChu NVARCHAR(500) = NULL,
    @IDNguoiDung NVARCHAR(100),
    @NguoiNhap NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LoaiNguon INT, @OldLoaiNguon INT;
    DECLARE @OwnerID NVARCHAR(100); -- 🔒 Thêm owner check
    
    -- Validate new source
    SELECT @LoaiNguon = Loai FROM TaiChinh_Nguon WHERE Id = @IdNguon;
    IF @LoaiNguon IS NULL
        RAISERROR('Nguồn thu/chi không tồn tại', 16, 1);
    
    -- Validate input
    IF @ThangTaiChinh NOT BETWEEN 1 AND 12 OR @NamTaiChinh < 1900 OR @SoTien <= 0
        RAISERROR('Dữ liệu không hợp lệ', 16, 1);
    
    -- 🔒 Find old record type AND check ownership
    SELECT @OldLoaiNguon = n.Loai, @OwnerID = t.IDNguoiDung
    FROM TaiChinh_ThuHoatDong t 
    INNER JOIN TaiChinh_Nguon n ON t.IdNguon = n.Id 
    WHERE t.Id = @Id;
    
    IF @OldLoaiNguon IS NULL
    BEGIN
        SELECT @OldLoaiNguon = n.Loai, @OwnerID = c.IDNguoiDung
        FROM TaiChinh_ChiHoatDong c 
        INNER JOIN TaiChinh_Nguon n ON c.IdNguon = n.Id 
        WHERE c.Id = @Id;
        
        IF @OldLoaiNguon IS NULL
            RAISERROR('Không tìm thấy bản ghi', 16, 1);
    END
    
    -- 🔒 Check ownership
    IF @OwnerID IS NULL OR @OwnerID <> @IDNguoiDung
    BEGIN
        SELECT -2 AS UpdatedId; -- -2 = không có quyền sửa
        RETURN;
    END
    
    -- Handle type change
    IF @OldLoaiNguon != @LoaiNguon
    BEGIN
        -- Delete old record
        IF @OldLoaiNguon = 1 
            DELETE FROM TaiChinh_ThuHoatDong WHERE Id = @Id;
        ELSE 
            DELETE FROM TaiChinh_ChiHoatDong WHERE Id = @Id;
        
        -- Insert new record with same ID
        IF @LoaiNguon = 1
        BEGIN
            SET IDENTITY_INSERT TaiChinh_ThuHoatDong ON;
            INSERT INTO TaiChinh_ThuHoatDong
                (Id, ThangTaiChinh, NamTaiChinh, IdNguon, LoaiThu, SoTien, MoTa, GhiChu, ThoiGianNhap, IDNguoiDung, NguoiNhap)
            VALUES
                (@Id, @ThangTaiChinh, @NamTaiChinh, @IdNguon, N'Thu hoạt động', @SoTien, @MoTa, @GhiChu, GETDATE(), @IDNguoiDung, @NguoiNhap);
            SET IDENTITY_INSERT TaiChinh_ThuHoatDong OFF;
        END
        ELSE
        BEGIN
            SET IDENTITY_INSERT TaiChinh_ChiHoatDong ON;
            INSERT INTO TaiChinh_ChiHoatDong
                (Id, ThangTaiChinh, NamTaiChinh, IdNguon, LoaiChi, SoTien, MoTa, GhiChu, ThoiGianNhap, IDNguoiDung, NguoiNhap)
            VALUES
                (@Id, @ThangTaiChinh, @NamTaiChinh, @IdNguon, N'Chi hoạt động', @SoTien, @MoTa, @GhiChu, GETDATE(), @IDNguoiDung, @NguoiNhap);
            SET IDENTITY_INSERT TaiChinh_ChiHoatDong OFF;
        END
    END
    ELSE
    BEGIN
        -- Update existing record
        IF @LoaiNguon = 1
        BEGIN
            UPDATE TaiChinh_ThuHoatDong
            SET ThangTaiChinh = @ThangTaiChinh, NamTaiChinh = @NamTaiChinh,
                IdNguon = @IdNguon, SoTien = @SoTien, MoTa = @MoTa,
                GhiChu = @GhiChu, ThoiGianNhap = GETDATE(),
                IDNguoiDung = @IDNguoiDung, NguoiNhap = @NguoiNhap
            WHERE Id = @Id;
        END
        ELSE
        BEGIN
            UPDATE TaiChinh_ChiHoatDong
            SET ThangTaiChinh = @ThangTaiChinh, NamTaiChinh = @NamTaiChinh,
                IdNguon = @IdNguon, SoTien = @SoTien, MoTa = @MoTa,
                GhiChu = @GhiChu, ThoiGianNhap = GETDATE(),
                IDNguoiDung = @IDNguoiDung, NguoiNhap = @NguoiNhap
            WHERE Id = @Id;
        END
    END
    
    SELECT @Id AS UpdatedId;
END
GO

-- 4) Delete Thu/Chi
IF OBJECT_ID(N'dbo.sp_Delete_ThuChiTaiChinh','P') IS NOT NULL DROP PROCEDURE dbo.sp_Delete_ThuChiTaiChinh;
GO
CREATE PROCEDURE dbo.sp_Delete_ThuChiTaiChinh
    @ID INT,
    @LoaiHoatDong INT, -- 1=Thu, 2=Chi
    @IDNguoiDung NVARCHAR(100) -- 🔒 Thêm ownership check
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RowsDeleted INT = 0;
    DECLARE @OwnerID NVARCHAR(100);
    
    -- 🔒 Check ownership trước khi delete
    IF @LoaiHoatDong = 1
    BEGIN
        SELECT @OwnerID = IDNguoiDung FROM TaiChinh_ThuHoatDong WHERE ID = @ID;
        IF @OwnerID IS NULL OR @OwnerID <> @IDNguoiDung
        BEGIN
            SELECT -2 AS RowsDeleted; -- -2 = không có quyền xóa
            RETURN;
        END
        DELETE FROM TaiChinh_ThuHoatDong WHERE ID = @ID;
        SET @RowsDeleted = @@ROWCOUNT;
    END
    ELSE IF @LoaiHoatDong = 2
    BEGIN
        SELECT @OwnerID = IDNguoiDung FROM TaiChinh_ChiHoatDong WHERE ID = @ID;
        IF @OwnerID IS NULL OR @OwnerID <> @IDNguoiDung
        BEGIN
            SELECT -2 AS RowsDeleted; -- -2 = không có quyền xóa
            RETURN;
        END
        DELETE FROM TaiChinh_ChiHoatDong WHERE ID = @ID;
        SET @RowsDeleted = @@ROWCOUNT;
    END
    ELSE
    BEGIN
        RAISERROR('LoaiHoatDong không hợp lệ (1=Thu, 2=Chi)', 16, 1);
    END
    
    SELECT @RowsDeleted AS RowsDeleted;
END
GO

PRINT 'Finance SPs optimized successfully.';