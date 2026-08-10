# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in SwiftBite, please report it privately rather than opening a public issue. Email the maintainer with details (affected service, reproduction steps, impact) and allow time for a fix before public disclosure.

## Secrets Handling

* Never commit real credentials, API keys, or connection strings. `appsettings.json` files are tracked and must only contain placeholder (`CHANGE_ME`) values.
* Real local values go in `appsettings.Development.json` per service, or `.env` at the repo root — both are gitignored.
* If a secret is accidentally committed, treat it as compromised: rotate it immediately at the provider (Razorpay, database, etc.), then remove it from git history.

## Known Gaps

See [docs/PRODUCTION_READINESS_CHECKLIST.md](docs/PRODUCTION_READINESS_CHECKLIST.md) for the current list of outstanding security work (secrets management for production, dependency scanning, security headers, etc.).
