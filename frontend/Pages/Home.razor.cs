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

    private List<BarItem> barItems = new();
    private List<TickItem> yTicks = new();
    private List<DonutSlice> donutSlices = new();
    private string donutTotal = "";

    private IEnumerable<Transaction> FilteredTransactions =>
        (transactions ?? new List<Transaction>()).Where(t =>
        {
            if (activeFilter != "ALL" && t.Type != activeFilter)
            {
                return false;
            }

            if (filterFrom.HasValue && DateTime.TryParse(t.Date, out var df) && df < filterFrom.Value)
            {
                return false;
            }

            if (filterTo.HasValue && DateTime.TryParse(t.Date, out var dt) && dt > filterTo.Value)
            {
                return false;
            }

            return true;
        });

    protected override async Task OnInitializedAsync() => await LoadData();

    private async Task LoadData()
    {
        transactions = await Http.GetFromJsonAsync<List<Transaction>>("api/transactions");
        summary = await Http.GetFromJsonAsync<Summary>("api/transactions/summary");
        categories = await Http.GetFromJsonAsync<Dictionary<string, double>>("api/transactions/categories");
        monthly = await Http.GetFromJsonAsync<List<MonthlyData>>("api/transactions/monthly");
        ComputeBarChart();
        ComputeDonut();
    }

    private async Task Delete(long id)
    {
        await Http.DeleteAsync($"api/transactions/{id}");
        await LoadData();
    }

    private void ClearDateFilter()
    {
        filterFrom = null;
        filterTo = null;
    }

    private async Task ExportCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Date,Description,Category,Type,Amount");

        foreach (var t in transactions ?? new List<Transaction>())
        {
            sb.AppendLine($"{t.Date},{Csv(t.Description)},{Csv(t.Category)},{t.Type},{t.Amount}");
        }

        await JS.InvokeVoidAsync("downloadFile", "transactions.csv", "text/csv", sb.ToString());
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
                BarW = SvgNumber(barW)
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
    }

    private void ComputeDonut()
    {
        donutSlices.Clear();
        donutTotal = "";

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

        donutTotal = $"EUR {FormatNumber(total, "F0")}";

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
                PctText = $"{FormatNumber(pct * 100, "F0")}%"
            });

            start += sweep;
            idx++;
        }
    }
}
