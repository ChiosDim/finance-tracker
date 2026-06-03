package com.financetracker.backend.service;

import com.financetracker.backend.model.Transaction;
import com.financetracker.backend.repository.TransactionRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor // Lombok: auto-generates constructor for dependencies
public class TransactionService {

    private final TransactionRepository repo;

    public List<Transaction> getAll() {
        return repo.findAll().stream()
                .sorted((a, b) -> b.getDate().compareTo(a.getDate()))
                .collect(Collectors.toList());
    }

    public Map<String, Double> getCategoryBreakdown() {
        return repo.findAll().stream()
                .filter(t -> "EXPENSE".equalsIgnoreCase(t.getType()))
                .collect(Collectors.groupingBy(
                        Transaction::getCategory,
                        Collectors.summingDouble(Transaction::getAmount)));
    }

    // Save a new transaction
    public Transaction create(Transaction transaction) {
        transaction.setId(null); // ignore whatever id came from the frontend
        return repo.save(transaction);
    }

    // Delete a transaction by its ID
    public void delete(Long id) {
        repo.deleteById(id);
    }

    // Calculate total income, expenses, and balance
    public Map<String, Double> getSummary() {
        List<Transaction> all = repo.findAll();

        double income = all.stream()
                .filter(t -> "INCOME".equalsIgnoreCase(t.getType()))
                .mapToDouble(Transaction::getAmount)
                .sum();

        double expense = all.stream()
                .filter(t -> "EXPENSE".equalsIgnoreCase(t.getType()))
                .mapToDouble(Transaction::getAmount)
                .sum();

        Map<String, Double> summary = new HashMap<>();
        summary.put("income", income);
        summary.put("expense", expense);
        summary.put("balance", income - expense);
        return summary;
    }
}