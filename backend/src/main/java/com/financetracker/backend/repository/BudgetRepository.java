package com.financetracker.backend.repository;

import com.financetracker.backend.model.Budget;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository
public interface BudgetRepository extends JpaRepository<Budget, Long> {
    // Find budget by category and budgetMonth
    Optional<Budget> findByCategoryAndBudgetMonth(String category, String budgetMonth);

    // Find all budgets for a specific budgetMonth
    List<Budget> findByBudgetMonth(String budgetMonth);
}