using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using frontend.Models;

namespace frontend.Services
{
    public class SavingsGoalService
    {
        private readonly HttpClient _http;

        public SavingsGoalService(HttpClient http)
        {
            _http = http;
        }

        public async Task<SavingsGoal> CreateSavingsGoalAsync(SavingsGoal savingsGoal)
        {
            var response = await _http.PostAsJsonAsync("api/savings-goals", savingsGoal);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<SavingsGoal>();
            if (result == null)
            {
                throw new InvalidOperationException("Expected a non-null SavingsGoal");
            }
            return result;
        }

        public async Task<SavingsGoal> UpdateSavingsGoalAsync(long id, SavingsGoal savingsGoal)
        {
            var response = await _http.PutAsJsonAsync($"api/savings-goals/{id}", savingsGoal);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<SavingsGoal>();
            if (result == null)
            {
                throw new InvalidOperationException("Expected a non-null SavingsGoal");
            }
            return result;
        }

        public async Task DeleteSavingsGoalAsync(long id)
        {
            var response = await _http.DeleteAsync($"api/savings-goals/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<SavingsGoal>> GetAllSavingsGoalsAsync()
        {
            var response = await _http.GetAsync("api/savings-goals");
            response.EnsureSuccessStatusCode();
            var savingsGoals = await response.Content.ReadFromJsonAsync<List<SavingsGoal>>();
            return savingsGoals ?? new List<SavingsGoal>();
        }

        public async Task<SavingsGoal?> GetSavingsGoalByIdAsync(long id)
        {
            var response = await _http.GetAsync($"api/savings-goals/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SavingsGoal>();
        }

        public async Task<SavingsGoal?> GetSavingsGoalByNameAsync(string name)
        {
            var response = await _http.GetAsync($"api/savings-goals/name/{name}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SavingsGoal>();
        }

        public async Task<List<SavingsGoal>> GetActiveSavingsGoalsAsync()
        {
            var response = await _http.GetAsync("api/savings-goals/active");
            response.EnsureSuccessStatusCode();
            var savingsGoals = await response.Content.ReadFromJsonAsync<List<SavingsGoal>>();
            return savingsGoals ?? new List<SavingsGoal>();
        }

        public async Task<SavingsGoal> AddToSavingsGoalAsync(long id, double amount)
        {
            var response = await _http.PostAsJsonAsync($"api/savings-goals/{id}/add", new { amount });
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<SavingsGoal>();
            if (result == null)
            {
                throw new InvalidOperationException("Expected a non-null SavingsGoal");
            }
            return result;
        }
    }
}