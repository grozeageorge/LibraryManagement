# 📚 Enterprise Library Management System

![.NET](https://img.shields.io/badge/.NET-10.0-512bd4?style=flat&logo=dotnet)
![Language](https://img.shields.io/badge/Language-C%23-blue)
![Architecture](https://img.shields.io/badge/Architecture-Clean-green)
![Tests](https://img.shields.io/badge/Tests-200%2B-success)
![Coverage](https://img.shields.io/badge/Coverage-95%25-brightgreen)

A robust, backend-focused Library Management System built with **.NET 10**, demonstrating **Clean Architecture**, **Domain-Driven Design (DDD)** principles, and complex business logic implementation.

This project focuses on data integrity, concurrency control, and rigorous unit testing, simulating a real-world enterprise environment.

---

## 🏗 Architecture

The solution follows the **Clean Architecture** (Onion Architecture) pattern to ensure separation of concerns and testability:

*   **`Library.Domain`**: The core of the system. Contains Entities, Enums, and Repository Interfaces. It has **zero dependencies** on external libraries.
*   **`Library.Data`**: Implements the persistence layer using **Entity Framework Core** and the **Repository Pattern**.
*   **`Library.Services`**: Contains the Business Logic, **FluentValidation** rules, and orchestrates the flow of data.
*   **`Library.ConsoleApp`**: The Composition Root. Handles Dependency Injection (DI) setup, Configuration, and Logging.

---

## 🚀 Key Features & Business Logic

This system implements a strict set of complex lending rules designed to ensure fair usage and inventory stability.

### 📖 Lending Constraints
*   **Inventory Protection (Stock Buffer):** A book cannot be borrowed if the remaining circulating stock drops below **10%** of the initial fund.
*   **Category Diversity:** To encourage diverse reading, if a user borrows 3 or more books in a single transaction, they must be selected from at least 2 distinct categories.
*   **Domain History Limits:** The system prevents hoarding by limiting how many books from a specific category (or its hierarchy) a user can borrow within a specific timeframe (e.g., last 3 months).
*   **Cooling-off Period:** Users must wait a configurable number of days before re-borrowing the exact same book to allow others a chance to read it.

### 👤 User Roles & Privileges
The system handles different logic for **Standard Readers** vs. **Librarians**:
*   **Standard Readers:** Subject to strict daily and total borrowing limits.
*   **Librarians:**
    *   Enjoy **doubled** limits for total books allowed and loan extensions.
    *   Benefit from **halved** waiting times for re-borrowing and history checks.
    *   **Unlimited Daily Borrowing:** Librarians bypass the standard daily borrowing cap.
    *   **Processing Throughput:** The system enforces a daily limit on how many loans a specific librarian can process/issue to prevent fatigue or errors.

### 🛡️ Advanced Technical Features
*   **Optimistic Concurrency Control:** Uses SQL `RowVersion` to prevent race conditions (e.g., two users trying to borrow the last copy simultaneously). The system gracefully handles conflicts without data corruption.
*   **Structured Logging:** Implemented using **Serilog** with sinks for both Console and **SQL Server**, capturing all business transactions and errors.
*   **Dynamic Configuration:** All business thresholds (Limits, Percentages, Days) are configurable via `appsettings.json` and can be adjusted without recompiling the application.

---

## 🛠 Tech Stack

*   **Framework:** .NET 10 (C# 13)
*   **Database:** SQL Server (via Entity Framework Core 10)
*   **ORM:** Entity Framework Core (Code-First Migrations)
*   **Logging:** Serilog (MSSqlServer Sink)
*   **Validation:** FluentValidation
*   **Testing:** NUnit, Moq, FluentAssertions
*   **Code Quality:** StyleCop Analyzers

---

## 🧪 Testing Strategy

The project maintains a high standard of quality assurance with over **200 Unit Tests** and **>90% Code Coverage**.

*   **Test Patterns:** Uses the **Object Mother / Factory Pattern** to create test data, keeping tests clean and maintainable.
*   **Parameterized Tests:** Extensive use of `[TestCase]` to verify boundary conditions (e.g., testing 9% vs 10% stock levels).
*   **Mocking:** Uses **Moq** to isolate the Service Layer from the Database and Configuration.
*   **Scenarios Covered:**
    *   Validation Logic (Empty fields, invalid formats).
    *   Business Rules (Stock calculations, Date arithmetic).
    *   **Concurrency Handling:** Simulates `DbUpdateConcurrencyException` to verify graceful failure handling.
    *   **Infrastructure Failure:** Verifies logging behavior when the database is unreachable.

---

## ⚙️ Getting Started

### Prerequisites
*   .NET 10 SDK
*   SQL Server (LocalDB or Express)
*   Visual Studio 2022 (or VS Code)

### Installation
1.  **Clone the repository:**
    ```bash
    git clone https://github.com/yourusername/LibraryManagement.git
    ```
2.  **Configure Database:**
    Open `src/Library.ConsoleApp/appsettings.json` and update the `ConnectionStrings` if necessary. Default is `(localdb)\MSSQLLocalDB`.

3.  **Apply Migrations:**
    Open your terminal in the root folder and run:
    ```bash
    dotnet ef database update --project src/Library.Data --startup-project src/Library.ConsoleApp
    ```

4.  **Run the Application:**
    ```bash
    dotnet run --project src/Library.ConsoleApp
    ```

---

## 📂 Project Structure

```text
LibraryManagement
├── src
│   ├── Library.Domain       # Core Entities & Interfaces
│   ├── Library.Data         # EF Core Context & Repositories
│   ├── Library.Services     # Business Logic & Validators
│   └── Library.ConsoleApp   # Entry Point & DI Configuration
├── tests
│   └── Library.Tests        # NUnit Tests (200+)
└── Directory.Build.props    # Global Build Settings
```
---

## 📝 License

This project is licensed under the MIT License.
