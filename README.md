# StockGuard — Inventory and Expiry Management Platform

A full-stack inventory management system for businesses tracking products with expiration dates — built to demonstrate production-style backend architecture, concurrency safety, and full-stack integration.

## What it does

StockGuard solves real inventory problems: tracking individual product batches (not just totals), automatically allocating stock using FEFO (First Expired, First Out), preventing overselling under concurrent load, and providing real-time alerts for expiring or low stock.

## Tech stack

**Backend:** ASP.NET Core 10 Web API, Entity Framework Core, SQL Server (Azure SQL Edge), ASP.NET Core Identity, JWT authentication, FluentValidation, Redis, SignalR
**Frontend:** Angular 20+, standalone components, signals
**Infrastructure:** Docker & Docker Compose, GitHub Actions CI, k6 load testing
**Testing:** xUnit, Moq, NetArchTest, WebApplicationFactory

## Architecture

Clean Architecture as a modular monolith:
- `StockGuard.Domain` — entities, business rules, zero external dependencies
- `StockGuard.Application` — use cases, interfaces, FEFO allocation service
- `StockGuard.Infrastructure` — EF Core, Identity, repositories, Redis, SignalR client
- `StockGuard.Api` — controllers, JWT auth, Swagger
- `StockGuard.Worker` — background jobs (alerts, outbox publisher)
- `StockGuard.Web` — Angular frontend

Dependency direction is enforced automatically by architecture tests (`NetArchTest`) on every build.

## Key engineering decisions

- **Concurrency safety**: reservations use EF Core optimistic concurrency (`rowversion`) plus application-level checks. Proven with a k6 load test — 100 simultaneous requests against a 50-unit batch, verified against the real database afterward with zero overselling.
- **FEFO allocation**: a dedicated, unit-tested service that allocates from the earliest-expiring batch first, with full edge-case coverage (insufficient stock, quarantined batches).
- **Outbox pattern**: inventory events are written to the database in the same transaction as the business change, then published by a background worker — solving the classic "database saved but message failed" consistency problem.
- **Segregation of duties**: a purchase order cannot be approved by the same user who created it — enforced in code, not just documentation.

## Real numbers (see `docs/metrics.md` for full detail)

- 25 API endpoints across 8 controllers
- 5 roles with policy-based authorization
- 11 automated tests (architecture, unit, integration)
- Normal load test: p95 response time 9.3ms (424 requests, 10 concurrent users)
- Concurrency test: 100 simultaneous reservation requests against a 50-unit batch — zero overselling, verified via direct database query

## Running locally

```bash
docker compose up --build
```

This starts SQL Server, Redis, the API (port 8080), and the background worker together. Migrations and role seeding run automatically on startup.

API docs: `http://localhost:8080/swagger`

## Running tests

```bash
dotnet test
```

## Running load tests

```bash
k6 run loadtests/smoke-test.js
k6 run loadtests/normal-load-test.js
k6 run loadtests/reserve-concurrency.js
```
