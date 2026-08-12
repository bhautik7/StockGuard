# StockGuard Performance & Testing Metrics

All results below are from actual test runs, not estimates. Reproduce using the commands shown.

## Smoke Test
- Date: 2026-08-12
- Environment: Local Docker Compose (Api container, Azure SQL Edge, Redis)
- Command: `k6 run loadtests/smoke-test.js`
- Load: 1 virtual user, 10 seconds
- Result: 10/10 requests succeeded (100%), avg response time 19.75ms, p95 81.13ms
- Evidence location: captured in project chat log / terminal output

## Normal Load Test
- Date: 2026-08-12
- Environment: Local Docker Compose (Api container, Azure SQL Edge, Redis)
- Command: `k6 run loadtests/normal-load-test.js`
- Load: Ramped 0→10 virtual users over 10s, held 10 users for 30s, ramped down over 10s (50s total)
- Endpoint tested: `GET /api/products`
- Result: 424/424 requests succeeded (100%)
- Response time: avg 7.27ms, median 7.42ms, p90 8.57ms, p95 9.3ms, max 109.18ms
- Threshold: p95 < 300ms — PASSED
- Limitations: Tested on local development hardware, not production-equivalent infrastructure; single-machine Docker Compose, not distributed load generation.

## Concurrency / Reservation Test
- Date: 2026-08-11 (Phase 9)
- Command: `k6 run loadtests/reserve-concurrency.js`
- Load: 100 virtual users, 1 iteration each, targeting a single 50-unit batch
- Result: 100/100 requests returned expected status (200 or 409); database verified afterward — `QuantityReserved` never exceeded `QuantityOnHand` (50)
- Evidence location: captured in project chat log / terminal output; verified via direct SQL query against `InventoryBatches`