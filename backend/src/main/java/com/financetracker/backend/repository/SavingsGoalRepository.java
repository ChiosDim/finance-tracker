package com.financetracker.backend.repository;

import com.financetracker.backend.model.SavingsGoal;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository
public interface SavingsGoalRepository extends JpaRepository<SavingsGoal, Long> {
    // Find active savings goals (those not yet completed)
    List<SavingsGoal> findByCurrentAmountLessThanTargetAmount();

    // Find savings goal by name
    Optional<SavingsGoal> findByName(String name);
}