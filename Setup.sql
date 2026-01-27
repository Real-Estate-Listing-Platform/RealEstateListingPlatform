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

-- BẢNG 2: LISTINGS
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
    District NVARCHAR(50),
    City NVARCHAR(50),
    Area NVARCHAR(50), -- Lưu ý: Nên dùng DECIMAL cho Area để tính toán, ở đây để NVARCHAR theo đề bài cũ
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
DECLARE @ListerId UNIQUEIDENTIFIER = NEWID(); -- Biến này sẽ dùng chung cho tất cả Listings
DECLARE @SeekerId UNIQUEIDENTIFIER = NEWID();

-- Khai báo ID cho các Listings
DECLARE @Listing1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Listing2 UNIQUEIDENTIFIER = NEWID();
DECLARE @L3 UNIQUEIDENTIFIER = NEWID();
DECLARE @L4 UNIQUEIDENTIFIER = NEWID();
DECLARE @L5 UNIQUEIDENTIFIER = NEWID();
DECLARE @L6 UNIQUEIDENTIFIER = NEWID();
DECLARE @L7 UNIQUEIDENTIFIER = NEWID();

-- 1. Insert Users
INSERT INTO Users (Id, DisplayName, Role, Email, PasswordHash, IsActive, IsEmailVerified, Bio, AvatarUrl)
VALUES 
(@AdminId, N'System Admin', 'Admin', 'admin@sys.com', 'HASH_PASS_123', 1, 1, N'Quản trị viên hệ thống', NULL),
(@ListerId, N'Phạm Hương Broker', 'Lister', 'lister@gmail.com', 'HASH_PASS_123', 1, 1, N'Chuyên căn hộ cao cấp Q9', 'https://i.pravatar.cc/150?u=lister'),
(@SeekerId, N'Nguyễn Sinh Viên', 'Seeker', 'student@fpt.edu.vn', 'HASH_PASS_123', 1, 1, N'Tìm trọ giá rẻ', 'https://i.pravatar.cc/150?u=student');

-- 2. Insert Listings
-- Lưu ý: Đã thêm cột 'Bathrooms' và 'Description' vào danh sách cột INSERT để khớp với dữ liệu mới
INSERT INTO Listings (Id, ListerId, Title, TransactionType, PropertyType, Price, City, District, Ward, StreetName, Bedrooms, Bathrooms, Area, Status, ExpirationDate, Description)
VALUES 
-- Tin 1
(@Listing1, @ListerId, N'Bán căn hộ Vinhome Grand Park 2PN', 'Sell', 'Apartment', 3500000000, N'Hồ Chí Minh', N'Thủ Đức', N'Long Thạnh Mỹ', N'Nguyễn Xiển', 2, 2, '59m2', 'Published', DATEADD(day, 60, GETDATE()), N'Căn hộ view công viên, tầng trung đẹp.'),
-- Tin 2
(@Listing2, @ListerId, N'Cho thuê phòng trọ gần ĐH FPT', 'Rent', 'Room', 3500000, N'Hồ Chí Minh', N'Thủ Đức', N'Tăng Nhơn Phú', N'Lê Văn Việt', 1, 1, '25m2', 'Published', DATEADD(day, 30, GETDATE()), N'Phòng trọ an ninh, giờ giấc tự do.'),
-- Tin 3 (Mới)
(@L3, @ListerId, N'Biệt thự Thảo Điền có hồ bơi, sân vườn rộng', 'Sell', 'Villa', 45000000000, N'Hồ Chí Minh', N'Thủ Đức', N'Thảo Điền', N'Nguyễn Văn Hưởng', 5, 6, '500m2', 'Published', DATEADD(day, 90, GETDATE()), N'Biệt thự đơn lập, full nội thất châu Âu, khu an ninh 24/7.'),
-- Tin 4 (Mới)
(@L4, @ListerId, N'Văn phòng hạng A view Bitexco, sàn trống suốt', 'Rent', 'Office', 55000000, N'Hồ Chí Minh', N'Quận 1', N'Bến Nghé', N'Nguyễn Huệ', 0, 2, '120m2', 'Published', DATEADD(day, 30, GETDATE()), N'Văn phòng ngay phố đi bộ, miễn phí phí quản lý 1 năm đầu.'),
-- Tin 5 (Mới)
(@L5, @ListerId, N'Đất thổ cư Củ Chi, mặt tiền đường nhựa', 'Sell', 'Land', 1800000000, N'Hồ Chí Minh', N'Củ Chi', N'Tân Phú Trung', N'Hương Lộ 2', 0, 0, '200m2', 'Published', DATEADD(day, 120, GETDATE()), N'Sổ hồng riêng, xây dựng tự do, gần bệnh viện Xuyên Á.'),
-- Tin 6 (Mới)
(@L6, @ListerId, N'Căn hộ Studio Full nội thất gần Hutech, UEF', 'Rent', 'Apartment', 7500000, N'Hồ Chí Minh', N'Bình Thạnh', N'Phường 25', N'Ung Văn Khiêm', 1, 1, '35m2', 'Published', DATEADD(day, 15, GETDATE()), N'Giờ giấc tự do, ra vào vân tay, có máy giặt riêng.'),
-- Tin 7 (Mới)
(@L7, @ListerId, N'Nhà phố liền kề Phú Mỹ Hưng, kinh doanh tốt', 'Sell', 'Townhouse', 12500000000, N'Hồ Chí Minh', N'Quận 7', N'Tân Phong', N'Nguyễn Đức Cảnh', 4, 4, '100m2', 'Published', DATEADD(day, 60, GETDATE()), N'Đang có hợp đồng thuê 40tr/tháng, thích hợp đầu tư giữ tiền.');

