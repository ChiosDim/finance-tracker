package com.financetracker.backend.repository;

import com.financetracker.backend.model.Transaction;
import java.util.List;
import java.util.Map;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

@Repository
public interface TransactionRepository extends JpaRepository<Transaction, Long> {

    // NEW: paged query
    @Query("SELECT t FROM Transaction t")
    Page<Transaction> findAllPaged(Pageable pageable);
    // JpaRepository gives us everything for free

    @Query("SELECT t FROM Transaction t ORDER BY t.date DESC")
    List<Transaction> findAllOrderedByDateDesc();

    // NEW: Aggregate queries for better performance
    @Query("SELECT SUM(CASE WHEN UPPER(t.type) = 'INCOME' THEN t.amount ELSE 0 END) as income, "
           + "SUM(CASE WHEN UPPER(t.type) = 'EXPENSE' THEN t.amount ELSE 0 END) as expense "
           + "FROM Transaction t")
    Object[] getSummaryAggregates();

    @Query("SELECT t.category as category, SUM(t.amount) as total "
           + "FROM Transaction t "
           + "WHERE UPPER(t.type) = 'EXPENSE' AND t.category IS NOT NULL AND t.category <> '' "
           + "GROUP BY t.category")
    List<Object[]> getCategoryBreakdownAggregates();

    @Query("SELECT SUBSTRING(t.date, 1, 7) as month, "
           + "SUM(CASE WHEN UPPER(t.type) = 'INCOME' THEN t.amount ELSE 0 END) as income, "
           + "SUM(CASE WHEN UPPER(t.type) = 'EXPENSE' THEN t.amount ELSE 0 END) as expense "
           + "FROM Transaction t "
           + "WHERE t.date IS NOT NULL AND LENGTH(t.date) >= 7 "
           + "GROUP BY SUBSTRING(t.date, 1, 7) "
           + "ORDER BY month")
    List<Object[]> getMonthlyBreakdownAggregates();
}