using Microsoft.EntityFrameworkCore;
using TestEFCoreMySql;

using (var context = new MoniWatchIContext())
{
    var accounts = context.BankAccounts.Include(a => a.Moni).ToList();
    foreach(var account in accounts) Console.WriteLine($"Le compte {account.BankAccountLabel} appartient à {account.Moni.MoniLogin}");
}
