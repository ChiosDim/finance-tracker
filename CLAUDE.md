# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Backend (Java Spring Boot)
- **Run backend**: `cd backend && ./mvnw spring-boot:run`
- **Run tests**: `cd backend && ./mvnw test`
- **Clean and build**: `cd backend && ./mvnw clean package`
- **Run a single test**: `cd backend && ./mvnw test -Dtest=<TestClassName>`

### Frontend (.NET Blazor WebAssembly)
- **Run frontend**: `cd frontend && dotnet run`
- **Run tests**: `cd frontend && dotnet test`
- **Build for production**: `cd frontend && dotnet publish -c Release`

### Common Development Flow
1. Start backend: `cd backend && ./mvnw spring-boot:run` (runs on http://localhost:8080)
2. Start frontend: `cd frontend && dotnet run` (runs on http://localhost:5097 or similar)
3. Both must be running simultaneously for full functionality

## Code Architecture

### High-Level Structure
The application follows a strict two-tier architecture:
- **Frontend**: .NET 8 Blazor WebAssembly (SPA) running in the browser
- **Backend**: Java 21 Spring Boot 3 REST API
- Communication occurs via JSON over HTTP/REST (frontend → backend)

### Backend Structure (`backend/`)
```
src/main/java/com/financetracker/backend/
├── BackendApplication.java          # Spring Boot entry point
├── config/
│   └── CorsConfig.java              # CORS configuration for frontend communication
├── controller/
│   └── TransactionController.java   # REST endpoints (/api/transactions/*)
├── model/
│   └── Transaction.java             # JPA entity representing financial transactions
├── repository/
│   └── TransactionRepository.java   # Spring Data JPA repository (extends JpaRepository)
└── service/
    └── TransactionService.java      # Business logic layer between controller and repository
```

### Frontend Structure (`frontend/`)
```
├── Layout/
│   ├── MainLayout.razor             # App shell (navigation + footer + body)
│   └── NavMenu.razor                # Sidebar navigation component
├── Models/
│   └── Transaction.cs               # C# model matching backend Transaction entity
├── Pages/
│   ├── Home.razor                   # Dashboard view (balance, charts, transaction list)
│   └── AddTransaction.razor         # Form for creating new transactions
└── wwwroot/css/
    └── app.css                      # Custom dark theme styling with CSS variables
```

### Key Integration Points
1. **Frontend → Backend Communication**:
   - Frontend uses `HttpClient` to call backend endpoints
   - Base URL configured in frontend services (typically `http://localhost:8080`)
   - Endpoints match those documented in README.md:
     - GET `/api/transactions` - list transactions
     - POST `/api/transactions` - create transaction
     - DELETE `/api/transactions/{id}` - delete transaction
     - GET `/api/transactions/summary` - financial summary
     - GET `/api/transactions/categories` - category breakdown

2. **Data Flow**:
   - Backend: HTTP request → Controller → Service → Repository → H2 database
   - Frontend: User interaction → Component state update → API call → Component re-render

3. **Persistence**:
   - Backend uses H2 file-based database (configured in `application.properties`)
   - Data persists between restarts in the backend directory

## Important Notes
- The backend must be running on port 8080 and frontend on their respective ports for CORS to work properly
- Both projects use wrapper scripts (`mvnw` for Maven, `dotnet` CLI) - no global installations required beyond JDK 21 and .NET 8 SDK
- When modifying backend entities, ensure frontend models stay in sync (both define the same fields)
- CSS modifications should be made in `wwwroot/css/app.css` using CSS variables for theme consistency