using System;
using System.Collections.Generic;

namespace HappyLunchBE.Models;

public partial class VnpayTransaction
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public string ResponseCode { get; set; } = null!;

    public string TransactionNo { get; set; } = null!;

    public string? BankCode { get; set; }

    public decimal? Amount { get; set; }

    public string? OrderInfo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
