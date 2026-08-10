# SwiftBite 🍔🚀

SwiftBite is a **microservices-based food delivery platform**, inspired by platforms like Swiggy. It's built with ASP.NET Core (backend) and Angular (frontend), following clean architecture and real-world microservices patterns.

SwiftBite supports:

* Customers ordering food from restaurants
* Restaurants managing menus and orders
* Delivery partners fulfilling deliveries
* Admins controlling the platform

See [docs/PRODUCTION_READINESS_CHECKLIST.md](docs/PRODUCTION_READINESS_CHECKLIST.md) for an honest, up-to-date assessment of what's done and what's left before this is production-ready.

---

## 🏗️ Architecture

```
Client (Web / Mobile)
        │
        ▼
   API Gateway (YARP)
        │
        ├── Auth Server (OpenIddict / OAuth2)
        ├── User Service
        ├── Restaurant Service
        ├── Order Service
        ├── Payment Service
        ├── Delivery Service
        └── Notification Service
```

Each microservice:

* Is independently deployable, with its own database
* Communicates via HTTP/REST (and Kafka for async events)
* Validates tokens issued by the Auth Server via OpenIddict introspection

### Current Status

* ✅ Auth Server (OpenIddict, JWT, Google login)
* ✅ API Gateway (YARP, rate limiting, Redis cache)
* ✅ User, Restaurant, Order, Payment, Delivery, Notification services (Clean Architecture: API / Application / Domain / Infrastructure)
* ✅ Angular frontend with auth flow, admin dashboard
* 🚧 Test coverage, CI/CD, deployment infrastructure (see checklist)

---

## 🛠️ Tech Stack

**Frontend:** Angular (standalone), TypeScript, Tailwind, `angular-oauth2-oidc`, SignalR client, Leaflet

**Backend:** ASP.NET Core Web API, Clean Architecture, Entity Framework Core, SQL Server, OpenIddict, MediatR, FluentValidation, Serilog, Redis, Kafka

**Infrastructure:** Docker & Docker Compose, GitHub Actions CI

---

## 📁 Repository Structure

```
src/
├── AuthServer/SwiftBite.AuthServer/       # OpenIddict auth server
├── ApiGateway/SwiftBite.ApiGateway/       # YARP reverse proxy
└── Services/
    ├── SwiftBite.UserService/
    ├── SwiftBite.RestaurantService/
    ├── SwiftBite.OrderService/
    ├── SwiftBite.PaymentService/
    ├── SwiftBite.DeliveryService/
    └── SwiftBite.NotificationService/
        # each service: .API / .Application / .Domain / .Infrastructure

SwiftBite.Shared.Kernel/          # shared domain primitives
SwiftBite.Shared.Exceptions/      # shared exception handling middleware
frontend/swiftbite-ui/            # Angular app
docker-compose.yml                # full local stack (infra + services)
```

---

## 🚀 Getting Started

### Prerequisites

* .NET 8 SDK
* Node.js + Angular CLI
* Docker Desktop (recommended — runs SQL Server, Redis, RabbitMQ, Kafka, and every service)

### Run everything with Docker

```bash
cp .env.example .env
# edit .env and fill in real values (DB password, Razorpay test keys, etc.)
docker compose up --build
```

This starts SQL Server, Redis, RabbitMQ, Kafka, and all backend services. The API Gateway is exposed on `http://localhost:5000`, the Auth Server on `http://localhost:5001`.

### Run a service locally (outside Docker)

Each service reads `appsettings.Development.json` for local secrets (gitignored — not committed). Create it from the values in the service's `appsettings.json`, pointing at your local SQL Server instance, e.g.:

```bash
cd src/AuthServer/SwiftBite.AuthServer
dotnet run
```

### Run the frontend

```bash
cd frontend/swiftbite-ui
npm install
ng serve
```

---

## 🔐 Security

Never commit real secrets. `appsettings.json` files ship with `CHANGE_ME` placeholders; real values belong in `appsettings.Development.json` (gitignored) or environment variables — see `.env.example`. See [SECURITY.md](SECURITY.md) for reporting vulnerabilities.

---

## 🧪 Testing

Run all backend tests:

```bash
dotnet test SwiftBite.sln
```

---

## 🤝 Contribution

Contributions, suggestions, and improvements are welcome — see [CONTRIBUTING.md](CONTRIBUTING.md).

## 📄 License

MIT — see [LICENSE](LICENSE).

## 👨‍💻 Author

**Sreedhar Nagalli** — .NET & Angular Developer
