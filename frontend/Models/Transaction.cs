namespace frontend.Models;

public class Transaction
{
    public long Id { get; set; }
    public string Description { get; set; } = "";
    public double Amount { get; set; }
    public string Type { get; set; } = "";
    public string Category { get; set; } = "";
    public string Date { get; set; } = "";
}

public class Summary
{
    public double Income { get; set; }
    public double Expense { get; set; }
    public double Balance { get; set; }
}

public class MonthlyData
{
    public string Month { get; set; } = "";
    public double Income { get; set; }
    public double Expense { get; set; }
}

public class TransactionPage
{
    public List<Transaction>? Content { get; set; }
    public int Number { get; set; }
    public int Size { get; set; }
    public long TotalElements { get; set; }
    public int TotalPages { get; set; }
    public bool First { get; set; }
    public bool Last { get; set; }
}