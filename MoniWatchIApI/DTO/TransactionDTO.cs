namespace MoniWatchIApI.DTO;
public class TransactionDto
{
    public int TransactionId { get; set; }
    public int BankAccountId { get; set; }
    public decimal TransactionAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionLabel { get; set; }
    public string TransactionDescription { get; set; }
    public int[] Tags { get; set; }
}