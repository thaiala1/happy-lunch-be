using System;
using System.Collections.Generic;

namespace HappyLunchBE.Models;

public partial class Product
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public long CategoryId { get; set; }

    public decimal RegularPrice { get; set; }

    public decimal? SalePrice { get; set; }

    public string StockStatus { get; set; } = null!;

    public double? StockQuantity { get; set; }

    public long ShopId { get; set; }

    public DateTime PublishedDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User Shop { get; set; } = null!;
}
