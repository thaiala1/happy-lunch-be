using System;
using System.Collections.Generic;

namespace HappyLunchBE.Models;

public partial class User
{
    public string UserLogin { get; set; } = null!;

    public string UserPass { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public DateTime UserRegistered { get; set; }

    public int UserStatus { get; set; }

    public string DisplayName { get; set; } = null!;

    public long Id { get; set; }

    public string? EmailConfirmationCode { get; set; }

    public bool? EmailConfirmed { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
