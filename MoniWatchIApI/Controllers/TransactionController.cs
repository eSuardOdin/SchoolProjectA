using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoniWatchIApI;
using MoniWatchIApI.DTO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoniWatch.Api.Controllers;

[ApiController]
[Route("transaction")]
public class TransactionController : ControllerBase
{
    private readonly ILogger<TransactionController> _logger;
    public TransactionController(ILogger<TransactionController> logger)
    {
        _logger = logger;
    }

    // // o--------------------------------o
    // // | GET ALL TRANSACTIONS WITH DATE |
    // // o--------------------------------o

    // [HttpGet]
    // [Route("GetAllTransactionsWithDate")]
    // public async Task<IEnumerable<Transaction>> GetAllTransactionsWithDate(int accountId, DateTime startDate, DateTime? endDate, bool isAsc = true)
    // {
    //     using (MoniWatchIContext db = new())
    //     {
    //         var all = await db.Transactions.Where(t => t.BankAccountId == accountId).ToArrayAsync();
    //         if (endDate is null)
    //         {
    //             return isAsc ? 
    //                 all.Where(t => t.TransactionDate >= startDate).OrderBy(t => t.TransactionDate) :
    //                 all.Where(t => t.TransactionDate >= startDate).OrderByDescending(t => t.TransactionDate);
    //         }
    //         return isAsc ? 
    //             all.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate).OrderBy(t => t.TransactionDate) :
    //             all.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate).OrderByDescending(t => t.TransactionDate);
    //     }
    // }


    // o------------------------o
    // | GET UNIQUE TRANSACTION |
    // o------------------------o
    /// <summary>
    /// Get a transaction with URL: /root/transaction/GetTransaction?transactionId={id}
    /// </summary>
    /// <param name="transactionId">The id of transaction to find</param>
    /// <returns>A status code</returns>
    [HttpGet]
    [Route("{transactionId}")]
    public async Task<ActionResult<Transaction>> GetTransaction(int transactionId)
    {
        using(MoniWatchIContext db = new())
        {
            Transaction transaction = await db.Transactions.FindAsync(transactionId);
            if (transaction is null) 
            {
                return NotFound("Transaction not found");
            }
            else
            {
                return Ok(transaction);
            }
        }
    }


