using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
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
    // o--------------------o
    // | UPDATE TRANSACTION |
    // o--------------------o
    /// <summary>
    /// Update transaction amount with an Http PATCH request</br>
    /// on URL: /root/transaction/{id}</br>
    /// Updates account 
    /// </summary>
    /// <param name="transactionId">Id of the transaction to patch</param>
    /// <param name="transactionDto">New transaction</param>
    /// <returns>Status code</returns>
    [HttpPatch]
    [Route("{transactionId}")]
    public async Task<ActionResult<Transaction>> UpdateTransaction(int transactionId, [FromBody]TransactionDto transactionDto)
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
            // // Link all tags
            // List<Tag> tags = new();
            // foreach(int tagId in transactionDto.Tags)
            // {
            //     Tag tag = await db.Tags.FindAsync(tagId);
            //     tags.Add(tag);
            // }
            transaction.TransactionLabel = transactionDto.TransactionLabel;
            transaction.TransactionDate = transactionDto.TransactionDate;
            transaction.TransactionAmount = transactionDto.TransactionAmount;
            transaction.TransactionDescription = transactionDto.TransactionDescription;
            await db.SaveChangesAsync();

            return Ok($"Transaction '{transaction.TransactionLabel} modified'");
        }
    }
    
//-------------------------------------------------------------------------------------------------------
    // OWNED OBJECTS

    [HttpPatch]
    [Route("{transactionId}/tag/{tagId}")]
    public async Task<ActionResult<Transaction>> AddTag(int transactionId, int tagId)
    {
        using(MoniWatchIContext db = new())
        {
            Transaction? transaction = await db.Transactions.FindAsync(transactionId);
            Tag? tag = await db.Tags.FindAsync(tagId);
            if(transaction is null) return BadRequest("This transaction does not exists");
            if(tag is null) return BadRequest("This tag does not exists");
            transaction.Tags.Add(tag);
            
            await db.SaveChangesAsync();
            return Ok($"Transaction '{transaction.TransactionLabel}' modified");
        }
    }


// USING ADO.NET
    [HttpDelete]
    [Route("{transactionId}/tag/{tagId}")]
    public async Task<ActionResult<Transaction>> RemoveTag(int transactionId, int tagId)
    {
        using(MoniWatchIContext db = new())
        {
            Transaction? transaction = await db.Transactions.FindAsync(transactionId);
            Tag? tag = await db.Tags.FindAsync(tagId);
            if(transaction is null) return BadRequest("This transaction does not exists");
            if(tag is null) return BadRequest("This tag does not exists");
            transaction.Tags.Remove(tag);

            
            await db.SaveChangesAsync();
            //return Ok($"Transaction '{transaction.TransactionLabel} modified'");
        }
        var connectionString = "server=localhost;user=wan;password=CnamOcc34!;database=MoniWatchI";
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        using(var cmd = new MySqlCommand())
        {
            cmd.Connection = connection;
            cmd.CommandText = "DELETE FROM Tags_Transactions WHERE TransactionId = (@tran) AND TagId = (@tag)";
            cmd.Parameters.AddWithValue("tran", transactionId);
            cmd.Parameters.AddWithValue("tag", tagId);
            await cmd.ExecuteNonQueryAsync();
            return Ok($"Tag {tagId} deleted from transaction {transactionId}");
        }
    }

}