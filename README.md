# SwiftBite 🍔🚀

SwiftBite is a **full‑scale enterprise food delivery application**, inspired by platforms like Swiggy. The goal of this project is to build a **production‑ready, scalable, and secure system** using modern backend and frontend technologies.

This repository currently contains the **Centralized Authentication System**, which acts as the foundation for all other microservices in the SwiftBite ecosystem.

---

## 🧩 Project Vision

SwiftBite is structured as a **single enterprise-grade repository** containing both **Frontend (Angular)** and **Backend (ASP.NET Core Microservices)**. The application is inspired by Swiggy and follows **real-world production architecture patterns** used in large-scale systems.

SwiftBite is designed as a **microservices‑based enterprise application** that supports:

* Customers ordering food from restaurants
* Restaurants managing menus and orders
* Delivery partners fulfilling deliveries
* Admins controlling the entire platform

The system focuses on:

* Scalability
* Security
* Clean architecture
* Enterprise‑grade best practices

---

## 🔐 Centralized Authentication System (Completed ✅)

The centralized authentication service is responsible for **identity management and security** across all SwiftBite services.

### Features

* User Registration
* Secure Login
* Role‑based Authentication (Admin, Customer, Restaurant, Delivery Partner)
* JWT‑based Authorization
* Token Validation for Microservices
* Password Hashing & Security Best Practices

This service will be **consumed by all other microservices** instead of duplicating authentication logic.

---

## 🏗️ Architecture Overview

SwiftBite follows a **Microservices + API Gateway architecture** with a clear separation of concerns.

### High-Level Architecture

```
Client (Web / Mobile)
        │
        ▼
   API Gateway (Upcoming)
        │
        ├── Auth Service (JWT / Identity)
        ├── User Service
        ├── Restaurant Service
        ├── Order Service
        ├── Delivery Service
        └── Payment Service
```

Each microservice:

* Is independently deployable
* Has its own database
* Communicates via HTTP/REST
* Uses JWT tokens issued by the Auth Service

---

### Current Status

* ✅ Centralized Authentication Server
* ✅ Angular Frontend with Auth Flow
* 🚧 API Gateway (In Progress)
* 🚧 Domain Services Expansion

SwiftBite follows a **Microservices Architecture** with:

* API Gateway (planned)
* Centralized Authentication Service (completed)
* Independent domain‑based services (planned)

Each service:

* Has its own database
* Communicates via secure APIs
* Uses authentication tokens issued by the Auth Service

---

## 🛠️ Tech Stack

### Frontend

* Angular (Standalone Architecture)
* Bootstrap
* JWT Interceptor
* Route Guards

### Backend

* ASP.NET Core Web API
* Clean Architecture (API / Application / Domain / Infrastructure)
* Entity Framework Core
* SQL Server
* JWT Authentication

### DevOps & Infrastructure

* Docker & Docker Compose
* Git & GitHub
* API Gateway (Planned: Ocelot / YARP)
* CI/CD (Planned)

### Backend

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* JWT Authentication
* RESTful APIs

### Frontend (In Progress)

* Angular
* Bootstrap (UI)
* REST API Integration

### DevOps & Tools

* Git & GitHub
* Docker (planned)
* CI/CD Pipelines (planned)

---

## 📁 Repository Structure

### Frontend (Angular)

```
FRONTEND/
└── swiftbite-ui/
    ├── src/app/
    │   ├── core/
    │   │   ├── auth/
    │   │   │   ├── auth.config.ts
    │   │   │   ├── auth.guard.ts
    │   │   │   └── auth.service.ts
    │   │   └── interceptors/
    │   │       └── jwt.interceptor.ts
    │   │
    │   └── features/
    │       ├── auth/
    │       │   ├── login/
    │       │   ├── register/
    │       │   └── callback/
    │       └── admin/dashboard/
    └── angular.json
```

---

### Backend (ASP.NET Core Microservices)

```
BACKEND/
└── src/
    ├── AuthServer/
    │   └── SwiftBite.AuthServer/
    │       ├── Controllers
    │       ├── Services
    │       ├── Models
    │       ├── Data
    │       ├── Migrations
    │       └── Program.cs
    │
    ├── Services/
    │   └── SwiftBite.UserService/
    │       ├── API
    │       ├── Application
    │       ├── Domain
    │       └── Infrastructure
    │
    └── Shared/
```

```
SwiftBite.AuthService
│
├── Controllers
├── Services
├── Repositories
├── Models
├── DTOs
├── Data
├── Middlewares
└── Program.cs
```

---

## 🚀 Getting Started

### Prerequisites

* Node.js
* Angular CLI
* .NET SDK
* SQL Server
* Docker (optional)

### Run Frontend

```bash
cd FRONTEND/swiftbite-ui
npm install
ng serve
```

### Run Auth Server

```bash
cd BACKEND/src/AuthServer/SwiftBite.AuthServer
dotnet run
```

---

### Docker (Optional)

A `docker-compose.yml` is included to orchestrate frontend, backend services, and databases for local development.

### Prerequisites

* .NET SDK
* SQL Server
* Git

### Steps

1. Clone the repository

   ```bash
   git clone https://github.com/your-username/SwiftBite.git
   ```

2. Configure the database connection string in `appsettings.json`

3. Apply migrations (if any)

   ```bash
   dotnet ef database update
   ```

4. Run the application

   ```bash
   dotnet run
   ```

---

## 🛣️ Roadmap

* ✅ Centralized Authentication Server

* ✅ Angular Auth Flow (Login / Register / JWT Handling)

* 🚧 API Gateway Implementation

* ⏳ User Service Completion

* ⏳ Restaurant & Menu Service

* ⏳ Order Management Service

* ⏳ Delivery Partner Service

* ⏳ Payment Service

* ⏳ Admin Dashboard Expansion

* ⏳ Dockerized Deployment

* ⏳ CI/CD Pipelines

* ✅ Centralized Authentication Service

* ⏳ User Service

* ⏳ Restaurant Service

* ⏳ Order Management Service

* ⏳ Delivery Partner Service

* ⏳ Payment Service

* ⏳ API Gateway

* ⏳ Admin Dashboard

* ⏳ Mobile & Web UI Enhancements

---

## 🤝 Contribution

This is a **learning‑driven enterprise project**. Contributions, suggestions, and improvements are welcome.

---

## 📌 Note

SwiftBite is a **self‑built enterprise application** created to demonstrate real‑world system design, clean code practices, and scalable architecture.

---

## 👨‍💻 Author

**Sreedhar Nagalli**
.NET & Angular Developer

---

⭐ If you like this project, don’t forget to star the repository!
