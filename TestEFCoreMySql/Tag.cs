using System;
using System.Collections.Generic;

namespace TestEFCoreMySql;

public partial class Tag
{
    public int TagId { get; set; }

    public string TagLabel { get; set; } = null!;

    public string? TagDescription { get; set; }

    public int MoniId { get; set; }

    public virtual Moni Moni { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
