﻿using System;
using System.Collections.Generic;



namespace MoniWatchIApI;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int BankAccountId { get; set; }

    public decimal TransactionAmount { get; set; }

    public DateTime TransactionDate { get; set; }

    public string TransactionLabel { get; set; } = null!;

    public string? TransactionDescription { get; set; }

    public virtual BankAccount? BankAccount { get; set; } = null;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
