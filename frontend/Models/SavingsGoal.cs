namespace frontend.Models;

public class SavingsGoal
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public double TargetAmount { get; set; }
    public double CurrentAmount { get; set; }
    public string TargetDate { get; set; } = ""; // Format: yyyy-MM-dd
    public string Description { get; set; } = "";
}