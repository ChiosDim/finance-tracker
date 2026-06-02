package com.financetracker.backend.controller;

import com.financetracker.backend.model.Transaction;
import com.financetracker.backend.service.TransactionService;
import lombok.RequiredArgsConstructor;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/transactions")
@RequiredArgsConstructor
@CrossOrigin(origins = "http://localhost:5000")  // Allow .NET frontend to call us
public class TransactionController {

    private final TransactionService service;

    // GET http://localhost:8080/api/transactions
    @GetMapping
    public List<Transaction> getAll() {
        return service.getAll();
    }

    // POST http://localhost:8080/api/transactions
    @PostMapping
    public Transaction create(@RequestBody Transaction transaction) {
        return service.create(transaction);
    }

    // DELETE http://localhost:8080/api/transactions/1
    @DeleteMapping("/{id}")
    public void delete(@PathVariable Long id) {
        service.delete(id);
    }

    // GET http://localhost:8080/api/transactions/summary
    @GetMapping("/summary")
    public Map<String, Double> getSummary() {
        return service.getSummary();
    }
}