    // o----------------------o
    // | POST NEW TRANSACTION |
    // o----------------------o
    /// <summary>
    /// Posts a new transaction to database</br>
    /// Update account balance accordingly
    /// </summary>
    /// <param name="transaction">The transaction object to add (as a JSON)</param>
    /// <returns>Created object</returns>
    [HttpPost]
    public async Task<ActionResult<Transaction>> PostTransaction([FromBody]TransactionDto transactionDto)
    {
        using (MoniWatchIContext db = new())
        {
            // Create real transaction
            Transaction transaction = new();
            // Link all tags
            List<Tag> tags = new();
            foreach(int tagId in transactionDto.Tags)
            {
                Tag tag = await db.Tags.FindAsync(tagId);
                tags.Add(tag);
            }
            transaction.TransactionLabel = transactionDto.TransactionLabel;
            transaction.TransactionDate = transactionDto.TransactionDate;
            transaction.TransactionAmount = transactionDto.TransactionAmount;
            transaction.BankAccountId = transactionDto.BankAccountId;
            transaction.TransactionDescription = transactionDto.TransactionDescription;
            transaction.Tags = tags;
            // Add transaction
            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();
            // Get associated account
            BankAccount account = await db.BankAccounts.FindAsync(transaction.BankAccountId);
            if (account is null)
            {
                return NotFound("Account not found");
            }

            
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransaction), new {transactionId = transaction.TransactionId}, transaction);
        }
    }


    // o--------------------o
    // | DELETE TRANSACTION |
    // o--------------------o
    /// <summary>
    /// Deletes a transaction and updates linked account balance </br>
    /// with URL: /root/transaction/DeleteTransaction?transactionId={id}
    /// </summary>
    /// <param name="transactionId">Id of the transaction to delete</param>
    /// <returns>A status code</returns>
    [HttpDelete]
    [Route("DeleteTransaction")]
    public async Task<ActionResult> DeleteTransaction(int transactionId)
    {
        using (MoniWatchIContext db = new())
        {
            Transaction transaction = await db.Transactions.FindAsync(transactionId);
            if(transaction is null)
            {
                return BadRequest("This transaction does not exists");
            }
            // Get linked account
            BankAccount account = db.BankAccounts.Where(a => a.BankAccountId == transaction.BankAccountId).FirstOrDefault();
            if(account is null)
            {
                return BadRequest("This transaction is not linked to any account");
            }
            db.Transactions.Remove(transaction);
            await db.SaveChangesAsync();
            return Ok($"Deleted {transaction.TransactionLabel}");
        }
    }



    // DATA UPDATE
    // o---------------------------o
    // | UPDATE TRANSACTION AMOUNT |
    // o---------------------------o
    /// <summary>
    /// Update transaction amount with an Http PATCH request</br>
    /// on URL: /root/transaction/UpdateTransactionAmount?transactionId={id}&newAmout={amount}</br>
    /// Updates account balance
    /// </summary>
    /// <param name="transactionId">Id of the transaction to patch</param>
    /// <param name="newAmount">Amount of the transaction</param>
    /// <returns>Status code</returns>
    [HttpPatch]
    [Route("UpdateTransactionAmount")]
    public async Task<ActionResult<Transaction>> UpdateTransactionAmount(int transactionId, decimal newAmount)
    {
        using (MoniWatchIContext db = new())
        {
            Transaction transaction = await db.Transactions.FindAsync(transactionId);
            if(transaction is null)
            {
                return BadRequest("This transaction does not exists");
            }
            // Get linked account
            BankAccount account = db.BankAccounts.Where(a => a.BankAccountId == transaction.BankAccountId).FirstOrDefault();
            if(account is null)
            {
                return BadRequest("This transaction is not linked to any account");
            }
            // Update
            transaction.TransactionAmount = newAmount;
            await db.SaveChangesAsync();

            return Ok($"Transaction '{transaction.TransactionLabel} modified'");
        }
    }


    // o-------------------------o
    // | UPDATE TRANSACTION NAME |
    // o-------------------------o
    /// <summary>
    /// Update transaction name with an Http PATCH request</br>
    /// on URL: /root/transaction/UpdateTransactionAmount?transactionId={id}&newName={name}</br>
    /// </summary>
    /// <param name="transactionId">Id of transaction to change</param>
    /// <param name="newName">New value to assign</param>
    /// <returns>A status code</returns>
    [HttpPatch]
    [Route("UpdateTransactionName")]
    public async Task<ActionResult<Transaction>> UpdateTransactionName(int transactionId, string newName)
    {
        using (MoniWatchIContext db = new())
        {
            Transaction transaction = await db.Transactions.FindAsync(transactionId);
            if(transaction is null)
            {
                return BadRequest("This transaction does not exists");
            }
            // Update
            transaction.TransactionLabel = newName;
            await db.SaveChangesAsync();

            return Ok($"Transaction '{transaction.TransactionLabel} modified'");
        }
    }


    // o------------------------o
    // | UPDATE TRANSACTION TAG |
    // o------------------------o
    /// <summary>
    /// Change the tag of a transaction</br>
    /// Ensure the tag provided is from the user provided and assign it to the transaction
    /// </summary>
    /// <param name="transactionId">Id of the transaction to</param>
    /// <param name="newTag"></param>
    /// <param name="moniId"></param>
    /// <returns></returns>
    // [HttpPatch]
    // [Route("UpdateTransactionTag")]
    // public async Task<ActionResult<Transaction>> UpdateTransactionTag(int transactionId, int newTag, int moniId)
    // {
    //     using (MoniWatchIContext db = new())
    //     {
    //         Transaction transaction = await db.Transactions.FindAsync(transactionId);
    //         if(transaction is null)
    //         {
    //             return BadRequest("This transaction does not exists");
    //         }

    //         Tag tag = await db.Tags.FindAsync(newTag);
    //         // If tag do not exists or is not one owned by user
    //         if(tag is null || tag.MoniId != moniId)
    //         {
    //             return BadRequest("This tag does not exists");
    //         }
    //         // Update
    //         transaction.TagId = newTag;
    //         await db.SaveChangesAsync();

    //         return Ok($"Transaction '{transaction.TransactionName} modified'");
    //     }
    // }

    // o-------------------------o
    // | UPDATE TRANSACTION DATE |
    // o-------------------------o

    [HttpPatch]
    [Route("UpdateTransactionDate")]
    public async Task<ActionResult<Transaction>> UpdateTransactionDate(DateTime transactionDate, int transactionId)
    {
        using (MoniWatchIContext db = new())
        {
            Transaction transaction = await db.Transactions.FindAsync(transactionId);
            if (transaction is null)
            {
                return BadRequest ("Transaction not found");
            }
            transaction.TransactionDate = transactionDate;
            await db.SaveChangesAsync();

            return Ok($"Transaction date is now: {transaction.TransactionDate}");
        }
    }

}