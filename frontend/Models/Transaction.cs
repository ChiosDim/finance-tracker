namespace frontend.Models;

public class Transaction
{
    public long Id { get; set; }
    public string Description { get; set; } = "";
    public double Amount { get; set; }
    public string Type { get; set; } = "";      // "INCOME" or "EXPENSE"
    public string Category { get; set; } = "";
    public string Date { get; set; } = "";
}

public class Summary
{
    public double Income { get; set; }
    public double Expense { get; set; }
    public double Balance { get; set; }
}