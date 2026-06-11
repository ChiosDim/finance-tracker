using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using frontend.Models;

namespace frontend.Services
{
    public class BudgetService
    {
        private readonly HttpClient _http;

        public BudgetService(HttpClient http)
        {
            _http = http;
        }

        public async Task<Budget> CreateBudgetAsync(Budget budget)
        {
            var response = await _http.PostAsJsonAsync("api/budgets", budget);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Budget>();
        }

        public async Task<Budget> UpdateBudgetAsync(long id, Budget budget)
        {
            var response = await _http.PutAsJsonAsync($"api/budgets/{id}", budget);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Budget>();
        }

        public async Task DeleteBudgetAsync(long id)
        {
            var response = await _http.DeleteAsync($"api/budgets/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<Budget?> GetBudgetByCategoryAndMonthAsync(string category, string month)
        {
            var response = await _http.GetFromJsonAsync<Budget>($"api/budgets/{category}/{month}");
            return response;
        }

        public async Task<List<Budget>> GetBudgetsByMonthAsync(string month)
        {
            var response = await _http.GetFromJsonAsync<List<Budget>>($"api/budgets/month/{month}");
            return response ?? new List<Budget>();
        }

        public async Task<List<Dictionary<string, object>>> GetBudgetVsActualAsync(string month)
        {
            var response = await _http.GetFromJsonAsync<List<Dictionary<string, object>>>($"api/budgets/vs-actual/{month}");
            return response ?? new List<Dictionary<string, object>>();
        }
    }
}