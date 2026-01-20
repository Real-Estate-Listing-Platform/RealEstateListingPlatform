USE master;
GO

-- 1. XÓA DATABASE CŨ (NẾU TỒN TẠI)
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'RealEstateListingPlatform')
BEGIN
    PRINT '>>> Dang xoa database cu (RealEstateListingPlatform)...';
    ALTER DATABASE RealEstateListingPlatform SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RealEstateListingPlatform;
END
GO

-- 2. TẠO DATABASE MỚI
PRINT '>>> Dang tao database moi...';
CREATE DATABASE RealEstateListingPlatform;
GO

USE RealEstateListingPlatform;
GO

-- =============================================================
-- 3. TẠO CẤU TRÚC BẢNG (SCHEMA)
-- =============================================================

-- BẢNG 1: USERS
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DisplayName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Seeker', 'Lister', 'Admin')),
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    
    PasswordHash NVARCHAR(MAX),
    AvatarUrl NVARCHAR(MAX),
    Bio NVARCHAR(500),
    
    IsEmailVerified BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    LastLoginAt DATETIME,
    
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO

-- BẢNG 2: LISTINGS (Đã thêm cột District)
CREATE TABLE Listings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ListerId UNIQUEIDENTIFIER NOT NULL,
    
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    TransactionType NVARCHAR(50),
    PropertyType NVARCHAR(50),
    Price DECIMAL(18, 2) NOT NULL,
    
    -- Location
    StreetName NVARCHAR(100),
    Ward NVARCHAR(50),
    District NVARCHAR(50),   -- <== ĐÃ THÊM CỘT NÀY
    City NVARCHAR(50),
    Area NVARCHAR(50),
    HouseNumber NVARCHAR(50),
    Latitude DECIMAL(9, 6),
    Longitude DECIMAL(9, 6),
    
    -- Specs
    Bedrooms INT,
    Bathrooms INT,
    Floors INT,
    LegalStatus NVARCHAR(50),
    FurnitureStatus NVARCHAR(50),
    Direction NVARCHAR(20),
    
    Status NVARCHAR(20) DEFAULT 'Draft' CHECK (Status IN ('Draft', 'PendingReview', 'Published', 'Rejected', 'Hidden', 'Expired', 'Violation')),
    ExpirationDate DATETIME,
    
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Listings_Users FOREIGN KEY (ListerId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

-- BẢNG 3: LISTING_MEDIA
CREATE TABLE ListingMedia (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ListingId UNIQUEIDENTIFIER NOT NULL,
    MediaType NVARCHAR(10) CHECK (MediaType IN ('image', 'video')),
    Url NVARCHAR(MAX),
    SortOrder INT,
    
    CONSTRAINT FK_ListingMedia_Listings FOREIGN KEY (ListingId) REFERENCES Listings(Id) ON DELETE CASCADE
);
GO

-- BẢNG 4: LISTING_TOUR360
CREATE TABLE ListingTour360 (
    ListingId UNIQUEIDENTIFIER PRIMARY KEY,
    Provider NVARCHAR(50),
    EmbedUrl NVARCHAR(MAX),
    
    CONSTRAINT FK_ListingTour360_Listings FOREIGN KEY (ListingId) REFERENCES Listings(Id) ON DELETE CASCADE
);
GO

-- BẢNG 5: LISTING_PRICE_HISTORY
CREATE TABLE ListingPriceHistory (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ListingId UNIQUEIDENTIFIER NOT NULL,
    OldPrice DECIMAL(18, 2),
    NewPrice DECIMAL(18, 2),
    ChangedByUserId UNIQUEIDENTIFIER,
    ChangedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_PriceHistory_Listings FOREIGN KEY (ListingId) REFERENCES Listings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PriceHistory_Users FOREIGN KEY (ChangedByUserId) REFERENCES Users(Id)
);
GO

-- BẢNG 6: LEADS
CREATE TABLE Leads (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ListingId UNIQUEIDENTIFIER NOT NULL,
    SeekerId UNIQUEIDENTIFIER NOT NULL,
    ListerId UNIQUEIDENTIFIER NOT NULL,
    
    Status NVARCHAR(20) DEFAULT 'New' CHECK (Status IN ('New', 'Contacted', 'Closed')),
    Message NVARCHAR(MAX),
    AppointmentDate DATETIME,
    ListerNote NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Leads_Listings FOREIGN KEY (ListingId) REFERENCES Listings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Leads_Seeker FOREIGN KEY (SeekerId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Leads_Lister FOREIGN KEY (ListerId) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

-- BẢNG 7: FAVORITES
CREATE TABLE Favorites (
    UserId UNIQUEIDENTIFIER NOT NULL,
    ListingId UNIQUEIDENTIFIER NOT NULL,
    SavedAt DATETIME DEFAULT GETDATE(),

    PRIMARY KEY (UserId, ListingId),
    CONSTRAINT FK_Favorites_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Favorites_Listings FOREIGN KEY (ListingId) REFERENCES Listings(Id) ON DELETE NO ACTION
);
GO

-- BẢNG 8: REPORTS
CREATE TABLE Reports (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReporterId UNIQUEIDENTIFIER NOT NULL,
    ListingId UNIQUEIDENTIFIER NOT NULL,
    
    Reason NVARCHAR(255),
    Status NVARCHAR(50) DEFAULT 'Pending',
    ResolvedBy UNIQUEIDENTIFIER,
    ResolvedAt DATETIME,
    AdminResponse NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Reports_Users FOREIGN KEY (ReporterId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Reports_Listings FOREIGN KEY (ListingId) REFERENCES Listings(Id) ON DELETE CASCADE
);
GO

-- BẢNG 9: NOTIFICATIONS
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200),
    Message NVARCHAR(MAX),
    IsRead BIT DEFAULT 0,
    Type NVARCHAR(50),
    RelatedLink NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

-- BẢNG 10: AUDIT_LOGS
CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ActorUserId UNIQUEIDENTIFIER,
    ActionType NVARCHAR(50),
    TargetType NVARCHAR(50),
    TargetId UNIQUEIDENTIFIER,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    IPAddress NVARCHAR(50),
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (ActorUserId) REFERENCES Users(Id) ON DELETE SET NULL
);
GO

-- =============================================================
-- 4. INSERT DATA (DỮ LIỆU MẪU)
-- =============================================================
PRINT '>>> Dang nap du lieu mau...';

DECLARE @AdminId UNIQUEIDENTIFIER = NEWID();
DECLARE @ListerId UNIQUEIDENTIFIER = NEWID();
DECLARE @SeekerId UNIQUEIDENTIFIER = NEWID();
DECLARE @Listing1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Listing2 UNIQUEIDENTIFIER = NEWID();

-- 1. Insert Users
INSERT INTO Users (Id, DisplayName, Role, Email, PasswordHash, IsActive, IsEmailVerified, Bio, AvatarUrl)
VALUES 
(@AdminId, N'System Admin', 'Admin', 'admin@sys.com', 'HASH_PASS_123', 1, 1, N'Quản trị viên hệ thống', NULL),
(@ListerId, N'Phạm Hương Broker', 'Lister', 'lister@gmail.com', 'HASH_PASS_123', 1, 1, N'Chuyên căn hộ cao cấp Q9', 'https://i.pravatar.cc/150?u=lister'),
(@SeekerId, N'Nguyễn Sinh Viên', 'Seeker', 'student@fpt.edu.vn', 'HASH_PASS_123', 1, 1, N'Tìm trọ giá rẻ', 'https://i.pravatar.cc/150?u=student');

-- 2. Insert Listings (Bây giờ đã có cột District nên sẽ không lỗi nữa)
INSERT INTO Listings (Id, ListerId, Title, TransactionType, PropertyType, Price, City, District, Ward, StreetName, Bedrooms, Area, Status, ExpirationDate)
VALUES 
(@Listing1, @ListerId, N'Bán căn hộ Vinhome Grand Park 2PN', 'Sell', 'Apartment', 3500000000, N'Hồ Chí Minh', N'Thủ Đức', N'Long Thạnh Mỹ', N'Nguyễn Xiển', 2, '59m2', 'Published', DATEADD(day, 60, GETDATE())),
(@Listing2, @ListerId, N'Cho thuê phòng trọ gần ĐH FPT', 'Rent', 'Room', 3500000, N'Hồ Chí Minh', N'Thủ Đức', N'Tăng Nhơn Phú', N'Lê Văn Việt', 1, '25m2', 'Published', DATEADD(day, 30, GETDATE()));

-- 3. Insert Media
INSERT INTO ListingMedia (ListingId, MediaType, Url, SortOrder) VALUES
(@Listing1, 'image', 'https://example.com/vinhome_living.jpg', 1),
(@Listing1, 'image', 'https://example.com/vinhome_bed.jpg', 2),
(@Listing2, 'image', 'https://example.com/tro_front.jpg', 1);

-- 4. Insert Lead
INSERT INTO Leads (ListingId, SeekerId, ListerId, Message, AppointmentDate)
VALUES (@Listing1, @SeekerId, @ListerId, N'Chào bạn, mình muốn xem nhà vào Chủ Nhật này ạ.', '2026-02-20 09:00:00');

-- 5. Insert Notification
INSERT INTO Notifications (UserId, Title, Message, Type)
VALUES (@ListerId, N'Khách hàng mới', N'Nguyễn Sinh Viên vừa nhắn tin cho bạn.', 'Lead');

PRINT '>>> FIXED: DA NAP DU LIEU THANH CONG!';
GO