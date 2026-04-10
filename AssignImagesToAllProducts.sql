USE [happylunch]
GO

SET QUOTED_IDENTIFIER ON;
GO

-- Script để gán ảnh cho TẤT CẢ sản phẩm
-- Mỗi sản phẩm sẽ có ít nhất 1 ảnh, dù trùng nhau cũng được

-- Xóa tất cả ảnh cũ (nếu muốn reset)
-- DELETE FROM ProductImage;
-- GO

-- Danh sách ảnh có sẵn (sẽ dùng lại cho nhiều sản phẩm)
DECLARE @ImageList TABLE (
    ImageUrl NVARCHAR(500),
    ImageOrder INT
);

INSERT INTO @ImageList (ImageUrl, ImageOrder) VALUES
('/uploads/products/com-trua-dac-biet-1.jpg', 1),
('/uploads/products/com-ga-nuong-1.jpg', 2),
('/uploads/products/pho-bo-1.jpg', 3),
('/uploads/products/bun-cha-1.jpg', 4),
('/uploads/products/banh-mi-1.jpg', 5),
('/uploads/products/bun-bo-hue-1.jpg', 6),
('/uploads/products/com-chay-nam-1.jpg', 7),
('/uploads/products/trai-cay-mix-1.jpg', 8),
('/uploads/products/ca-phe-da-1.jpg', 9),
('/uploads/products/combo-van-phong-1.jpg', 10);

-- Lấy tất cả sản phẩm chưa có ảnh hoặc cần thêm ảnh
DECLARE @ProductsWithoutImages TABLE (
    ProductId BIGINT,
    ProductName NVARCHAR(255),
    RowNum INT
);

-- Insert tất cả sản phẩm
INSERT INTO @ProductsWithoutImages (ProductId, ProductName, RowNum)
SELECT Id, Name, ROW_NUMBER() OVER (ORDER BY Id) 
FROM Product;

-- Gán ảnh cho từng sản phẩm
DECLARE @ProductId BIGINT;
DECLARE @ProductName NVARCHAR(255);
DECLARE @ImageUrl NVARCHAR(500);
DECLARE @ImageIndex INT;
DECLARE @TotalImages INT = (SELECT COUNT(*) FROM @ImageList);
DECLARE @HasImage BIT;

DECLARE product_cursor CURSOR FOR
SELECT ProductId, ProductName FROM @ProductsWithoutImages;

OPEN product_cursor;
FETCH NEXT FROM product_cursor INTO @ProductId, @ProductName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Kiểm tra xem sản phẩm đã có ảnh chưa
    SELECT @HasImage = CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
    FROM ProductImage
    WHERE ProductId = @ProductId;
    
    -- Nếu chưa có ảnh, gán ảnh cho sản phẩm
    IF @HasImage = 0
    BEGIN
        -- Chọn ảnh theo vòng lặp (1-10, rồi lặp lại)
        SET @ImageIndex = ((SELECT RowNum FROM @ProductsWithoutImages WHERE ProductId = @ProductId) % @TotalImages) + 1;
        IF @ImageIndex = 0 SET @ImageIndex = @TotalImages;
        
        SELECT @ImageUrl = ImageUrl 
        FROM @ImageList 
        WHERE ImageOrder = @ImageIndex;
        
        -- Insert ảnh chính
        INSERT INTO ProductImage (ProductId, ImageUrl, IsPrimary, CreatedAt)
        VALUES (@ProductId, @ImageUrl, 1, GETDATE());
        
        PRINT N'Đã gán ảnh cho sản phẩm: ' + @ProductName + N' - ' + @ImageUrl;
    END
    ELSE
    BEGIN
        PRINT N'Sản phẩm đã có ảnh: ' + @ProductName;
    END
    
    FETCH NEXT FROM product_cursor INTO @ProductId, @ProductName;
END

CLOSE product_cursor;
DEALLOCATE product_cursor;

-- Kiểm tra kết quả
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    COUNT(pi.Id) AS ImageCount
FROM Product p
LEFT JOIN ProductImage pi ON p.Id = pi.ProductId
GROUP BY p.Id, p.Name
ORDER BY p.Id;

PRINT N'Hoàn thành! Tất cả sản phẩm đã có ảnh.';
GO




