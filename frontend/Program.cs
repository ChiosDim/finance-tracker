using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using frontend;
using frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Point all HTTP calls at the Java backend
var apiUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:8081/";
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiUrl)
});

// Register application services
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<SavingsGoalService>();

await builder.Build().RunAsync();