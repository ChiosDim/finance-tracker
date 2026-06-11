package com.financetracker.backend.controller;

import com.financetracker.backend.model.SavingsGoal;
import com.financetracker.backend.service.SavingsGoalService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/savings-goals")
@RequiredArgsConstructor
public class SavingsGoalController {

    private final SavingsGoalService savingsGoalService;

    @PostMapping
    public SavingsGoal createSavingsGoal(@RequestBody SavingsGoal savingsGoal) {
        return savingsGoalService.createSavingsGoal(savingsGoal);
    }

    @PutMapping("/{id}")
    public SavingsGoal updateSavingsGoal(@PathVariable Long id, @RequestBody SavingsGoal savingsGoalDetails) {
        return savingsGoalService.updateSavingsGoal(id, savingsGoalDetails);
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteSavingsGoal(@PathVariable Long id) {
        savingsGoalService.deleteSavingsGoal(id);
        return ResponseEntity.noContent().build();
    }

    @GetMapping
    public List<SavingsGoal> getAllSavingsGoals() {
        return savingsGoalService.getAllSavingsGoals();
    }

    @GetMapping("/{id}")
    public ResponseEntity<SavingsGoal> getSavingsGoalById(@PathVariable Long id) {
        return savingsGoalService.getSavingsGoalById(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping("/name/{name}")
    public ResponseEntity<SavingsGoal> getSavingsGoalByName(@PathVariable String name) {
        return savingsGoalService.getSavingsGoalByName(name)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping("/active")
    public List<SavingsGoal> getActiveSavingsGoals() {
        return savingsGoalService.getActiveSavingsGoals();
    }

    @PostMapping("/{id}/add")
    public SavingsGoal addToSavingsGoal(@PathVariable Long id, @RequestParam double amount) {
        return savingsGoalService.addToSavingsGoal(id, amount);
    }
}