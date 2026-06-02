package com.financetracker.backend.model;

import jakarta.persistence.*;
import lombok.Data;
import java.time.LocalDate;
import com.fasterxml.jackson.annotation.JsonFormat;

@Data               // Lombok: auto-generates getters, setters, toString
@Entity             // Tells JPA this class maps to a database table
@Table(name = "transactions")
public class Transaction {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String description;
    private Double amount;
    private String type;      // "INCOME" or "EXPENSE"
    private String category;  // e.g. "Food", "Salary", "Rent"
    @JsonFormat(pattern = "yyyy-MM-dd")
    private LocalDate date;
}