-- 3. Insert Media
INSERT INTO ListingMedia (ListingId, MediaType, Url, SortOrder) VALUES
-- Tin 1
(@Listing1, 'image', 'https://example.com/vinhome_living.jpg', 1),
(@Listing1, 'image', 'https://example.com/vinhome_bed.jpg', 2),
-- Tin 2
(@Listing2, 'image', 'https://example.com/tro_front.jpg', 1),
-- Tin 3
(@L3, 'image', 'https://images.unsplash.com/photo-1613977257363-707ba9348227?auto=format&fit=crop&w=800&q=80', 1),
(@L3, 'image', 'https://images.unsplash.com/photo-1613490493576-7fde63acd811?auto=format&fit=crop&w=800&q=80', 2),
(@L3, 'image', 'https://images.unsplash.com/photo-1484154218962-a1c002085d2f?auto=format&fit=crop&w=800&q=80', 3),
-- Tin 4
(@L4, 'image', 'https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=800&q=80', 1),
(@L4, 'image', 'https://images.unsplash.com/photo-1497215728101-856f4ea42174?auto=format&fit=crop&w=800&q=80', 2),
-- Tin 5
(@L5, 'image', 'https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=800&q=80', 1),
(@L5, 'image', 'https://images.unsplash.com/photo-1524813686514-a57563d77965?auto=format&fit=crop&w=800&q=80', 2),
-- Tin 6
(@L6, 'image', 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=800&q=80', 1),
(@L6, 'image', 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=800&q=80', 2),
-- Tin 7
(@L7, 'image', 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=800&q=80', 1),
(@L7, 'image', 'https://images.unsplash.com/photo-1616486338812-3dadae4b4f9d?auto=format&fit=crop&w=800&q=80', 2);

-- 4. Insert Lead
INSERT INTO Leads (ListingId, SeekerId, ListerId, Message, AppointmentDate)
VALUES (@Listing1, @SeekerId, @ListerId, N'Chào bạn, mình muốn xem nhà vào Chủ Nhật này ạ.', '2026-02-20 09:00:00');

-- 5. Insert Notification
INSERT INTO Notifications (UserId, Title, Message, Type)
VALUES (@ListerId, N'Khách hàng mới', N'Nguyễn Sinh Viên vừa nhắn tin cho bạn.', 'Lead');

PRINT '>>> FIXED: DA NAP DU LIEU THANH CONG!';
GO
