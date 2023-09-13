using System;
using System.Collections.Generic;

namespace TestEFCoreMySql;

public partial class Moni
{
    public int MoniId { get; set; }

    public string MoniLogin { get; set; } = null!;

    public string MoniPwd { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
