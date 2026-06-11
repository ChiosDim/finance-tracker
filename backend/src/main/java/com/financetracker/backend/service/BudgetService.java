package com.financetracker.backend.service;

import com.financetracker.backend.model.Budget;
import com.financetracker.backend.model.Transaction;
import com.financetracker.backend.repository.BudgetRepository;
import com.financetracker.backend.repository.TransactionRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.*;

@Service
@RequiredArgsConstructor
public class BudgetService {

    private final BudgetRepository budgetRepository;
    private final TransactionRepository transactionRepository;

    public Budget createBudget(Budget budget) {
        return budgetRepository.save(budget);
    }

    public Budget updateBudget(Long id, Budget budgetDetails) {
        Budget budget = budgetRepository.findById(id)
                .orElseThrow(() -> new NoSuchElementException("Budget not found with id: " + id));

        budget.setCategory(budgetDetails.getCategory());
        budget.setAmount(budgetDetails.getAmount());
        budget.setBudgetMonth(budgetDetails.getBudgetMonth());

        return budgetRepository.save(budget);
    }

    public void deleteBudget(Long id) {
        Budget budget = budgetRepository.findById(id)
                .orElseThrow(() -> new NoSuchElementException("Budget not found with id: " + id));
        budgetRepository.delete(budget);
    }

    public Optional<Budget> getBudgetByCategoryAndMonth(String category, String month) {
        return budgetRepository.findByCategoryAndBudgetMonth(category, month);
    }

    public List<Budget> getBudgetsByMonth(String month) {
        return budgetRepository.findByBudgetMonth(month);
    }

    public List<Map<String, Object>> getBudgetVsActual(String month) {
        List<Map<String, Object>> result = new ArrayList<>();

        // Get all budgets for the month
        List<Budget> budgets = getBudgetsByMonth(month);

        // Get all transactions for the month (expenses only)
        List<Transaction> transactions = transactionRepository.findAll();
        Map<String, Double> actualExpenses = new HashMap<>();

        for (Transaction t : transactions) {
            if ("EXPENSE".equalsIgnoreCase(t.getType()) &&
                t.getDate() != null && t.getDate().length() >= 7 &&
                t.getDate().substring(0, 7).equals(month)) {

                String category = t.getCategory();
                if (category == null || category.isBlank()) {
                    category = "Uncategorized";
                }

                actualExpenses.merge(category, t.getAmount(), Double::sum);
            }
        }

        // Calculate budget vs actual for each category
        for (Budget budget : budgets) {
            String category = budget.getCategory();
            double budgeted = budget.getAmount();
            double actual = actualExpenses.getOrDefault(category, 0.0);
            double remaining = budgeted - actual;
            double percentage = (budgeted > 0) ? (actual / budgeted) * 100 : 0;

            Map<String, Object> item = new LinkedHashMap<>();
            item.put("category", category);
            item.put("budgeted", budgeted);
            item.put("actual", actual);
            item.put("remaining", remaining);
            item.put("percentage", Math.min(percentage, 100)); // Cap at 100% for display

            result.add(item);
        }

        return result;
    }
}