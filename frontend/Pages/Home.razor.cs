using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace frontend.Pages;

public partial class Home
{
    private List<Transaction>? transactions;
    private Summary? summary;
    private Dictionary<string, double>? categories;
    private List<MonthlyData>? monthly;

    private string activeFilter = "ALL";
    private DateTime? filterFrom;
    private DateTime? filterTo;
    private string searchTerm = "";
    private string sortBy = "date";
    private bool sortDescending = true;

    // Budget and savings goal related
    private List<Budget> budgets = new();
    private List<SavingsGoal> savingsGoals = new();
    private List<Dictionary<string, object>> budgetVsActual = new();
    private string selectedMonthForBudget = ""; // Format: yyyy-MM
    private bool isLoadingBudgets = false;
    private bool isLoadingSavingsGoals = false;

    private List<BarItem> barItems = new();
    private List<TickItem> yTicks = new();
    private List<DonutSlice> donutSlices = new();
    private bool isLoading = true;
    private string loadError = "";
    private bool isExporting = false;
    private bool isClearingFilter = false;
    private long? deletingTransactionId = null;

    private IEnumerable<Transaction> FilteredTransactions
    {
        get
        {
            var filtered = (transactions ?? new List<Transaction>())
                .Where(t =>
                {
                    // Type filter
                    if (activeFilter != "ALL" && t.Type != activeFilter)
                    {
                        return false;
                    }

                    // Date range filter
                    if (filterFrom.HasValue && DateTime.TryParse(t.Date, out var df) && df < filterFrom.Value)
                    {
                        return false;
                    }

                    if (filterTo.HasValue && DateTime.TryParse(t.Date, out var dt) && dt > filterTo.Value)
                    {
                        return false;
                    }

                    // Search filter
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        var lowerSearch = searchTerm.ToLowerInvariant();
                        if (!(t.Description?.ToLowerInvariant().Contains(lowerSearch) ?? false) &&
                            !(t.Category?.ToLowerInvariant().Contains(lowerSearch) ?? false))
                        {
                            return false;
                        }
                    }

                    return true;
                });

            // Apply sorting
            return sortBy switch
            {
                "date" => sortDescending
                    ? filtered.OrderByDescending(t => DateTime.Parse(t.Date ?? "1900-01-01"))
                    : filtered.OrderBy(t => DateTime.Parse(t.Date ?? "1900-01-01")),
                "amount" => sortDescending
                    ? filtered.OrderByDescending(t => t.Amount)
                    : filtered.OrderBy(t => t.Amount),
                "description" => sortDescending
                    ? filtered.OrderByDescending(t => t.Description ?? "")
                    : filtered.OrderBy(t => t.Description ?? ""),
                "category" => sortDescending
                    ? filtered.OrderByDescending(t => t.Category ?? "")
                    : filtered.OrderBy(t => t.Category ?? ""),
                _ => sortDescending
                    ? filtered.OrderByDescending(t => DateTime.Parse(t.Date ?? "1900-01-01"))
                    : filtered.OrderBy(t => DateTime.Parse(t.Date ?? "1900-01-01"))
            };
        }
    }

    // Toast notification helper
    private async Task ShowToast(string message, string type = "info")
    {
        await JS.InvokeVoidAsync("showToast", message, type);
    }

    protected override async Task OnInitializedAsync() => await LoadData();

    private async Task LoadData()
    {
        isLoading = true;
        loadError = "";
        barItems.Clear();
        yTicks.Clear();
        donutSlices.Clear();

        try
        {
            transactions = await Http.GetFromJsonAsync<List<Transaction>>("api/transactions") ?? new();
            summary = BuildSummary(transactions);
            categories = BuildCategories(transactions);
            monthly = BuildMonthly(transactions);
            ComputeBarChart();
            ComputeDonut();

            // Clear any previous error
            loadError = "";

            // Show success toast if we have data
            if (transactions != null && transactions.Any())
            {
                await ShowToast("Transactions loaded successfully!", "success");
            }
        }
        catch (Exception ex)
        {
            transactions = new();
            summary = new Summary();
            categories = new();
            monthly = new();
            loadError = $"Could not load transactions: {ex.Message}";
            await ShowToast("Failed to load transactions. Please try again.", "error");
            return;
        }
        finally
        {
            isLoading = false;
        }
    }

    private static Summary BuildSummary(IEnumerable<Transaction> source)
    {
        var income = source
            .Where(t => string.Equals(t.Type, "INCOME", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        var expense = source
            .Where(t => string.Equals(t.Type, "EXPENSE", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        return new Summary
        {
            Income = income,
            Expense = expense,
            Balance = income - expense
        };
    }

    private static Dictionary<string, double> BuildCategories(IEnumerable<Transaction> source) =>
        source
            .Where(t => string.Equals(t.Type, "EXPENSE", StringComparison.OrdinalIgnoreCase))
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? "Uncategorized" : t.Category)
            .ToDictionary(group => group.Key, group => group.Sum(t => t.Amount));

    private static List<MonthlyData> BuildMonthly(IEnumerable<Transaction> source) =>
        source
            .Where(t => !string.IsNullOrWhiteSpace(t.Date) && t.Date.Length >= 7)
            .GroupBy(t => t.Date[..7])
            .OrderBy(group => group.Key)
            .Select(group => new MonthlyData
            {
                Month = group.Key,
                Income = group
                    .Where(t => string.Equals(t.Type, "INCOME", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount),
                Expense = group
                    .Where(t => string.Equals(t.Type, "EXPENSE", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount)
            })
            .ToList();

    private async Task Delete(long id)
    {
        deletingTransactionId = id;
        try
        {
            await Http.DeleteAsync($"api/transactions/{id}");
            await LoadData();
            await ShowToast("Transaction deleted successfully!", "success");
        }
        finally
        {
            deletingTransactionId = null;
        }
    }

    private async Task ConfirmDelete(long id)
    {
        var confirmed = await JS.InvokeAsync<bool>("confirm", "Are you sure you want to delete this transaction?");
        if (confirmed)
        {
            await Delete(id);
        }
        else
        {
            await ShowToast("Delete cancelled", "info");
        }
    }

    private void ClearDateFilter()
    {
        isClearingFilter = true;
        filterFrom = null;
        filterTo = null;
        isClearingFilter = false;
    }

    private async Task ExportCsv()
    {
        if (transactions == null || !transactions.Any())
        {
            await ShowToast("No transactions to export", "warning");
            return;
        }

        isExporting = true;
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Date,Description,Category,Type,Amount");

            foreach (var t in transactions)
            {
                sb.AppendLine($"{t.Date},{Csv(t.Description)},{Csv(t.Category)},{t.Type},{t.Amount}");
            }

            await JS.InvokeVoidAsync("downloadFile", "transactions.csv", "text/csv", sb.ToString());
            await ShowToast("Transactions exported successfully!", "success");
        }
        catch (Exception _)
        {
            await ShowToast("Failed to export transactions. Please try again.", "error");
        }
        finally
        {
            isExporting = false;
        }
    }

    private static string Csv(string value)
    {
        var quote = ((char)34).ToString();

        if (!value.Contains(',') && !value.Contains((char)34))
        {
            return value;
        }

        return quote + value.Replace(quote, quote + quote) + quote;
    }

    private static string FormatDate(string date) =>
        DateOnly.TryParse(date, out var d) ? d.ToString("dd/MM/yyyy") : date;

    private static string FormatNumber(double value, string format = "F2") =>
        value.ToString(format, CultureInfo.InvariantCulture);

    private static string SvgNumber(double value) =>
        value.ToString("F1", CultureInfo.InvariantCulture);

    private static string HtmlEncode(string value) =>
        WebUtility.HtmlEncode(value) ?? "";

    private static MarkupString SvgText(
        string x,
        string y,
        string cssClass,
        string content,
        string? textAnchor = null)
    {
        var anchorAttribute = string.IsNullOrWhiteSpace(textAnchor)
            ? ""
            : $" text-anchor=\"{HtmlEncode(textAnchor)}\"";

        var markup =
            $"\u003Ctext x=\"{HtmlEncode(x)}\" y=\"{HtmlEncode(y)}\" class=\"{HtmlEncode(cssClass)}\"{anchorAttribute}\u003E{HtmlEncode(content)}\u003C/text\u003E";

        return new MarkupString(markup);
    }

    private class BarItem
    {
        public string IncomeX = "";
        public string IncomeY = "";
        public string IncomeH = "";
        public string ExpenseX = "";
        public string ExpenseY = "";
        public string ExpenseH = "";
        public string LabelX = "";
        public string Label = "";
        public string BarW = "";
        public double IncomeValue = 0;
        public double ExpenseValue = 0;
    }

    private class TickItem
    {
        public string Y = "";
        public string LabelY = "";
        public string Label = "";
    }

    private void ComputeBarChart()
    {
        barItems.Clear();
        yTicks.Clear();

        if (monthly == null || !monthly.Any())
        {
            return;
        }

        const double bot = 175;
        const double top = 15;
        const double leftPad = 48;
        const double rightEdge = 485;
        const double chartH = bot - top;
        const double chartW = rightEdge - leftPad;

        double maxVal = monthly.Max(m => Math.Max(m.Income, m.Expense));
        if (maxVal <= 0)
        {
            maxVal = 100;
        }

        int n = monthly.Count;
        double groupW = chartW / n;
        double barW = Math.Min(groupW * 0.28, 26);
        double gap = barW * 0.3;

        for (int i = 0; i < n; i++)
        {
            var m = monthly[i];
            double gx = leftPad + i * groupW + groupW / 2;
            double iH = m.Income / maxVal * chartH;
            double eH = m.Expense / maxVal * chartH;
            string lbl = DateTime.TryParse(m.Month + "-01", out var d)
                ? d.ToString("MMM", CultureInfo.InvariantCulture)
                : m.Month;

            barItems.Add(new BarItem
            {
                IncomeX = SvgNumber(gx - barW - gap / 2),
                IncomeY = SvgNumber(bot - iH),
                IncomeH = SvgNumber(Math.Max(iH, 0)),
                ExpenseX = SvgNumber(gx + gap / 2),
                ExpenseY = SvgNumber(bot - eH),
                ExpenseH = SvgNumber(Math.Max(eH, 0)),
                LabelX = SvgNumber(gx),
                Label = lbl,
                BarW = SvgNumber(barW),
                IncomeValue = m.Income,
                ExpenseValue = m.Expense
            });
        }

        foreach (var p in new[] { 0.25, 0.5, 0.75, 1.0 })
        {
            double y = bot - p * chartH;
            double v = maxVal * p;

            yTicks.Add(new TickItem
            {
                Y = SvgNumber(y),
                LabelY = SvgNumber(y + 4),
                Label = v >= 1000
                    ? $"EUR {FormatNumber(v / 1000, "F1")}k"
                    : $"EUR {FormatNumber(v, "F0")}"
            });
        }
    }

    private class DonutSlice
    {
        public string Path = "";
        public string Color = "";
        public string Category = "";
        public string PctText = "";
        public double Amount = 0;
        public double Percentage = 0;
    }

    private void ComputeDonut()
    {
        donutSlices.Clear();

        if (categories == null || !categories.Any())
        {
            return;
        }

        var colors = new[] { "#f43f5e", "#3b82f6", "#10b981", "#f59e0b", "#8b5cf6", "#ec4899", "#06b6d4", "#84cc16" };
        double total = categories.Values.Sum();

        if (total <= 0)
        {
            return;
        }

        double start = -90;
        double cx = 110;
        double cy = 100;
        double outerR = 72;
        double innerR = 44;
        int idx = 0;

        foreach (var cat in categories.OrderByDescending(c => c.Value))
        {
            double pct = cat.Value / total;
            double sweep = categories.Count == 1 ? 359.99 : pct * 360;
            double s = start * Math.PI / 180;
            double e = (start + sweep) * Math.PI / 180;
            int large = sweep > 180 ? 1 : 0;

            double x1 = cx + outerR * Math.Cos(s);
            double y1 = cy + outerR * Math.Sin(s);
            double x2 = cx + outerR * Math.Cos(e);
            double y2 = cy + outerR * Math.Sin(e);
            double x3 = cx + innerR * Math.Cos(e);
            double y3 = cy + innerR * Math.Sin(e);
            double x4 = cx + innerR * Math.Cos(s);
            double y4 = cy + innerR * Math.Sin(s);

            donutSlices.Add(new DonutSlice
            {
                Path = $"M {FormatNumber(x1)} {FormatNumber(y1)} A {outerR} {outerR} 0 {large} 1 {FormatNumber(x2)} {FormatNumber(y2)} " +
                       $"L {FormatNumber(x3)} {FormatNumber(y3)} A {innerR} {innerR} 0 {large} 0 {FormatNumber(x4)} {FormatNumber(y4)} Z",
                Color = colors[idx % colors.Length],
                Category = cat.Key,
                PctText = $"{FormatNumber(pct * 100, "F0")}%",
                Amount = cat.Value,
                Percentage = pct * 100
            });

            start += sweep;
            idx++;
        }
    }
}
