# Deploying SwiftBite for free (portfolio demo)

This is the one-time setup to get the whole stack live at zero cost:
**Vercel/Netlify** (frontend) + **Azure Container Apps** (8 backend services, free consumption grant) +
**one free Azure SQL Database** (shared, one schema-free set of tables per service — see note below) +
**Upstash** (free Redis + Kafka).

Local `docker compose up` continues to work exactly as before — none of this changes local dev.

---

## 1. Upstash (Redis + Kafka)

1. Sign up at [upstash.com](https://upstash.com) (no card required).
2. Create a **Redis** database (free tier). Copy the connection string in **StackExchange.Redis** format, e.g.:
   ```
   <host>:6379,password=<password>,ssl=True,abortConnect=False
   ```
3. Create a **Kafka** cluster (free tier). Note down:
   - Bootstrap endpoint (`Kafka:BootstrapServers`)
   - Username (`Kafka:SaslUsername`)
   - Password (`Kafka:SaslPassword`)

These three values get set as environment variables on every backend Container App in step 3.

---

## 2. Azure SQL Database (one free database, shared by all services)

All 7 SQL-backed services (AuthServer + 6 domain services) can safely point at **one physical database** —
their table names don't collide (verified: `Users`, `Orders`, `Payments`, `Restaurants`, etc. are all
distinct, and each service calls `Database.MigrateAsync()` on startup, so tables get created
automatically on first boot). This is a deploy-time configuration choice only — **no code or migration
changes were needed.**

1. In the Azure Portal, create an **Azure SQL Database** using the **Free offer** (one per subscription).
2. Allow Azure services to access the server (firewall rule), and note the full connection string.
3. You'll set the *same* connection string (with each service's own database name substituted, or all
   pointed at the same database name — either works) as `ConnectionStrings__DefaultConnection` /
   `ConnectionStrings__<Service>Db` on every Container App below.

---

## 3. Azure Container Apps (8 backend services)

Create one **Container Apps Environment** (free consumption grant covers this for near-zero traffic).
For each of the 8 services below, create a Container App pointing at its image on
`ghcr.io/<your-github-username>/swiftbite-<service>:latest`, with these environment variables:

**Every service needs:**
- `ASPNETCORE_ENVIRONMENT=Production`
- `Cors__AllowedOrigins__0=https://<your-vercel-or-netlify-domain>`

**AuthServer** additionally needs:
- `ConnectionStrings__DefaultConnection=<Azure SQL connection string>`
- `AuthServer__Issuer=https://<authserver-container-app-url>`
- `AuthServer__AngularBaseUrl=https://<your-vercel-or-netlify-domain>`
- `OpenIddictClients__GatewaySecret`, `OpenIddictClients__UserServiceSecret`,
  `OpenIddictClients__RestaurantServiceSecret`, `OpenIddictClients__OrderServiceSecret`,
  `OpenIddictClients__PaymentServiceSecret`, `OpenIddictClients__NotificationServiceSecret`,
  `OpenIddictClients__DeliveryServiceSecret` — generate a strong random string per secret
  (`openssl rand -hex 32`), and reuse the **same value** on the matching downstream service below.
- `Google__ClientId`, `Google__ClientSecret` — only if you want Google login live.

**ApiGateway** additionally needs:
- `AuthServer__Authority=https://<authserver-container-app-url>`
- `Redis__ConnectionString=<Upstash Redis connection string>`
- `OpenIddictClients__GatewaySecret=<same value as AuthServer's>`

**Each of the 6 domain services** (User/Restaurant/Order/Payment/Delivery/Notification) additionally needs:
- `ConnectionStrings__DefaultConnection` / `ConnectionStrings__<Service>Db=<Azure SQL connection string>`
- `AuthServer__Authority=https://<authserver-container-app-url>`
- `Redis__ConnectionString=<Upstash Redis connection string>` (User/Restaurant/Order/Delivery/Notification)
- `Kafka__BootstrapServers`, `Kafka__SaslUsername`, `Kafka__SaslPassword` (Order/Payment/Delivery/Notification/Restaurant/User — from Upstash Kafka)
- `OpenIddictClients__<ThatService>Secret=<matching value from AuthServer's list above>`

**PaymentService** additionally needs:
- `Razorpay__KeyId`, `Razorpay__KeySecret`, `Razorpay__WebhookSecret` (rotated Razorpay keys, see SECURITY.md)

Deploy AuthServer first, then the rest (they call it on startup for token validation).

---

## 4. GitHub Actions CI/CD

`.github/workflows/deploy.yml` builds each service's Docker image, pushes it to
`ghcr.io/<you>/swiftbite-<service>`, and updates the matching Container App to the new image —
triggered on every push to `main` once you've done the one-time setup above.

Add these repository secrets (Settings → Secrets and variables → Actions):
- `AZURE_CREDENTIALS` — output of `az ad sp create-for-rbac --sdk-auth` scoped to your resource group
- `AZURE_RESOURCE_GROUP` — the resource group containing your Container Apps

`GITHUB_TOKEN` (built-in) is sufficient to push to `ghcr.io` for a public repo.

---

## 5. Frontend (Vercel or Netlify)

1. Before your first deploy, edit `frontend/swiftbite-ui/src/environments/environment.production.ts` and
   replace every `CHANGE_ME` with your real Azure Container Apps URLs (once step 3 is live) and your
   live Razorpay key.
2. Connect your GitHub repo in Vercel/Netlify's dashboard. Framework preset: Angular.
   - Build command: `npm run build`
   - Output directory: `dist/swiftbite-ui/browser`
3. Once deployed, go back to AuthServer's Container App and update `AuthServer__AngularBaseUrl` and every
   service's `Cors__AllowedOrigins__0` to the real Vercel/Netlify URL, then redeploy AuthServer (it
   re-seeds OAuth clients with the correct redirect URIs on startup).

---

## Verifying it's live

1. Open the Vercel/Netlify URL, register a user, log in — confirms AuthServer + ApiGateway + CORS + OpenIddict are wired correctly end-to-end.
2. Place a test order — confirms OrderService, Kafka (Upstash), and Redis are working.
3. Check each Container App's logs in the Azure Portal if anything 401s or 500s — almost always a mismatched `OpenIddictClients` secret or a missing env var.
