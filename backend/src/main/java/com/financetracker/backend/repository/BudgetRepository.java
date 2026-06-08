package com.financetracker.backend.repository;

import com.financetracker.backend.model.Budget;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository
public interface BudgetRepository extends JpaRepository<Budget, Long> {
    // Find budget by category and month
    Optional<Budget> findByCategoryAndMonth(String category, String month);

    // Find all budgets for a specific month
    List<Budget> findByMonth(String month);
}