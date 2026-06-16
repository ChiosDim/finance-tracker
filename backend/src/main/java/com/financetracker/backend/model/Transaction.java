package com.financetracker.backend.model;

import jakarta.persistence.*;
import lombok.Data;

@Data
@Entity
@Table(name = "transactions", indexes = @Index(columnList = "date"))
public class Transaction {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String description;
    private Double amount;
    private String type;
    private String category;
    private String date;
}