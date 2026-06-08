namespace frontend.Models;

public class Budget
{
    public long Id { get; set; }
    public string Category { get; set; } = "";
    public double Amount { get; set; }
    public string Month { get; set; } = ""; // Format: yyyy-MM
}