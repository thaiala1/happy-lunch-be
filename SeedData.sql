USE [happylunch]
GO

-- Clear existing data
SET QUOTED_IDENTIFIER ON;
GO

DELETE FROM Review;
DELETE FROM ProductImage;
DELETE FROM CartItem;
DELETE FROM Cart;
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM Product;
DELETE FROM Category;
GO

-- Seed Categories theo menu Shop
INSERT INTO Category (Name, CreatedAt) VALUES
(N'Món ăn mặn', GETDATE()),
(N'Món ăn chay', GETDATE()),
(N'Món ăn kiêng', GETDATE()),
(N'Trái cây', GETDATE()),
(N'Đồ uống', GETDATE()),
(N'Combo', GETDATE()),
(N'Giảm giá', GETDATE());
GO

-- Get ShopId and Category IDs, then insert Products
DECLARE @ShopId BIGINT;
SELECT TOP 1 @ShopId = Id FROM users WHERE Role = 'Admin' OR Role = 'User';
IF @ShopId IS NULL
BEGIN
    INSERT INTO users (user_login, user_pass, user_email, user_registered, user_status, display_name, EmailConfirmed, Role)
    VALUES ('admin', '$2a$11$KIXQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 'admin@happylunch.com', GETDATE(), 1, N'Admin', 1, 'Admin');
    SET @ShopId = SCOPE_IDENTITY();
END

DECLARE @CategoryMan BIGINT, @CategoryChay BIGINT, @CategoryKieng BIGINT, @CategoryTraiCay BIGINT, @CategoryDoUong BIGINT, @CategoryCombo BIGINT, @CategoryGiamGia BIGINT;
SELECT TOP 1 @CategoryMan = Id FROM Category WHERE Name = N'Món ăn mặn';
SELECT TOP 1 @CategoryChay = Id FROM Category WHERE Name = N'Món ăn chay';
SELECT TOP 1 @CategoryKieng = Id FROM Category WHERE Name = N'Món ăn kiêng';
SELECT TOP 1 @CategoryTraiCay = Id FROM Category WHERE Name = N'Trái cây';
SELECT TOP 1 @CategoryDoUong = Id FROM Category WHERE Name = N'Đồ uống';
SELECT TOP 1 @CategoryCombo = Id FROM Category WHERE Name = N'Combo';
SELECT TOP 1 @CategoryGiamGia = Id FROM Category WHERE Name = N'Giảm giá';

-- Món ăn mặn
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Cơm trưa đặc biệt', N'Cơm trưa với thịt kho tàu, canh chua cá lóc và rau xào tổng hợp. Đầy đủ dinh dưỡng cho bữa trưa.', @CategoryMan, 50000, 45000, 'InStock', 100, @ShopId, GETDATE(), GETDATE()),
(N'Cơm gà nướng mật ong', N'Cơm với gà nướng mật ong thơm lừng, rau củ tươi và nước sốt đặc biệt. Món ăn được yêu thích nhất.', @CategoryMan, 55000, 50000, 'InStock', 90, @ShopId, GETDATE(), GETDATE()),
(N'Cơm tấm sườn bì chả', N'Cơm tấm truyền thống với sườn nướng, bì, chả trứng và đồ chua. Món ăn đặc trưng miền Nam.', @CategoryMan, 48000, 45000, 'InStock', 85, @ShopId, GETDATE(), GETDATE()),
(N'Bún chả Hà Nội', N'Bún chả Hà Nội đặc biệt với thịt nướng than hoa, nước mắm pha chua ngọt và rau sống tươi.', @CategoryMan, 45000, NULL, 'InStock', 70, @ShopId, GETDATE(), GETDATE()),
(N'Bún bò Huế', N'Bún bò Huế cay nồng với thịt bò, giò heo, chả cua và rau thơm. Hương vị đậm đà đặc trưng xứ Huế.', @CategoryMan, 60000, 55000, 'InStock', 65, @ShopId, GETDATE(), GETDATE()),
(N'Phở bò tái chín', N'Phở bò truyền thống với nước dùng đậm đà, thịt bò tái chín, hành lá và rau thơm. Món ăn quốc dân.', @CategoryMan, 60000, NULL, 'InStock', 80, @ShopId, GETDATE(), GETDATE()),
(N'Bánh mì thịt nướng', N'Bánh mì giòn tan với thịt nướng thơm lừng, pate, chả lụa và rau củ tươi. Bữa sáng hoàn hảo.', @CategoryMan, 35000, 30000, 'InStock', 120, @ShopId, GETDATE(), GETDATE()),
(N'Bánh cuốn nóng', N'Bánh cuốn nóng với nhân thịt, chả lụa, nước mắm pha và rau thơm. Món ăn sáng nhẹ nhàng.', @CategoryMan, 40000, NULL, 'InStock', 60, @ShopId, GETDATE(), GETDATE()),
(N'Cơm rang dưa bò', N'Cơm rang thơm ngon với dưa bò chua ngọt, trứng và hành lá. Món ăn đơn giản nhưng hấp dẫn.', @CategoryMan, 42000, 38000, 'InStock', 75, @ShopId, GETDATE(), GETDATE()),
(N'Bún riêu cua', N'Bún riêu cua đậm đà với riêu cua, thịt bò, đậu phụ và rau sống. Món canh chua đặc biệt.', @CategoryMan, 50000, 45000, 'InStock', 55, @ShopId, GETDATE(), GETDATE());

-- Món ăn chay
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Cơm chay nấm', N'Cơm chay với nấm xào, đậu phụ chiên và rau củ tươi. Món ăn thanh đạm, tốt cho sức khỏe.', @CategoryChay, 45000, 40000, 'InStock', 50, @ShopId, GETDATE(), GETDATE()),
(N'Bún chay đậu hủ', N'Bún chay với đậu hủ chiên, nấm, rau củ và nước dùng chay thanh đạm. Món ăn chay ngon miệng.', @CategoryChay, 40000, NULL, 'InStock', 45, @ShopId, GETDATE(), GETDATE()),
(N'Phở chay', N'Phở chay với nước dùng rau củ đậm đà, đậu phụ, nấm và rau thơm. Món chay đầy đủ dinh dưỡng.', @CategoryChay, 50000, 45000, 'InStock', 40, @ShopId, GETDATE(), GETDATE()),
(N'Salad chay trộn', N'Salad chay với rau xanh tươi, đậu phụ, hạt điều và sốt chay đặc biệt. Món ăn thanh mát.', @CategoryChay, 35000, 30000, 'InStock', 60, @ShopId, GETDATE(), GETDATE()),
(N'Cơm chay thập cẩm', N'Cơm chay với nhiều món: đậu phụ, nấm, rau củ xào và canh chua chay. Đầy đủ dinh dưỡng.', @CategoryChay, 48000, 43000, 'InStock', 35, @ShopId, GETDATE(), GETDATE());

-- Món ăn kiêng
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Salad rau xanh kiêng', N'Salad với rau xanh tươi, ức gà luộc, trứng và sốt kiêng đặc biệt. Phù hợp người ăn kiêng.', @CategoryKieng, 35000, 30000, 'InStock', 55, @ShopId, GETDATE(), GETDATE()),
(N'Cơm gà luộc kiêng', N'Cơm gạo lứt với ức gà luộc, rau củ hấp và sốt kiêng. Món ăn ít calo, giàu protein.', @CategoryKieng, 50000, 45000, 'InStock', 40, @ShopId, GETDATE(), GETDATE()),
(N'Bún gạo lứt kiêng', N'Bún gạo lứt với thịt bò nạc, rau củ và nước dùng thanh đạm. Món ăn kiêng đầy đủ dinh dưỡng.', @CategoryKieng, 45000, 40000, 'InStock', 30, @ShopId, GETDATE(), GETDATE()),
(N'Salad cá hồi kiêng', N'Salad với cá hồi nướng, rau xanh và sốt kiêng. Món ăn giàu omega-3, tốt cho sức khỏe.', @CategoryKieng, 65000, 60000, 'InStock', 25, @ShopId, GETDATE(), GETDATE());

-- Trái cây
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Trái cây tươi mix', N'Đĩa trái cây tươi mix với dưa hấu, dứa, xoài, thanh long và nho. Trái cây tươi ngon mỗi ngày.', @CategoryTraiCay, 30000, 25000, 'InStock', 100, @ShopId, GETDATE(), GETDATE()),
(N'Dưa hấu tươi', N'Dưa hấu tươi ngọt mát, cắt sẵn. Giải nhiệt mùa hè, giàu vitamin và khoáng chất.', @CategoryTraiCay, 20000, NULL, 'InStock', 80, @ShopId, GETDATE(), GETDATE()),
(N'Xoài chín cây', N'Xoài chín cây ngọt ngào, thơm lừng. Trái cây nhiệt đới đầy vitamin C.', @CategoryTraiCay, 25000, 22000, 'InStock', 60, @ShopId, GETDATE(), GETDATE()),
(N'Dứa tươi cắt sẵn', N'Dứa tươi cắt sẵn, ngọt mát. Trái cây giàu enzyme, tốt cho tiêu hóa.', @CategoryTraiCay, 18000, 15000, 'InStock', 70, @ShopId, GETDATE(), GETDATE()),
(N'Cam tươi vắt', N'Nước cam tươi vắt, không đường. Giàu vitamin C, tăng cường sức đề kháng.', @CategoryTraiCay, 25000, NULL, 'InStock', 90, @ShopId, GETDATE(), GETDATE());

-- Đồ uống
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Cà phê đá', N'Cà phê đá truyền thống pha phin, đậm đà hương vị. Thức uống yêu thích của người Việt.', @CategoryDoUong, 20000, 18000, 'InStock', 150, @ShopId, GETDATE(), GETDATE()),
(N'Trà đá', N'Trà đá mát lạnh, giải nhiệt. Thức uống đơn giản nhưng tươi mát.', @CategoryDoUong, 10000, NULL, 'InStock', 300, @ShopId, GETDATE(), GETDATE()),
(N'Nước ngọt', N'Coca Cola, Pepsi, 7Up, Sprite. Nước ngọt có ga, mát lạnh.', @CategoryDoUong, 15000, NULL, 'InStock', 200, @ShopId, GETDATE(), GETDATE()),
(N'Trà sữa', N'Trà sữa thơm ngon với trân châu, thạch và topping đa dạng. Thức uống trẻ trung.', @CategoryDoUong, 35000, 30000, 'InStock', 80, @ShopId, GETDATE(), GETDATE()),
(N'Sinh tố trái cây', N'Sinh tố trái cây tươi mix, không đường. Giàu vitamin, tốt cho sức khỏe.', @CategoryDoUong, 30000, 25000, 'InStock', 60, @ShopId, GETDATE(), GETDATE()),
(N'Nước ép cam', N'Nước ép cam tươi, không đường. Giàu vitamin C, tăng cường miễn dịch.', @CategoryDoUong, 25000, 22000, 'InStock', 70, @ShopId, GETDATE(), GETDATE()),
(N'Cà phê sữa đá', N'Cà phê sữa đá pha phin, ngọt ngào. Thức uống yêu thích buổi sáng.', @CategoryDoUong, 22000, 20000, 'InStock', 120, @ShopId, GETDATE(), GETDATE());

-- Combo
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Combo trưa văn phòng', N'Combo gồm: Cơm trưa đặc biệt + Canh + Đồ uống. Bữa trưa đầy đủ cho dân văn phòng.', @CategoryCombo, 65000, 58000, 'InStock', 50, @ShopId, GETDATE(), GETDATE()),
(N'Combo sáng nhanh', N'Combo gồm: Bánh mì thịt nướng + Cà phê đá. Bữa sáng nhanh gọn, đầy năng lượng.', @CategoryCombo, 45000, 40000, 'InStock', 80, @ShopId, GETDATE(), GETDATE()),
(N'Combo chay healthy', N'Combo gồm: Cơm chay nấm + Salad chay + Nước ép. Món chay đầy đủ dinh dưỡng.', @CategoryCombo, 70000, 63000, 'InStock', 30, @ShopId, GETDATE(), GETDATE()),
(N'Combo kiêng giảm cân', N'Combo gồm: Salad kiêng + Cơm gà luộc + Trà đá. Món ăn kiêng, ít calo.', @CategoryCombo, 75000, 68000, 'InStock', 25, @ShopId, GETDATE(), GETDATE());

-- Giảm giá (sản phẩm đang sale)
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Cơm trưa đặc biệt - Sale', N'Cơm trưa với thịt kho tàu, canh chua và rau xào. Đang giảm giá 10%.', @CategoryGiamGia, 50000, 45000, 'InStock', 50, @ShopId, GETDATE(), GETDATE()),
(N'Bún bò Huế - Sale', N'Bún bò Huế cay nồng đặc trưng. Đang giảm giá đặc biệt.', @CategoryGiamGia, 60000, 55000, 'InStock', 40, @ShopId, GETDATE(), GETDATE()),
(N'Trà sữa - Sale', N'Trà sữa thơm ngon với topping đa dạng. Giảm giá cuối tuần.', @CategoryGiamGia, 35000, 30000, 'InStock', 60, @ShopId, GETDATE(), GETDATE());
GO

-- Seed Product Images
DECLARE @ProductId1 BIGINT, @ProductId2 BIGINT, @ProductId3 BIGINT, @ProductId4 BIGINT, @ProductId5 BIGINT;
SELECT TOP 1 @ProductId1 = Id FROM Product WHERE Name = N'Cơm trưa đặc biệt';
SELECT TOP 1 @ProductId2 = Id FROM Product WHERE Name = N'Cơm gà nướng mật ong';
SELECT TOP 1 @ProductId3 = Id FROM Product WHERE Name = N'Phở bò tái chín';
SELECT TOP 1 @ProductId4 = Id FROM Product WHERE Name = N'Bún chả Hà Nội';
SELECT TOP 1 @ProductId5 = Id FROM Product WHERE Name = N'Bánh mì thịt nướng';

INSERT INTO ProductImage (ProductId, ImageUrl, IsPrimary, CreatedAt) VALUES
(@ProductId1, '/uploads/products/com-trua-dac-biet-1.jpg', 1, GETDATE()),
(@ProductId1, '/uploads/products/com-trua-dac-biet-2.jpg', 0, GETDATE()),
(@ProductId2, '/uploads/products/com-ga-nuong-1.jpg', 1, GETDATE()),
(@ProductId2, '/uploads/products/com-ga-nuong-2.jpg', 0, GETDATE()),
(@ProductId3, '/uploads/products/pho-bo-1.jpg', 1, GETDATE()),
(@ProductId3, '/uploads/products/pho-bo-2.jpg', 0, GETDATE()),
(@ProductId4, '/uploads/products/bun-cha-1.jpg', 1, GETDATE()),
(@ProductId4, '/uploads/products/bun-cha-2.jpg', 0, GETDATE()),
(@ProductId5, '/uploads/products/banh-mi-1.jpg', 1, GETDATE()),
(@ProductId5, '/uploads/products/banh-mi-2.jpg', 0, GETDATE());

DECLARE @ProductId6 BIGINT, @ProductId7 BIGINT, @ProductId8 BIGINT, @ProductId9 BIGINT, @ProductId10 BIGINT;
SELECT TOP 1 @ProductId6 = Id FROM Product WHERE Name = N'Bún bò Huế';
SELECT TOP 1 @ProductId7 = Id FROM Product WHERE Name = N'Cơm chay nấm';
SELECT TOP 1 @ProductId8 = Id FROM Product WHERE Name = N'Trái cây tươi mix';
SELECT TOP 1 @ProductId9 = Id FROM Product WHERE Name = N'Cà phê đá';
SELECT TOP 1 @ProductId10 = Id FROM Product WHERE Name = N'Combo trưa văn phòng';

INSERT INTO ProductImage (ProductId, ImageUrl, IsPrimary, CreatedAt) VALUES
(@ProductId6, '/uploads/products/bun-bo-hue-1.jpg', 1, GETDATE()),
(@ProductId7, '/uploads/products/com-chay-nam-1.jpg', 1, GETDATE()),
(@ProductId8, '/uploads/products/trai-cay-mix-1.jpg', 1, GETDATE()),
(@ProductId9, '/uploads/products/ca-phe-da-1.jpg', 1, GETDATE()),
(@ProductId10, '/uploads/products/combo-van-phong-1.jpg', 1, GETDATE());
GO

-- Seed Reviews với tiếng Việt
DECLARE @UserId BIGINT;
SELECT TOP 1 @UserId = Id FROM users WHERE user_login = 'testuser' OR user_email = 'testuser@test.com' OR Role = 'User';
IF @UserId IS NULL
BEGIN
    INSERT INTO users (user_login, user_pass, user_email, user_registered, user_status, display_name, EmailConfirmed, Role)
    VALUES ('testuser', '$2a$11$KIXQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 'testuser@test.com', GETDATE(), 1, N'Người dùng thử nghiệm', 1, 'User');
    SET @UserId = SCOPE_IDENTITY();
END

DECLARE @ProductId1 BIGINT, @ProductId2 BIGINT, @ProductId3 BIGINT, @ProductId4 BIGINT, @ProductId5 BIGINT;
SELECT TOP 1 @ProductId1 = Id FROM Product WHERE Name = N'Cơm trưa đặc biệt';
SELECT TOP 1 @ProductId2 = Id FROM Product WHERE Name = N'Cơm gà nướng mật ong';
SELECT TOP 1 @ProductId3 = Id FROM Product WHERE Name = N'Phở bò tái chín';
SELECT TOP 1 @ProductId4 = Id FROM Product WHERE Name = N'Bún chả Hà Nội';
SELECT TOP 1 @ProductId5 = Id FROM Product WHERE Name = N'Bánh mì thịt nướng';

IF @UserId IS NOT NULL
BEGIN
    INSERT INTO Review (ProductId, UserId, Rating, Comment, CreatedAt) VALUES
    (@ProductId1, @UserId, 5, N'Món ăn rất ngon, giá cả hợp lý! Thịt kho mềm, canh chua đậm đà. Sẽ quay lại!', GETDATE()),
    (@ProductId1, @UserId, 4, N'Ngon nhưng hơi mặn một chút. Rau xào tươi ngon. Đáng giá tiền.', DATEADD(day, -1, GETDATE())),
    (@ProductId2, @UserId, 5, N'Gà nướng mật ong thơm lừng, cơm dẻo. Món ăn được yêu thích nhất!', GETDATE()),
    (@ProductId2, @UserId, 5, N'Xuất sắc! Gà mềm, thơm, nước sốt đặc biệt. Rất đáng thử!', DATEADD(day, -2, GETDATE())),
    (@ProductId3, @UserId, 5, N'Phở rất đậm đà, nước dùng ngon. Thịt bò tươi, rau thơm đầy đủ.', GETDATE()),
    (@ProductId3, @UserId, 4, N'Tốt nhưng cần thêm rau thơm. Nước dùng đậm đà, thịt bò mềm.', DATEADD(day, -3, GETDATE())),
    (@ProductId4, @UserId, 5, N'Bún chả Hà Nội đúng vị! Thịt nướng thơm, nước mắm pha chuẩn.', GETDATE()),
    (@ProductId4, @UserId, 4, N'Nguyên liệu tươi, nhưng hơi ít thịt. Nước mắm ngon.', DATEADD(day, -4, GETDATE())),
    (@ProductId5, @UserId, 5, N'Bánh mì giòn, thịt nướng thơm lừng. Pate ngon, rau củ tươi.', GETDATE()),
    (@ProductId5, @UserId, 5, N'Bữa sáng hoàn hảo! Bánh mì giòn tan, thịt nướng đậm đà.', DATEADD(day, -5, GETDATE()));
END
GO

-- Seed Cart and Cart Items (for test user)
DECLARE @UserId BIGINT;
SELECT TOP 1 @UserId = Id FROM users WHERE user_login = 'testuser' OR user_email = 'testuser@test.com' OR Role = 'User';

IF @UserId IS NOT NULL
BEGIN
    DECLARE @CartId BIGINT;
    
    IF NOT EXISTS (SELECT 1 FROM Cart WHERE UserId = @UserId AND Status = 'Active')
    BEGIN
        INSERT INTO Cart (UserId, Status, CreatedAt) VALUES (@UserId, 'Active', GETDATE());
        SET @CartId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        SELECT @CartId = Id FROM Cart WHERE UserId = @UserId AND Status = 'Active';
    END
    
    IF @CartId IS NOT NULL
    BEGIN
        DECLARE @ProductId1 BIGINT, @ProductId2 BIGINT, @ProductId3 BIGINT;
        SELECT TOP 1 @ProductId1 = Id FROM Product WHERE Name = N'Cơm trưa đặc biệt';
        SELECT TOP 1 @ProductId2 = Id FROM Product WHERE Name = N'Phở bò tái chín';
        SELECT TOP 1 @ProductId3 = Id FROM Product WHERE Name = N'Bánh mì thịt nướng';
        
        INSERT INTO CartItem (CartId, ProductId, Quantity, Price, CreatedAt, IsSelected) VALUES
        (@CartId, @ProductId1, 2, 45000, GETDATE(), 1),
        (@CartId, @ProductId2, 1, 60000, GETDATE(), 1),
        (@CartId, @ProductId3, 3, 30000, GETDATE(), 0);
    END
END
GO

-- Display summary
SELECT 'Categories' as TableName, COUNT(*) as Count FROM Category
UNION ALL
SELECT 'Products', COUNT(*) FROM Product
UNION ALL
SELECT 'ProductImages', COUNT(*) FROM ProductImage
UNION ALL
SELECT 'Reviews', COUNT(*) FROM Review
UNION ALL
SELECT 'Carts', COUNT(*) FROM Cart
UNION ALL
SELECT 'CartItems', COUNT(*) FROM CartItem;
GO


-- Clear existing data
SET QUOTED_IDENTIFIER ON;
GO

DELETE FROM Review;
DELETE FROM ProductImage;
DELETE FROM CartItem;
DELETE FROM Cart;
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM Product;
DELETE FROM Category;
GO

-- Seed Categories theo menu Shop
INSERT INTO Category (Name, CreatedAt) VALUES
(N'Món ăn mặn', GETDATE()),
(N'Món ăn chay', GETDATE()),
(N'Món ăn kiêng', GETDATE()),
(N'Trái cây', GETDATE()),
(N'Đồ uống', GETDATE()),
(N'Combo', GETDATE()),
(N'Giảm giá', GETDATE());
GO

-- Get ShopId and Category IDs, then insert Products
DECLARE @ShopId BIGINT;
SELECT TOP 1 @ShopId = Id FROM users WHERE Role = 'Admin' OR Role = 'User';
IF @ShopId IS NULL
BEGIN
    INSERT INTO users (user_login, user_pass, user_email, user_registered, user_status, display_name, EmailConfirmed, Role)
    VALUES ('admin', '$2a$11$KIXQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 'admin@happylunch.com', GETDATE(), 1, N'Admin', 1, 'Admin');
    SET @ShopId = SCOPE_IDENTITY();
END

DECLARE @CategoryMan BIGINT, @CategoryChay BIGINT, @CategoryKieng BIGINT, @CategoryTraiCay BIGINT, @CategoryDoUong BIGINT, @CategoryCombo BIGINT, @CategoryGiamGia BIGINT;
SELECT TOP 1 @CategoryMan = Id FROM Category WHERE Name = N'Món ăn mặn';
SELECT TOP 1 @CategoryChay = Id FROM Category WHERE Name = N'Món ăn chay';
SELECT TOP 1 @CategoryKieng = Id FROM Category WHERE Name = N'Món ăn kiêng';
SELECT TOP 1 @CategoryTraiCay = Id FROM Category WHERE Name = N'Trái cây';
SELECT TOP 1 @CategoryDoUong = Id FROM Category WHERE Name = N'Đồ uống';
SELECT TOP 1 @CategoryCombo = Id FROM Category WHERE Name = N'Combo';
SELECT TOP 1 @CategoryGiamGia = Id FROM Category WHERE Name = N'Giảm giá';

-- Món ăn mặn
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Cơm trưa đặc biệt', N'Cơm trưa với thịt kho tàu, canh chua cá lóc và rau xào tổng hợp. Đầy đủ dinh dưỡng cho bữa trưa.', @CategoryMan, 50000, 45000, 'InStock', 100, @ShopId, GETDATE(), GETDATE()),
(N'Cơm gà nướng mật ong', N'Cơm với gà nướng mật ong thơm lừng, rau củ tươi và nước sốt đặc biệt. Món ăn được yêu thích nhất.', @CategoryMan, 55000, 50000, 'InStock', 90, @ShopId, GETDATE(), GETDATE()),
(N'Cơm tấm sườn bì chả', N'Cơm tấm truyền thống với sườn nướng, bì, chả trứng và đồ chua. Món ăn đặc trưng miền Nam.', @CategoryMan, 48000, 45000, 'InStock', 85, @ShopId, GETDATE(), GETDATE()),
(N'Bún chả Hà Nội', N'Bún chả Hà Nội đặc biệt với thịt nướng than hoa, nước mắm pha chua ngọt và rau sống tươi.', @CategoryMan, 45000, NULL, 'InStock', 70, @ShopId, GETDATE(), GETDATE()),
(N'Bún bò Huế', N'Bún bò Huế cay nồng với thịt bò, giò heo, chả cua và rau thơm. Hương vị đậm đà đặc trưng xứ Huế.', @CategoryMan, 60000, 55000, 'InStock', 65, @ShopId, GETDATE(), GETDATE()),
(N'Phở bò tái chín', N'Phở bò truyền thống với nước dùng đậm đà, thịt bò tái chín, hành lá và rau thơm. Món ăn quốc dân.', @CategoryMan, 60000, NULL, 'InStock', 80, @ShopId, GETDATE(), GETDATE()),
(N'Bánh mì thịt nướng', N'Bánh mì giòn tan với thịt nướng thơm lừng, pate, chả lụa và rau củ tươi. Bữa sáng hoàn hảo.', @CategoryMan, 35000, 30000, 'InStock', 120, @ShopId, GETDATE(), GETDATE()),
(N'Bánh cuốn nóng', N'Bánh cuốn nóng với nhân thịt, chả lụa, nước mắm pha và rau thơm. Món ăn sáng nhẹ nhàng.', @CategoryMan, 40000, NULL, 'InStock', 60, @ShopId, GETDATE(), GETDATE()),
(N'Cơm rang dưa bò', N'Cơm rang thơm ngon với dưa bò chua ngọt, trứng và hành lá. Món ăn đơn giản nhưng hấp dẫn.', @CategoryMan, 42000, 38000, 'InStock', 75, @ShopId, GETDATE(), GETDATE()),
(N'Bún riêu cua', N'Bún riêu cua đậm đà với riêu cua, thịt bò, đậu phụ và rau sống. Món canh chua đặc biệt.', @CategoryMan, 50000, 45000, 'InStock', 55, @ShopId, GETDATE(), GETDATE());

-- Món ăn chay
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Cơm chay nấm', N'Cơm chay với nấm xào, đậu phụ chiên và rau củ tươi. Món ăn thanh đạm, tốt cho sức khỏe.', @CategoryChay, 45000, 40000, 'InStock', 50, @ShopId, GETDATE(), GETDATE()),
(N'Bún chay đậu hủ', N'Bún chay với đậu hủ chiên, nấm, rau củ và nước dùng chay thanh đạm. Món ăn chay ngon miệng.', @CategoryChay, 40000, NULL, 'InStock', 45, @ShopId, GETDATE(), GETDATE()),
(N'Phở chay', N'Phở chay với nước dùng rau củ đậm đà, đậu phụ, nấm và rau thơm. Món chay đầy đủ dinh dưỡng.', @CategoryChay, 50000, 45000, 'InStock', 40, @ShopId, GETDATE(), GETDATE()),
(N'Salad chay trộn', N'Salad chay với rau xanh tươi, đậu phụ, hạt điều và sốt chay đặc biệt. Món ăn thanh mát.', @CategoryChay, 35000, 30000, 'InStock', 60, @ShopId, GETDATE(), GETDATE()),
(N'Cơm chay thập cẩm', N'Cơm chay với nhiều món: đậu phụ, nấm, rau củ xào và canh chua chay. Đầy đủ dinh dưỡng.', @CategoryChay, 48000, 43000, 'InStock', 35, @ShopId, GETDATE(), GETDATE());

-- Món ăn kiêng
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Salad rau xanh kiêng', N'Salad với rau xanh tươi, ức gà luộc, trứng và sốt kiêng đặc biệt. Phù hợp người ăn kiêng.', @CategoryKieng, 35000, 30000, 'InStock', 55, @ShopId, GETDATE(), GETDATE()),
(N'Cơm gà luộc kiêng', N'Cơm gạo lứt với ức gà luộc, rau củ hấp và sốt kiêng. Món ăn ít calo, giàu protein.', @CategoryKieng, 50000, 45000, 'InStock', 40, @ShopId, GETDATE(), GETDATE()),
(N'Bún gạo lứt kiêng', N'Bún gạo lứt với thịt bò nạc, rau củ và nước dùng thanh đạm. Món ăn kiêng đầy đủ dinh dưỡng.', @CategoryKieng, 45000, 40000, 'InStock', 30, @ShopId, GETDATE(), GETDATE()),
(N'Salad cá hồi kiêng', N'Salad với cá hồi nướng, rau xanh và sốt kiêng. Món ăn giàu omega-3, tốt cho sức khỏe.', @CategoryKieng, 65000, 60000, 'InStock', 25, @ShopId, GETDATE(), GETDATE());

-- Trái cây
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Trái cây tươi mix', N'Đĩa trái cây tươi mix với dưa hấu, dứa, xoài, thanh long và nho. Trái cây tươi ngon mỗi ngày.', @CategoryTraiCay, 30000, 25000, 'InStock', 100, @ShopId, GETDATE(), GETDATE()),
(N'Dưa hấu tươi', N'Dưa hấu tươi ngọt mát, cắt sẵn. Giải nhiệt mùa hè, giàu vitamin và khoáng chất.', @CategoryTraiCay, 20000, NULL, 'InStock', 80, @ShopId, GETDATE(), GETDATE()),
(N'Xoài chín cây', N'Xoài chín cây ngọt ngào, thơm lừng. Trái cây nhiệt đới đầy vitamin C.', @CategoryTraiCay, 25000, 22000, 'InStock', 60, @ShopId, GETDATE(), GETDATE()),
(N'Dứa tươi cắt sẵn', N'Dứa tươi cắt sẵn, ngọt mát. Trái cây giàu enzyme, tốt cho tiêu hóa.', @CategoryTraiCay, 18000, 15000, 'InStock', 70, @ShopId, GETDATE(), GETDATE()),
(N'Cam tươi vắt', N'Nước cam tươi vắt, không đường. Giàu vitamin C, tăng cường sức đề kháng.', @CategoryTraiCay, 25000, NULL, 'InStock', 90, @ShopId, GETDATE(), GETDATE());

-- Đồ uống
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Cà phê đá', N'Cà phê đá truyền thống pha phin, đậm đà hương vị. Thức uống yêu thích của người Việt.', @CategoryDoUong, 20000, 18000, 'InStock', 150, @ShopId, GETDATE(), GETDATE()),
(N'Trà đá', N'Trà đá mát lạnh, giải nhiệt. Thức uống đơn giản nhưng tươi mát.', @CategoryDoUong, 10000, NULL, 'InStock', 300, @ShopId, GETDATE(), GETDATE()),
(N'Nước ngọt', N'Coca Cola, Pepsi, 7Up, Sprite. Nước ngọt có ga, mát lạnh.', @CategoryDoUong, 15000, NULL, 'InStock', 200, @ShopId, GETDATE(), GETDATE()),
(N'Trà sữa', N'Trà sữa thơm ngon với trân châu, thạch và topping đa dạng. Thức uống trẻ trung.', @CategoryDoUong, 35000, 30000, 'InStock', 80, @ShopId, GETDATE(), GETDATE()),
(N'Sinh tố trái cây', N'Sinh tố trái cây tươi mix, không đường. Giàu vitamin, tốt cho sức khỏe.', @CategoryDoUong, 30000, 25000, 'InStock', 60, @ShopId, GETDATE(), GETDATE()),
(N'Nước ép cam', N'Nước ép cam tươi, không đường. Giàu vitamin C, tăng cường miễn dịch.', @CategoryDoUong, 25000, 22000, 'InStock', 70, @ShopId, GETDATE(), GETDATE()),
(N'Cà phê sữa đá', N'Cà phê sữa đá pha phin, ngọt ngào. Thức uống yêu thích buổi sáng.', @CategoryDoUong, 22000, 20000, 'InStock', 120, @ShopId, GETDATE(), GETDATE());

-- Combo
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Combo trưa văn phòng', N'Combo gồm: Cơm trưa đặc biệt + Canh + Đồ uống. Bữa trưa đầy đủ cho dân văn phòng.', @CategoryCombo, 65000, 58000, 'InStock', 50, @ShopId, GETDATE(), GETDATE()),
(N'Combo sáng nhanh', N'Combo gồm: Bánh mì thịt nướng + Cà phê đá. Bữa sáng nhanh gọn, đầy năng lượng.', @CategoryCombo, 45000, 40000, 'InStock', 80, @ShopId, GETDATE(), GETDATE()),
(N'Combo chay healthy', N'Combo gồm: Cơm chay nấm + Salad chay + Nước ép. Món chay đầy đủ dinh dưỡng.', @CategoryCombo, 70000, 63000, 'InStock', 30, @ShopId, GETDATE(), GETDATE()),
(N'Combo kiêng giảm cân', N'Combo gồm: Salad kiêng + Cơm gà luộc + Trà đá. Món ăn kiêng, ít calo.', @CategoryCombo, 75000, 68000, 'InStock', 25, @ShopId, GETDATE(), GETDATE());

-- Giảm giá (sản phẩm đang sale)
INSERT INTO Product (Name, Description, CategoryId, RegularPrice, SalePrice, StockStatus, StockQuantity, ShopId, PublishedDate, CreatedAt) VALUES
(N'Cơm trưa đặc biệt - Sale', N'Cơm trưa với thịt kho tàu, canh chua và rau xào. Đang giảm giá 10%.', @CategoryGiamGia, 50000, 45000, 'InStock', 50, @ShopId, GETDATE(), GETDATE()),
(N'Bún bò Huế - Sale', N'Bún bò Huế cay nồng đặc trưng. Đang giảm giá đặc biệt.', @CategoryGiamGia, 60000, 55000, 'InStock', 40, @ShopId, GETDATE(), GETDATE()),
(N'Trà sữa - Sale', N'Trà sữa thơm ngon với topping đa dạng. Giảm giá cuối tuần.', @CategoryGiamGia, 35000, 30000, 'InStock', 60, @ShopId, GETDATE(), GETDATE());
GO

-- Seed Product Images
DECLARE @ProductId1 BIGINT, @ProductId2 BIGINT, @ProductId3 BIGINT, @ProductId4 BIGINT, @ProductId5 BIGINT;
SELECT TOP 1 @ProductId1 = Id FROM Product WHERE Name = N'Cơm trưa đặc biệt';
SELECT TOP 1 @ProductId2 = Id FROM Product WHERE Name = N'Cơm gà nướng mật ong';
SELECT TOP 1 @ProductId3 = Id FROM Product WHERE Name = N'Phở bò tái chín';
SELECT TOP 1 @ProductId4 = Id FROM Product WHERE Name = N'Bún chả Hà Nội';
SELECT TOP 1 @ProductId5 = Id FROM Product WHERE Name = N'Bánh mì thịt nướng';

INSERT INTO ProductImage (ProductId, ImageUrl, IsPrimary, CreatedAt) VALUES
(@ProductId1, '/uploads/products/com-trua-dac-biet-1.jpg', 1, GETDATE()),
(@ProductId1, '/uploads/products/com-trua-dac-biet-2.jpg', 0, GETDATE()),
(@ProductId2, '/uploads/products/com-ga-nuong-1.jpg', 1, GETDATE()),
(@ProductId2, '/uploads/products/com-ga-nuong-2.jpg', 0, GETDATE()),
(@ProductId3, '/uploads/products/pho-bo-1.jpg', 1, GETDATE()),
(@ProductId3, '/uploads/products/pho-bo-2.jpg', 0, GETDATE()),
(@ProductId4, '/uploads/products/bun-cha-1.jpg', 1, GETDATE()),
(@ProductId4, '/uploads/products/bun-cha-2.jpg', 0, GETDATE()),
(@ProductId5, '/uploads/products/banh-mi-1.jpg', 1, GETDATE()),
(@ProductId5, '/uploads/products/banh-mi-2.jpg', 0, GETDATE());

DECLARE @ProductId6 BIGINT, @ProductId7 BIGINT, @ProductId8 BIGINT, @ProductId9 BIGINT, @ProductId10 BIGINT;
SELECT TOP 1 @ProductId6 = Id FROM Product WHERE Name = N'Bún bò Huế';
SELECT TOP 1 @ProductId7 = Id FROM Product WHERE Name = N'Cơm chay nấm';
SELECT TOP 1 @ProductId8 = Id FROM Product WHERE Name = N'Trái cây tươi mix';
SELECT TOP 1 @ProductId9 = Id FROM Product WHERE Name = N'Cà phê đá';
SELECT TOP 1 @ProductId10 = Id FROM Product WHERE Name = N'Combo trưa văn phòng';

INSERT INTO ProductImage (ProductId, ImageUrl, IsPrimary, CreatedAt) VALUES
(@ProductId6, '/uploads/products/bun-bo-hue-1.jpg', 1, GETDATE()),
(@ProductId7, '/uploads/products/com-chay-nam-1.jpg', 1, GETDATE()),
(@ProductId8, '/uploads/products/trai-cay-mix-1.jpg', 1, GETDATE()),
(@ProductId9, '/uploads/products/ca-phe-da-1.jpg', 1, GETDATE()),
(@ProductId10, '/uploads/products/combo-van-phong-1.jpg', 1, GETDATE());
GO

-- Seed Reviews với tiếng Việt
DECLARE @UserId BIGINT;
SELECT TOP 1 @UserId = Id FROM users WHERE user_login = 'testuser' OR user_email = 'testuser@test.com' OR Role = 'User';
IF @UserId IS NULL
BEGIN
    INSERT INTO users (user_login, user_pass, user_email, user_registered, user_status, display_name, EmailConfirmed, Role)
    VALUES ('testuser', '$2a$11$KIXQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 'testuser@test.com', GETDATE(), 1, N'Người dùng thử nghiệm', 1, 'User');
    SET @UserId = SCOPE_IDENTITY();
END

DECLARE @ProductId1 BIGINT, @ProductId2 BIGINT, @ProductId3 BIGINT, @ProductId4 BIGINT, @ProductId5 BIGINT;
SELECT TOP 1 @ProductId1 = Id FROM Product WHERE Name = N'Cơm trưa đặc biệt';
SELECT TOP 1 @ProductId2 = Id FROM Product WHERE Name = N'Cơm gà nướng mật ong';
SELECT TOP 1 @ProductId3 = Id FROM Product WHERE Name = N'Phở bò tái chín';
SELECT TOP 1 @ProductId4 = Id FROM Product WHERE Name = N'Bún chả Hà Nội';
SELECT TOP 1 @ProductId5 = Id FROM Product WHERE Name = N'Bánh mì thịt nướng';

IF @UserId IS NOT NULL
BEGIN
    INSERT INTO Review (ProductId, UserId, Rating, Comment, CreatedAt) VALUES
    (@ProductId1, @UserId, 5, N'Món ăn rất ngon, giá cả hợp lý! Thịt kho mềm, canh chua đậm đà. Sẽ quay lại!', GETDATE()),
    (@ProductId1, @UserId, 4, N'Ngon nhưng hơi mặn một chút. Rau xào tươi ngon. Đáng giá tiền.', DATEADD(day, -1, GETDATE())),
    (@ProductId2, @UserId, 5, N'Gà nướng mật ong thơm lừng, cơm dẻo. Món ăn được yêu thích nhất!', GETDATE()),
    (@ProductId2, @UserId, 5, N'Xuất sắc! Gà mềm, thơm, nước sốt đặc biệt. Rất đáng thử!', DATEADD(day, -2, GETDATE())),
    (@ProductId3, @UserId, 5, N'Phở rất đậm đà, nước dùng ngon. Thịt bò tươi, rau thơm đầy đủ.', GETDATE()),
    (@ProductId3, @UserId, 4, N'Tốt nhưng cần thêm rau thơm. Nước dùng đậm đà, thịt bò mềm.', DATEADD(day, -3, GETDATE())),
    (@ProductId4, @UserId, 5, N'Bún chả Hà Nội đúng vị! Thịt nướng thơm, nước mắm pha chuẩn.', GETDATE()),
    (@ProductId4, @UserId, 4, N'Nguyên liệu tươi, nhưng hơi ít thịt. Nước mắm ngon.', DATEADD(day, -4, GETDATE())),
    (@ProductId5, @UserId, 5, N'Bánh mì giòn, thịt nướng thơm lừng. Pate ngon, rau củ tươi.', GETDATE()),
    (@ProductId5, @UserId, 5, N'Bữa sáng hoàn hảo! Bánh mì giòn tan, thịt nướng đậm đà.', DATEADD(day, -5, GETDATE()));
END
GO

-- Seed Cart and Cart Items (for test user)
DECLARE @UserId BIGINT;
SELECT TOP 1 @UserId = Id FROM users WHERE user_login = 'testuser' OR user_email = 'testuser@test.com' OR Role = 'User';

IF @UserId IS NOT NULL
BEGIN
    DECLARE @CartId BIGINT;
    
    IF NOT EXISTS (SELECT 1 FROM Cart WHERE UserId = @UserId AND Status = 'Active')
    BEGIN
        INSERT INTO Cart (UserId, Status, CreatedAt) VALUES (@UserId, 'Active', GETDATE());
        SET @CartId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        SELECT @CartId = Id FROM Cart WHERE UserId = @UserId AND Status = 'Active';
    END
    
    IF @CartId IS NOT NULL
    BEGIN
        DECLARE @ProductId1 BIGINT, @ProductId2 BIGINT, @ProductId3 BIGINT;
        SELECT TOP 1 @ProductId1 = Id FROM Product WHERE Name = N'Cơm trưa đặc biệt';
        SELECT TOP 1 @ProductId2 = Id FROM Product WHERE Name = N'Phở bò tái chín';
        SELECT TOP 1 @ProductId3 = Id FROM Product WHERE Name = N'Bánh mì thịt nướng';
        
        INSERT INTO CartItem (CartId, ProductId, Quantity, Price, CreatedAt, IsSelected) VALUES
        (@CartId, @ProductId1, 2, 45000, GETDATE(), 1),
        (@CartId, @ProductId2, 1, 60000, GETDATE(), 1),
        (@CartId, @ProductId3, 3, 30000, GETDATE(), 0);
    END
END
GO

-- Display summary
SELECT 'Categories' as TableName, COUNT(*) as Count FROM Category
UNION ALL
SELECT 'Products', COUNT(*) FROM Product
UNION ALL
SELECT 'ProductImages', COUNT(*) FROM ProductImage
UNION ALL
SELECT 'Reviews', COUNT(*) FROM Review
UNION ALL
SELECT 'Carts', COUNT(*) FROM Cart
UNION ALL
SELECT 'CartItems', COUNT(*) FROM CartItem;
GO




