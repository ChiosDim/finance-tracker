package com.financetracker.backend.service;

import com.financetracker.backend.model.SavingsGoal;
import com.financetracker.backend.repository.SavingsGoalRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;
import java.util.NoSuchElementException;

@Service
@RequiredArgsConstructor
public class SavingsGoalService {

    private final SavingsGoalRepository savingsGoalRepository;

    public SavingsGoal createSavingsGoal(SavingsGoal savingsGoal) {
        return savingsGoalRepository.save(savingsGoal);
    }

    public SavingsGoal updateSavingsGoal(Long id, SavingsGoal savingsGoalDetails) {
        SavingsGoal savingsGoal = savingsGoalRepository.findById(id)
                .orElseThrow(() -> new NoSuchElementException("Savings goal not found with id: " + id));

        savingsGoal.setName(savingsGoalDetails.getName());
        savingsGoal.setTargetAmount(savingsGoalDetails.getTargetAmount());
        savingsGoal.setCurrentAmount(savingsGoalDetails.getCurrentAmount());
        savingsGoal.setTargetDate(savingsGoalDetails.getTargetDate());
        savingsGoal.setDescription(savingsGoalDetails.getDescription());

        return savingsGoalRepository.save(savingsGoal);
    }

    public void deleteSavingsGoal(Long id) {
        SavingsGoal savingsGoal = savingsGoalRepository.findById(id)
                .orElseThrow(() -> new NoSuchElementException("Savings goal not found with id: " + id));
        savingsGoalRepository.delete(savingsGoal);
    }

    public List<SavingsGoal> getAllSavingsGoals() {
        return savingsGoalRepository.findAll();
    }

    public Optional<SavingsGoal> getSavingsGoalById(Long id) {
        return savingsGoalRepository.findById(id);
    }

    public Optional<SavingsGoal> getSavingsGoalByName(String name) {
        return savingsGoalRepository.findByName(name);
    }

    public List<SavingsGoal> getActiveSavingsGoals() {
        return savingsGoalRepository.findByCurrentAmountLessThanTargetAmount();
    }

    public SavingsGoal addToSavingsGoal(Long id, double amount) {
        SavingsGoal savingsGoal = savingsGoalRepository.findById(id)
                .orElseThrow(() -> new NoSuchElementException("Savings goal not found with id: " + id));

        double newCurrentAmount = savingsGoal.getCurrentAmount() + amount;
        savingsGoal.setCurrentAmount(Math.min(newCurrentAmount, savingsGoal.getTargetAmount()));

        return savingsGoalRepository.save(savingsGoal);
    }
}