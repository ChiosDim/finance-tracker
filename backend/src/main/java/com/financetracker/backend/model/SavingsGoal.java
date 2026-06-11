package com.financetracker.backend.model;

import jakarta.persistence.*;
import lombok.Data;

@Data
@Entity
@Table(name = "savings_goals")
public class SavingsGoal {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String name;
    private double targetAmount;
    private double currentAmount;
    private String targetDate; // Format: yyyy-MM-dd
    private String description;

    // Default constructor
    public SavingsGoal() {
        this.currentAmount = 0.0;
    }
}