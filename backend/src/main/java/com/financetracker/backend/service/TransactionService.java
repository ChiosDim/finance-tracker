package com.financetracker.backend.service;

import com.financetracker.backend.model.Transaction;
import com.financetracker.backend.repository.TransactionRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.*;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class TransactionService {

    private final TransactionRepository repo;

    public List<Transaction> getAll() {
        return repo.findAllOrderedByDateDesc();
    }

    public Optional<Transaction> getById(Long id) {
        return repo.findById(id);
    }

    public Transaction create(Transaction transaction) {
        transaction.setId(null);
        return repo.save(transaction);
    }

    public Transaction update(Long id, Transaction transaction) {
        transaction.setId(id);
        return repo.save(transaction);
    }

    public void delete(Long id) {
        repo.deleteById(id);
    }

    public Map<String, Double> getSummary() {
        List<Transaction> all = repo.findAll();
        double income  = all.stream().filter(t -> "INCOME".equalsIgnoreCase(t.getType()))
            .mapToDouble(Transaction::getAmount).sum();
        double expense = all.stream().filter(t -> "EXPENSE".equalsIgnoreCase(t.getType()))
            .mapToDouble(Transaction::getAmount).sum();
        Map<String, Double> summary = new HashMap<>();
        summary.put("income",  income);
        summary.put("expense", expense);
        summary.put("balance", income - expense);
        return summary;
    }

    public Map<String, Double> getCategoryBreakdown() {
        return repo.findAll().stream()
            .filter(t -> "EXPENSE".equalsIgnoreCase(t.getType()))
            .collect(Collectors.groupingBy(
                Transaction::getCategory,
                Collectors.summingDouble(Transaction::getAmount)
            ));
    }

    public List<Map<String, Object>> getMonthlyBreakdown() {
        // TreeMap keeps months sorted chronologically (yyyy-MM sorts lexicographically)
        Map<String, double[]> monthly = new TreeMap<>();
        for (Transaction t : repo.findAll()) {
            if (t.getDate() == null || t.getDate().length() < 7) continue;
            String month = t.getDate().substring(0, 7);
            monthly.computeIfAbsent(month, k -> new double[]{0.0, 0.0});
            if ("INCOME".equalsIgnoreCase(t.getType()))
                monthly.get(month)[0] += t.getAmount();
            else if ("EXPENSE".equalsIgnoreCase(t.getType()))
                monthly.get(month)[1] += t.getAmount();
        }
        List<Map<String, Object>> result = new ArrayList<>();
        for (var entry : monthly.entrySet()) {
            Map<String, Object> item = new LinkedHashMap<>();
            item.put("month",   entry.getKey());
            item.put("income",  entry.getValue()[0]);
            item.put("expense", entry.getValue()[1]);
            result.add(item);
        }
        return result;
    }
}