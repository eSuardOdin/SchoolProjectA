using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;



namespace MoniWatchIApI.Controllers;


[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }


    // o------------------o
    // | GET ALL ACCOUNTS |
    // o------------------o
    /// <summary>
    /// Get all accounts in DB with URL: /root/account/GetAllAccounts</br>
    /// Or all accounts from a specific user with URL: /root/account/GetAllAccounts?MoniId={id}
    /// </summary>
    /// <param name="moniId">If specified, the id of account's owner</param>
    /// <returns>An array of accounts</returns>
    [HttpGet]
    [Route("GetAllAccounts")]
    public async Task<IEnumerable<BankAccount>> GetAllAccounts(int? moniId)
    {
        using (MoniWatchIContext db = new())
        {
            return !moniId.HasValue ? await db.BankAccounts.ToArrayAsync() : await db.BankAccounts.Where(a => a.MoniId == moniId).ToArrayAsync();
        }
    }


    // o--------------------o
    // | GET UNIQUE ACCOUNT |
    // o--------------------o
    /// <summary>
    /// Get an account with URL: /root/account/GetAccount?accountId={id}
    /// </summary>
    /// <param name="accountId">The id of the account to find</param>
    /// <returns>A status code</returns>
    [HttpGet]
    [Route("GetAccount")]
    public async Task<ActionResult<BankAccount>> GetAccount(int accountId)
    {
        using (MoniWatchIContext db = new())
        {
            BankAccount acc = await db.BankAccounts.FindAsync(accountId);
            if (acc is null) 
            {
                return NotFound();
            }
            else
            {
                return Ok(acc);
            }
        }
    }


    // o------------------o
    // | POST NEW ACCOUNT |
    // o------------------o    
    /// <summary>
    /// Http POST request </br>
    /// Adds an account to the database with URL: /root/account/PostAccount
    /// </summary>
    /// <param name="account">The account to add (specified as JSON, translated by EF Core)</param>
    /// <returns>A status code</returns>
    [HttpPost]
    [Route("PostAccount")]
    public async Task<ActionResult<BankAccount>> PostAccount([FromBody] BankAccount account)
    {
        using (MoniWatchIContext db = new())
        {
            db.BankAccounts.Add(account);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccount), new {accountId = account.BankAccountId}, account);
        }
    }


    // o----------------o
    // | DELETE ACCOUNT |
    // o----------------o
    [HttpDelete]
    [Route("DeleteAccount")]
    public async Task<ActionResult> DeleteAccount(int accountId)
    {
        using (MoniWatchIContext db = new())
        {
            BankAccount acc = await db.BankAccounts.FindAsync(accountId);
            if (acc is null)
            {
                return BadRequest("Account not found");
            }

            db.BankAccounts.Remove(acc);
            await db.SaveChangesAsync();
            return Ok($"Deleted {acc.BankAccountLabel}");
        }
    }


    // o---------------o
    // | PATCH ACCOUNT |
    // o---------------o
    [HttpPatch]
    [Route("UpdateAccountName")]
    public async Task<ActionResult<Transaction>> UpdateAccountName(string accountLabel, int accountId)
    {
        using (MoniWatchIContext db = new())
        {
            BankAccount acc = await db.BankAccounts.FindAsync(accountId);
            if (acc is null)
            {
                return BadRequest("Account not found");
            }
            acc.BankAccountLabel = accountLabel;
            db.SaveChangesAsync();

            return Ok($"Account is now named {acc.BankAccountLabel}");
        }
    } 


}