package com.financetracker.backend.repository;

import com.financetracker.backend.model.Transaction;
import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

@Repository
public interface TransactionRepository extends JpaRepository<Transaction, Long> {
    // JpaRepository gives us everything for free

    @Query("SELECT t FROM Transaction t ORDER BY t.date DESC")
    List<Transaction> findAllOrderedByDateDesc();
}