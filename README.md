# DataStatisticsService

Consumes ingested data from RabbitMQ, persists and aggregates readings, and exposes GraphQL/REST APIs plus real-time SignalR notifications.

## Architecture

| Deployable | Role | Default port |
|------------|------|--------------|
| `data-statistics` (Processor Host) | RabbitMQ consumer, raw storage, aggregation | 5100 |
| `data-statistics-gateway` | GraphQL + REST query API | 5200 |
| `data-statistics-notification` | SignalR hub for live updates | 5300 |
| `data-statistics-frontend` | Angular dashboard | 4200 |

## Prerequisites

- Docker Desktop with Compose v2
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- DataIngestorService stack running first (provides WeakApp, RabbitMQ, and `app-network`)

## Local startup order

1. Start the ingestor stack (from parent repo or `DataIngestorService/`):
   ```bash
   cd DataIngestorService
   docker compose up -d
   ```
2. Start the statistics stack:
   ```bash
   cd DataStatisticsService
   docker compose up -d --build
   ```
3. Or from the parent repo root:
   ```bash
   docker compose up -d --build
   ```

## Environment

Copy `.env` defaults or adjust ports/credentials:

| Variable | Description |
|----------|-------------|
| `STATISTICS_PORT` | Processor API port (5100) |
| `GATEWAY_PORT` | GraphQL/REST gateway (5200) |
| `NOTIFICATION_PORT` | SignalR service (5300) |
| `FRONTEND_PORT` | Angular UI (4200) |
| `POSTGRES_*` | PostgreSQL credentials |
| `RABBITMQ_*` | Must match DataIngestorService |
| `RABBITMQ_STATISTICS_UPDATES_*` | Processor → Notification topology |

## Endpoints

| Service | URL |
|---------|-----|
| Processor health | http://localhost:5100/health/ready |
| GraphQL | http://localhost:5200/graphql |
| GraphQL IDE (dev) | http://localhost:5200/graphql (Banana Cake Pop) |
| REST latest readings | http://localhost:5200/api/readings/latest |
| SignalR hub | http://localhost:5300/hubs/statistics |
| Frontend | http://localhost:4200 |
| pgAdmin | http://localhost:5050 |

## Manual E2E smoke test

1. `docker compose up -d` from parent repo
2. Wait ~30s for ingestor poll cycle
3. Query PostgreSQL: `SELECT count(*) FROM raw_ingested_events;` — expect rows > 0
4. Open frontend at http://localhost:4200 — dashboard shows latest readings
5. Watch live updates on dashboard after each ingestor poll

## Troubleshooting

- **Readiness check fails**: verify RabbitMQ and PostgreSQL are healthy (`docker compose ps`)
- **No messages consumed**: ensure ingestor stack started first and queue `ingested-data-processing-queue` exists in RabbitMQ management UI (http://localhost:15672)
- **Failed messages**: check Wolverine error queue in RabbitMQ management UI
- **Empty dashboard**: confirm gateway can reach PostgreSQL and aggregation tables have data

## Development

```bash
cd server
dotnet restore
dotnet test
dotnet run --project src/DataStatisticsService.Host
```

## CI

GitHub Actions workflow runs restore, build, test, and Docker image builds on push/PR.
