package com.financetracker.backend.controller;

import com.financetracker.backend.model.Transaction;
import com.financetracker.backend.service.TransactionService;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/transactions")
@RequiredArgsConstructor
public class TransactionController {

    private final TransactionService service;

    @GetMapping
    public List<Transaction> getAll() {
        return service.getAll();
    }

    @GetMapping("/{id}")
    public ResponseEntity<Transaction> getById(@PathVariable Long id) {
        return service.getById(id)
            .map(ResponseEntity::ok)
            .orElse(ResponseEntity.notFound().build());
    }

    @PostMapping
    public Transaction create(@RequestBody Transaction transaction) {
        return service.create(transaction);
    }

    @PutMapping("/{id}")
    public Transaction update(@PathVariable Long id, @RequestBody Transaction transaction) {
        return service.update(id, transaction);
    }

    @DeleteMapping("/{id}")
    public void delete(@PathVariable Long id) {
        service.delete(id);
    }

    @GetMapping("/summary")
    public Map<String, Double> getSummary() {
        return service.getSummary();
    }

    @GetMapping("/categories")
    public Map<String, Double> getCategories() {
        return service.getCategoryBreakdown();
    }

    @GetMapping("/monthly")
    public List<Map<String, Object>> getMonthly() {
        return service.getMonthlyBreakdown();
    }

    // NEW: Paginated endpoint for frontend
    @GetMapping("/page")
    public Page<Transaction> getTransactionsPaged(Pageable pageable) {
        return repo.findAllPaged(pageable);
    }
}