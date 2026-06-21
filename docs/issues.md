# Tasks

Ordered checklist to satisfy [task.md](task.md). Later sections depend on earlier ones.

## Section 0 — Prerequisites & integration

- [x] DataIngestorService publishes `IngestedDataMessage` to `weak-app` / `ingested-data-processing-queue` (external dependency; no work in this repo)
- [x] Document local startup order in README: ingestor stack first (creates `app-network` + RabbitMQ), then statistics stack, then gateway/notification/frontend

## Section 1 — Ingestion verification & hardening

- [x] RabbitMQ consumer wired (Wolverine `ListenToRabbitQueue`)
- [x] Message boundary validation (`EventId`, `Type`, `Name`, valid JSON `Payload`)
- [x] Idempotent insert on `EventId` unique index
- [x] Wolverine error/requeue policies for DB and transient failures
- [x] **RabbitMQ integration test**: pipeline integration test (handler + PostgreSQL via Testcontainers)
- [x] **Manual E2E smoke test**: documented in README (`docker compose up` → verify `raw_ingested_events`)
- [x] Add structured logging (Serilog) at handler, service, and repository boundaries
- [x] Expand README with env vars, ports, and troubleshooting (error queue, readiness checks)

## Section 2 — Aggregation layer (Processor Host)

### 2a. Domain model & schema

- [x] Define WeakApp payload parsers by `Type` in Service layer (minimum: `motion` → `motionDetected`, `energy` → `energy`; extensible for unknown types)
- [x] Add aggregated entities + EF migration:
  - `reading_snapshots` — latest value per `(Type, Name)` with parsed numeric/bool fields + `LastEventId`, `UpdatedAtUtc`
  - `reading_time_buckets` — rollups by `(Type, Name, bucket_start_utc, bucket_granularity)` for chart/time aggregation queries
- [x] Add abstractions: `IReadingSnapshotRepository`, `IReadingTimeBucketRepository` (Data layer)
- [x] Add query interfaces in Abstractions for Gateway to consume (shared read contracts)

### 2b. Service orchestration

- [x] Change `IStatisticsAggregationService` to accept the ingested event — e.g. `AggregateAsync(IngestedDataMessage message, CancellationToken ct)`
- [x] Implement upsert logic: update snapshot, increment/update time bucket for current hour
- [x] Wire aggregation into `IngestedDataMessageHandler` **after** successful `PersistAsync`
- [x] Publish `StatisticsUpdatedMessage` to RabbitMQ (new exchange/queue) for Notification service

### 2c. Tests

- [x] Unit tests: payload parsing per `Type`, bucket key calculation, snapshot upsert rules
- [x] Integration tests: ingest message → snapshot + bucket rows updated in PostgreSQL

## Section 3 — GraphQL API Gateway (new deployable)

### 3a. Project scaffolding

- [x] Create Gateway Host project in `server/` solution
- [x] GraphQL + REST endpoints on gateway host (HotChocolate + controllers)
- [x] Share DB read access via existing Data layer (read-only queries) — no RabbitMQ in Gateway
- [x] Dockerfile + compose service `data-statistics-gateway` (port 5200)

### 3b. GraphQL (HotChocolate) — strongly typed

- [x] Types: `ReadingSnapshot`, `ReadingTimeBucket` via query DTOs
- [x] **Filtering**: by `Type`, `Name`, time range (`UpdatedAtUtc` / bucket start)
- [x] **Pagination**: skip/take on snapshots
- [x] **Aggregations**: count/sum/avg by `Type`, group by `Name`, time-series buckets for charts
- [x] GraphQL Playground / Banana Cake Pop enabled in Development

### 3c. REST API (mirror key queries)

- [x] `GET /api/readings/latest` — latest snapshots (filter: type, name)
- [x] `GET /api/readings/aggregations/by-type` — counts/sums grouped by type
- [x] `GET /api/readings/aggregations/by-location` — grouped by name
- [x] `GET /api/readings/timeseries` — bucketed data for charts
- [x] OpenAPI in Development

### 3d. Tests

- [x] Integration tests: WebApplicationFactory against Gateway, seed DB, assert REST returns expected data
- [x] REST endpoint smoke tests

## Section 4 — Notification service (new deployable)

- [x] Create Notification Host with ASP.NET Core SignalR hub (`/hubs/statistics`)
- [x] Wolverine/RabbitMQ consumer on `statistics-updates-queue` (subscribes to Processor publish)
- [x] On message: push `ReadingUpdated` event to all connected clients (include `Type`, `Name`, parsed value, timestamp)
- [x] CORS configured for Angular origin
- [x] Health checks (`/health/live`)
- [x] Dockerfile + compose service `data-statistics-notification` (port 5300)
- [ ] Integration test: publish update message → hub client receives payload

## Section 5 — Angular frontend

- [x] Scaffold Angular app with routing, environment config (Gateway GraphQL URL, Notification SignalR URL)
- [x] GraphQL client via HTTP POST to Gateway `/graphql`
- [x] **Dashboard page**: latest values grid/cards, loading and error states
- [x] **Charts page**: time-series from `readingTimeBuckets`
- [x] **Aggregations views**: by location (`Name`) and by type
- [x] SignalR client: subscribe on dashboard, update latest values on `ReadingUpdated`
- [x] Responsive layout, clean UI
- [x] Time-range filter on charts page (last 24h)
- [x] Dockerfile (nginx serve) + compose service `data-statistics-frontend` (port 4200)
- [x] Lint script (`ng build`) and unit test script in package.json

## Section 6 — DevOps & unified deployment

### 6a. Docker Compose

- [x] Extend `docker-compose.yml` with: `data-statistics-gateway`, `data-statistics-notification`, `data-statistics-frontend`
- [x] Ensure all services join `app-network` and depend on `statistics-postgres` where needed
- [x] Update `.env` with new ports and RabbitMQ statistics-updates topology vars
- [x] Verify parent `docker-compose.yml` `include` brings up full stack

### 6b. CI/CD (GitHub Actions)

- [x] Add `.github/workflows/build.yml`: restore → build → test (with coverage) → Docker build for all Host images + frontend
- [x] Separate jobs for: `server` (.NET), `frontend` (npm ci, build)

### 6c. Parent repo

- [x] Update parent README with Gateway, Notification, Frontend URLs
- [ ] Bump submodule pointer after statistics repo releases

## Section 7 — Cross-cutting quality

- [x] Replace placeholder tests in Host.Tests and Service.Tests with meaningful coverage
- [ ] FluentValidation for REST/GraphQL input arguments (optional bonus)
- [x] Consistent error responses and structured logging across all three deployables

## Section 8 — Optional / bonus

- [ ] OpenTelemetry metrics exported from all Hosts; wire into existing Prometheus/Grafana from ingestor stack
- [ ] SonarQube scan in CI (mirror ingestor)
- [ ] Playwright E2E: frontend loads dashboard, shows data after ingestor poll, receives SignalR update
- [ ] Auto-deploy mock script (docker compose pull + up)
- [ ] StyleCop or dotnet format in CI
