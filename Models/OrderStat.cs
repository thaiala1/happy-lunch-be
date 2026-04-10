using System;
using System.Collections.Generic;

namespace HappyLunchBE.Models;

public partial class OrderStat
{
    public long OrderId { get; set; }

    public long ParentId { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateCreatedGmt { get; set; }

    public DateTime? DatePaid { get; set; }

    public DateTime? DateCompleted { get; set; }

    public int NumItemsSold { get; set; }

    public double TotalSales { get; set; }

    public double ShippingTotal { get; set; }

    public double NetTotal { get; set; }

    public byte? ReturningCustomer { get; set; }

    public string Status { get; set; } = null!;

    public long CustomerId { get; set; }
}
