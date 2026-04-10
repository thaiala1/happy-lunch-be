using System;
using System.Collections.Generic;

namespace HappyLunchBE.Models;

public partial class Order
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string DeliveryBranch { get; set; } = null!;

    public string DeliveryTimeCode { get; set; } = null!;

    public string? Note { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public long TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string OrderStatus { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<VnpayTransaction> VnpayTransactions { get; set; } = new List<VnpayTransaction>();
}
