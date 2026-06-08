package com.financetracker.backend.controller;

import com.financetracker.backend.model.Budget;
import com.financetracker.backend.service.BudgetService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/budgets")
@RequiredArgsConstructor
public class BudgetController {

    private final BudgetService budgetService;

    @PostMapping
    public Budget createBudget(@RequestBody Budget budget) {
        return budgetService.createBudget(budget);
    }

    @PutMapping("/{id}")
    public Budget updateBudget(@PathVariable Long id, @RequestBody Budget budgetDetails) {
        return budgetService.updateBudget(id, budgetDetails);
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteBudget(@PathVariable Long id) {
        budgetService.deleteBudget(id);
        return ResponseEntity.noContent().build();
    }

    @GetMapping("/{category}/{month}")
    public ResponseEntity<Budget> getBudgetByCategoryAndMonth(
            @PathVariable String category,
            @PathVariable String month) {
        return budgetService.getBudgetByCategoryAndMonth(category, month)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping("/month/{month}")
    public List<Budget> getBudgetsByMonth(@PathVariable String month) {
        return budgetService.getBudgetsByMonth(month);
    }

    @GetMapping("/vs-actual/{month}")
    public List<Map<String, Object>> getBudgetVsActual(@PathVariable String month) {
        return budgetService.getBudgetVsActual(month);
    }
}