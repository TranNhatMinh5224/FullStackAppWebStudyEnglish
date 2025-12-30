# Learning English Platform

Welcome to the **Learning English Platform**, a comprehensive full-stack solution designed to provide an interactive and effective English learning experience. This repository contains the backend source code built with **ASP.NET Core**, following **Clean Architecture** principles.

## 🚀 Overview

This platform offers a robust set of features for students, teachers, and administrators, including:

*   **Course Management**: Structured learning paths with Courses, Lessons, Modules, and Lectures.
*   **Interactive Learning**:
    *   **Flashcards**: Vocabulary practice with spaced repetition.
    *   **Quizzes**: Multiple choice, fill-in-the-blank, matching, ordering, and true/false questions.
    *   **Pronunciation Assessment**: Real-time speech analysis using **Azure Speech Services**.
    *   **Essay Grading**: AI-powered essay scoring and feedback using **Google Gemini**.
*   **Gamification**: Streaks, user statistics, and progress tracking.
*   **User Management**: Role-based access control (RBAC) for Students, Teachers, and Admins.
*   **Monetization**: Integration with **PayOS** for course and subscription payments.

## 🛠️ Technology Stack

*   **Framework**: ASP.NET Core 8.0 Web API
*   **Database**: PostgreSQL (via Entity Framework Core)
*   **Object-Relational Mapper (ORM)**: Entity Framework Core
*   **File Storage**: MinIO (S3 compatible)
*   **Authentication**: JWT (JSON Web Tokens), OAuth2 (Google, Facebook)
*   **Validation**: FluentValidation
*   **Documentation**: Swagger / OpenAPI
*   **External Integrations**:
    *   **Azure Speech**: For pronunciation assessment.
    *   **Google Gemini**: For automated essay grading.
    *   **Oxford Dictionary API**: For dictionary lookups.
    *   **PayOS**: For payment processing.
    *   **Unsplash**: For sourcing images.
    *   **SMTP**: For email notifications.

## 📂 Project Structure

The solution follows the **Clean Architecture** pattern:

*   **`LearningEnglish.API`**: The entry point of the application. Contains Controllers, Middlewares, and Configuration (`Program.cs`, `appsettings.json`).
*   **`LearningEnglish.Application`**: Contains business logic, interfaces, DTOs, Mapping profiles, Validators, and Services.
*   **`LearningEnglish.Domain`**: Contains the core business entities, Enums, and database models.
*   **`LearningEnglish.Infrastructure`**: Implements interfaces defined in the Application layer. Handles Database context (`AppDbContext`), Repositories, External API integrations, and File Storage.

## ⚙️ Prerequisites

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [PostgreSQL](https://www.postgresql.org/)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop) (Optional, for running dependencies)
*   [MinIO](https://min.io/) (or an S3-compatible service)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/LearningEnglish.git
cd LearningEnglish
```

### 2. Configure Environment

Navigate to `BackendASP/LearningEnglish.API` and update `appsettings.json` (or create `appsettings.Development.json`) with your local configuration:

*   **ConnectionStrings**: Update `DefaultConnection` to point to your PostgreSQL instance.
*   **Jwt**: Set a secure `Key`, `Issuer`, and `Audience`.
*   **MinIO**: Configure your MinIO endpoint and credentials.
*   **External APIs**: Add keys for Azure Speech, Google Auth, etc., if you intend to use those features.

### 3. Run with Docker (Recommended for Dependencies)

You can use Docker Compose to start the database and MinIO services.

```bash
cd BackendASP
docker-compose up -d
```

### 4. Database Migrations

Apply the Entity Framework Core migrations to set up your database schema.

```bash
cd BackendASP/LearningEnglish.API
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at `https://localhost:5030` (or `http://localhost:5030`).
Swagger documentation can be accessed at `https://localhost:5030/swagger`.

## 🧪 Key Features Breakdown

### Authentication & Authorization
*   Uses **JWT** for stateless authentication.
*   **Role-Based Access Control (RBAC)** with dynamic permissions stored in the database.
*   Supports **OAuth2** login via Google and Facebook.

### Course & Content
*   **Courses** are hierarchical: `Course` -> `Lesson` -> `Module`.
*   **Modules** can contain:
    *   **Lectures**: Rich text content (Markdown/HTML) or Video.
    *   **Flashcards**: Vocabulary cards with images, audio, and examples.
    *   **Quizzes**: Assessments to test knowledge.
    *   **Essays**: Writing assignments.

### AI & Integrations
*   **Pronunciation**: Users record audio, which is sent to **Azure Speech SDK** for phoneme-level accuracy scoring.
*   **Essay Grading**: Student submissions are analyzed by **Google Gemini AI** to provide instant scores and detailed feedback based on grammar, vocabulary, and coherence.

## 🤝 Contributing

1.  Fork the repository.
2.  Create a feature branch (`git checkout -b feature/AmazingFeature`).
3.  Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4.  Push to the branch (`git push origin feature/AmazingFeature`).
5.  Open a Pull Request.

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information. sadsdas dsdsads
