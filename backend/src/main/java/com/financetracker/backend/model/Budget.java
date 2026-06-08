package com.financetracker.backend.model;

import jakarta.persistence.*;
import lombok.Data;

@Data
@Entity
@Table(name = "budgets")
public class Budget {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String category;
    private double amount;
    private String month; // Format: yyyy-MM

    // Unique constraint: one budget per category per month
    // This would typically be handled with a unique constraint in the database
}