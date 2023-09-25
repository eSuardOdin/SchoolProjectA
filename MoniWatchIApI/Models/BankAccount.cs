using System;
using System.Collections.Generic;

namespace MoniWatchIApI;

public partial class BankAccount
{
    public int BankAccountId { get; set; }

    public string BankAccountLabel { get; set; } = null!;

    public decimal BankAccountBalance { get; set; }

    public int MoniId { get; set; }

    public virtual Moni Moni { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
