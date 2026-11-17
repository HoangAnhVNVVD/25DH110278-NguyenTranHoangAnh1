USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'MyStore3')
BEGIN
    ALTER DATABASE MyStore3 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MyStore3;
END
GO

CREATE DATABASE MyStore3;
GO

USE MyStore3;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE [User]
(
    Username NVARCHAR(255) NOT NULL PRIMARY KEY,
    Password NCHAR(50) NOT NULL,
    UserRole NCHAR(1) NOT NULL
);
GO

CREATE TABLE Category
(
    CategoryID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CategoryName NVARCHAR(MAX) NOT NULL
);
GO

CREATE TABLE Customer
(
    CustomerID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CustomerName NVARCHAR(MAX) NOT NULL,
    CustomerPhone NVARCHAR(15) NOT NULL,
    CustomerEmail NVARCHAR(MAX) NULL,
    CustomerAddress NVARCHAR(MAX) NULL,
    Username NVARCHAR(255) NOT NULL,
    CONSTRAINT FK_User_Customer FOREIGN KEY (Username) REFERENCES [User](Username)
);
GO

CREATE TABLE Product
(
    ProductID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CategoryID INT NOT NULL,
    ProductName NVARCHAR(MAX) NOT NULL,
    ProductDecription NVARCHAR(MAX) NOT NULL,
    ProductPrice DECIMAL(18,2) NOT NULL,
    ProductImage NVARCHAR(MAX) NULL,
    Quantity INT NOT NULL DEFAULT 50,
    CONSTRAINT FK_Pro_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);
GO

CREATE TABLE [Order]
(
    OrderID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CustomerID INT NOT NULL,
    OrderDate DATETIME NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaymentStatus NVARCHAR(MAX) NULL,
    AddressDelivery NVARCHAR(MAX) NOT NULL,
    DeliveryMethod NVARCHAR(50) NULL,
    PaymentMethod NVARCHAR(50) NULL,
    ShippingAddress NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Order_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID)
);
GO

CREATE TABLE OrderDetail
(
    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductID INT NOT NULL,
    OrderID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice AS (Quantity * UnitPrice) PERSISTED,
    CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (ProductID) REFERENCES Product(ProductID),
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
);
GO

INSERT INTO [User] (Username, Password, UserRole) VALUES
('admin', '123456', 'A'),
('thanhtu', '123456', 'C'),
('hoanganh', '123456', 'C');
GO

INSERT INTO Category (CategoryName) VALUES
(N'Kinh tế - Tài chính'),
(N'Kỹ năng sống'),
(N'Văn học'),
(N'Công nghệ thông tin');
GO

INSERT INTO Customer (CustomerName, CustomerPhone, CustomerEmail, CustomerAddress, Username) VALUES
(N'Bui Thi Thanh Tu', '0901234567', 'thanhtu@gmail.com', N'280 An Dương Vương, P.4, Q.5, TP.HCM', 'thanhtu'),
(N'Nguyen Tran Hoang Anh', '0909876543', 'hoanganh@gmail.com', N'Hồ Chí Minh', 'hoanganh');
GO

INSERT INTO Product (CategoryID, ProductName, ProductDecription, ProductPrice, ProductImage, Quantity) VALUES
(1, N'Trò Chơi Của Những Kẻ Hiểu Luật', N'Chiến lược thay đổi tư duy làm chủ cuộc chơi tài chính', 106548, '~/Content/images/tro-choi-cua-nhung-ke-hieu-luat.jpg', 50),
(2, N'Nghệ Thuật Từ Chối – Cách Nói Không Mà Vẫn Có Được Đồng Thuận', N'Nghệ thuật từ chối nhẹ nhàng nhưng hiệu quả', 55900, '~/Content/images/nghe-thuat-tu-choi.jpg', 50),
(1, N'Kinh Doanh Thời Trang - 8 Bước Xây Dựng, Vận Hành Và Phát Triển Shop Quần Áo Của Bạn', N'Cẩm nang kinh doanh thời trang từ A-Z', 142000, '~/Content/images/kinh-doanh-thoi-trang.jpg', 40),
(1, N'LÀM GIÀU TỪ SIÊU CỔ PHIẾU – Câu Chuyện Của Gã Du Mục Phố Wall – JESSE C. STINE', N'Bí quyết đầu tư siêu cổ phiếu của Jesse C. Stine', 399000, '~/Content/images/lam-giau-tu-sieu-co-phieu.jpg', 30),
(3, N'Hãy Chăm Sóc Mẹ (Tái Bản 2019)', N'Tiểu thuyết cảm động về tình mẫu tử của Shin Kyung-sook', 67000, '~/Content/images/hay-cham-soc-me.jpg', 25),
(3, N'Cây Cam Ngọt Của Tôi', N'Tác phẩm kinh điển của nhà văn José Mauro de Vasconcelos', 73800, '~/Content/images/cay-cam-ngot-cua-toi.jpg', 20),
(4, N'Lập trình Web ASP.NET MVC5 từ Cơ bản đến Nâng cao', N'Giáo trình thực hành ASP.NET MVC5 Entity Framework', 150000, '~/Content/images/asp-net-mvc.jpg', 0);
GO

INSERT INTO [Order] (CustomerID, OrderDate, TotalAmount, PaymentStatus, AddressDelivery, DeliveryMethod, PaymentMethod, ShippingAddress) VALUES
(1, '2026-11-16 08:30:00', 734645, N'Đã giao', N'280 An Dương Vương, P.4, Q.5, TP.HCM', N'Giao hàng nhanh', N'Thanh toán tiền mặt', N'280 An Dương Vương, P.4, Q.5, TP.HCM'),
(1, '2026-11-16 09:15:00', 80300, N'Đã hủy', N'280 An Dương Vương, P.4, Q.5, TP.HCM', N'Giao hàng tiết kiệm', N'Thanh toán tiền mặt', N'280 An Dương Vương, P.4, Q.5, TP.HCM'),
(1, '2026-11-16 10:00:00', 624092, N'Chưa thanh toán', N'280 An Dương Vương, P.4, Q.5, TP.HCM', N'Giao hàng tiêu chuẩn', N'Chưa thanh toán', N'280 An Dương Vương, P.4, Q.5, TP.HCM');
GO

INSERT INTO OrderDetail (ProductID, OrderID, Quantity, UnitPrice) VALUES
(4, 1, 1, 399000),
(5, 1, 5, 67000),
(6, 2, 1, 73800),
(1, 3, 4, 106548),
(2, 3, 1, 55900),
(3, 3, 1, 142000);
GO