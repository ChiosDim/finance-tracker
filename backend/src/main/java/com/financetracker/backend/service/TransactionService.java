package com.financetracker.backend.service;

import com.financetracker.backend.model.Transaction;
import com.financetracker.backend.repository.TransactionRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;

import java.util.*;

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

    public Page<Transaction> getTransactionsPaged(Pageable pageable) {
        return repo.findAllPaged(pageable);
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
        Object[] result = repo.getSummaryAggregates();
        double income = result[0] != null ? ((Number) result[0]).doubleValue() : 0.0;
        double expense = result[1] != null ? ((Number) result[1]).doubleValue() : 0.0;
        Map<String, Double> summary = new HashMap<>();
        summary.put("income",  income);
        summary.put("expense", expense);
        summary.put("balance", income - expense);
        return summary;
    }

    public Map<String, Double> getCategoryBreakdown() {
        List<Object[]> results = repo.getCategoryBreakdownAggregates();
        Map<String, Double> breakdown = new LinkedHashMap<>();
        for (Object[] row : results) {
            String category = (row[0] != null) ? row[0].toString() : "Uncategorized";
            double total = (row[1] != null) ? ((Number) row[1]).doubleValue() : 0.0;
            breakdown.put(category, total);
        }
        return breakdown;
    }

    public List<Map<String, Object>> getMonthlyBreakdown() {
        List<Object[]> results = repo.getMonthlyBreakdownAggregates();
        List<Map<String, Object>> result = new ArrayList<>();
        for (Object[] row : results) {
            Map<String, Object> item = new LinkedHashMap<>();
            item.put("month",   (row[0] != null) ? row[0].toString() : "");
            item.put("income",  (row[1] != null) ? ((Number) row[1]).doubleValue() : 0.0);
            item.put("expense", (row[2] != null) ? ((Number) row[2]).doubleValue() : 0.0);
            result.add(item);
        }
        return result;
    }
}