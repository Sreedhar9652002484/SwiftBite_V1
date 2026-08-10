# SwiftBite UI

The Angular frontend for [SwiftBite](../../README.md) — customer ordering flow, restaurant admin dashboard, delivery partner view, and OAuth2/OIDC login against the SwiftBite Auth Server.

**Stack:** Angular 20 (standalone components), TypeScript, Tailwind CSS, `angular-oauth2-oidc`, SignalR client (live order/notification updates), Leaflet (delivery tracking map).

## Prerequisites

* Node.js 20+
* The backend running — either via `docker compose up --build` from the repo root, or the individual services via `dotnet run` (see the [root README](../../README.md#-getting-started))

## Development server

```bash
npm install
ng serve
```

Open `http://localhost:4200/`. The dev build reads `src/environments/environment.ts`, which points at the local backend (`http://localhost:5000` gateway, `http://localhost:5001` auth server).

## Production build

```bash
ng build --configuration production
```

This swaps in `src/environments/environment.production.ts` (via the `fileReplacements` entry in `angular.json`) — **update the `CHANGE_ME` placeholders in that file with your real deployed backend URLs before building for a live deployment.** See [docs/DEPLOYMENT.md](../../docs/DEPLOYMENT.md) for the full deployment walkthrough (Vercel/Netlify).

Output goes to `dist/swiftbite-ui/browser`.

## Tests

```bash
ng test
```

Runs the Karma/Jasmine unit tests. Coverage here is currently minimal (see the [production readiness checklist](../../docs/PRODUCTION_READINESS_CHECKLIST.md)) — contributions welcome.

## Project structure

```
src/app/
├── core/
│   ├── auth/            # OIDC config, auth guard, auth service
│   ├── interceptors/    # JWT + loading interceptors
│   └── services/        # API clients per domain (order, payment, restaurant, delivery, notification, user)
└── features/
    ├── auth/             # login, register, OAuth callback
    ├── customer/         # ordering flow, order tracking
    ├── restaurents/      # restaurant admin dashboard, menu manager, analytics
    ├── delivery/         # delivery partner dashboard
    └── admin/            # admin dashboard
```
