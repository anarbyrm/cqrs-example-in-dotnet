# CQRS Example in .NET

A reference implementation of **CQRS** (Command Query Responsibility Segregation) combined with the **Transactional Outbox** pattern, built with ASP.NET Core 8.

## What this project demonstrates

The main goal of this project is to show how to separate the write and read paths of an application and keep them in sync reliably, without relying on distributed transactions or 2PC.

Concretely, it demonstrates:

- **Command/query separation** — writes and reads go through distinct models, repositories, and datastores instead of a single shared one.
- **Transactional outbox** — when a command changes write-side state, the resulting domain event is written to an `Outbox` table in the *same database transaction*, so the state change and the event are never persisted inconsistently with each other.
- **Asynchronous read-model projection** — a background worker polls the outbox, publishes pending events to a message broker, and a separate consumer projects those events into the read store. The read model is eventually consistent with the write model.
- **Outbox lifecycle management** — beyond publishing, the project also tracks delivery attempts/success and periodically prunes old processed events, which are the parts of the outbox pattern that example projects often skip.
- **MediatR-based request pipeline** — commands and queries are modeled as `IRequest`/`IRequestHandler` pairs, keeping controllers thin and each use case isolated in its own handler.

### Steps taken to build it

1. Started with a single ASP.NET Core Web API project (`CqrsExample`) using MediatR to route commands and queries to dedicated handlers, keeping controllers as thin HTTP adapters.
2. Modeled the **write side** on PostgreSQL via EF Core (`CommandDbContext`), including the `Product` entity and the `Outbox` table that stores pending domain events.
3. Modeled the **read side** on MongoDB (`ProductDocument`), optimized for the queries the API actually serves (listing/paging products), decoupled from the write-side schema.
4. Implemented `CreateProductCommandHandler` to write the product and its `ProductCreated` outbox event inside one database transaction — the core of the outbox pattern.
5. Added `OutboxEventScanner`, a background service that polls unprocessed outbox rows, publishes each one to RabbitMQ, and records the attempt/success back on the row.
6. Added `OutboxEventConsumer`, a background service that subscribes to the RabbitMQ exchange and upserts the corresponding read-model document (e.g. projecting `ProductCreated` into MongoDB).
7. Added `OutboxEventCleaner`, a background service that periodically deletes old, already-processed outbox events so the table doesn't grow unbounded.
8. Exposed an `OutboxController` endpoint to inspect outbox events directly, useful for observing the pattern end-to-end while testing.
9. Containerized the app and its dependencies (Postgres, MongoDB, RabbitMQ) with a `Dockerfile` and `docker-compose.yml` for a one-command local environment.

### What is used and why

| Technology | Role | Why |
|---|---|---|
| **ASP.NET Core 8 (Web API)** | HTTP host | Minimal, modern hosting model with built-in DI, config, and Swagger support. |
| **MediatR** | In-process request/response pipeline | Cleanly separates commands and queries into independent handlers instead of bloating controllers, which is the backbone of the CQRS split. |
| **PostgreSQL + EF Core + Npgsql** | Write store | Relational integrity and transactional guarantees are exactly what's needed to atomically persist an entity *and* its outbox event together. |
| **Outbox table (`Outbox` entity, `OutboxRepository`)** | Reliable event handoff | Avoids the dual-write problem (DB write succeeds, message publish fails, or vice versa) without needing distributed transactions. |
| **RabbitMQ (`RabbitMQ.Client`)** | Message broker | Decouples publishing outbox events from projecting them; a topic exchange (`domain-events`) lets multiple event types/consumers coexist via routing keys. |
| **MongoDB (`MongoDB.Driver`)** | Read store | Schema-flexible, query-optimized store for the read side, independent of the write-side relational schema — the essence of CQRS. |
| **Background services (`BackgroundService`)** | Outbox scanner, consumer, cleaner | ASP.NET Core's built-in hosted-service model is sufficient to run polling/consuming/cleanup loops alongside the API without extra infrastructure. |
| **Swashbuckle (Swagger/OpenAPI)** | API exploration | Lets you try the endpoints from a browser in development without a separate HTTP client. |
| **Docker Compose** | Local environment | Spins up Postgres, MongoDB, and RabbitMQ (with health checks) together with the app so the whole stack runs with one command. |

## Architecture at a glance

```
POST /api/products
        │
        ▼
CreateProductCommandHandler ── (single DB transaction) ──► Postgres: Product + Outbox row
                                                                       │
                                                     OutboxEventScanner │ (polls every 10s)
                                                                       ▼
                                                                  RabbitMQ (topic exchange: domain-events)
                                                                       │
                                                          OutboxEventConsumer │ (subscribes to queue "outbox-events")
                                                                       ▼
                                                              MongoDB: ProductDocument

GET /api/products  ──► ProductReadRepository ──► MongoDB
GET /api/outbox    ──► OutboxRepository      ──► Postgres (inspect outbox state)

OutboxEventCleaner ──► deletes old processed Outbox rows (runs once a day)
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (only needed if running the API outside Docker)
- [Docker](https://docs.docker.com/get-docker/) and Docker Compose (recommended way to run everything)

## Running with Docker Compose (recommended)

This spins up the API along with Postgres, MongoDB, and RabbitMQ, pre-wired with matching connection strings:

```bash
docker compose up --build
```

Once containers are healthy:

- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger` (Development environment only)
- RabbitMQ management UI: `http://localhost:15672` (user: `user` / password: `example`)
- Postgres: `localhost:5432` (user: `postgres` / password: `example`, db: `cqrsdb`)
- MongoDB: `localhost:27017` (user: `mongouser` / password: `example`)

Apply EF Core migrations against the containerized Postgres before using the API (see below).

To stop everything:

```bash
docker compose down
```

Add `-v` to also drop the named volumes (`pgdata`, `mongodata`, `rabbitmqdata`) if you want a clean slate.

## Running locally (without Docker for the app)

You'll still need Postgres, MongoDB, and RabbitMQ available — either run just those three services from Compose, or point `appsettings.Development.json` at your own instances.

```bash
# start only the infrastructure dependencies
docker compose up postgres mongo rabbitmq
```

Then, from the repo root:

```bash
cd CqrsExample

# restore & build
dotnet restore
dotnet build

# apply EF Core migrations (creates Product/Outbox tables in Postgres)
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef database update

# run the API
dotnet run
```

By default (`launchSettings.json`) the API listens on the ports Kestrel assigns for the `https`/`http` profiles; check the console output on startup for the exact URL, or use the Docker Compose setup above where the port is fixed to `5000`.

## Configuration

Connection strings and broker settings are read from `appsettings.json` / `appsettings.Development.json`, and can be overridden with environment variables (as done in `docker-compose.yml`), following standard ASP.NET Core configuration precedence:

| Setting | Purpose |
|---|---|
| `ConnectionStrings:Postgres` | Write-side database (products, outbox) |
| `ConnectionStrings:Mongo` | Read-side database (product documents) |
| `RabbitMQ:Host` / `Port` / `Username` / `Password` / `VirtualHost` | Broker connection |
| `RabbitMQ:ExchangeName` | Topic exchange used to publish/consume domain events (default: `domain-events`) |

## API endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/products` | Create a product (write side); enqueues a `ProductCreated` outbox event in the same transaction |
| `GET` | `/api/products?size=&pageNumber=` | List products (read side, paginated) |
| `GET` | `/api/outbox?size=&pageNumber=` | Inspect outbox events and their processing status |

A sample `.http` request file is available at [CqrsExample.http](CqrsExample/CqrsExample.http) for use with the REST Client in VS Code or a similar tool.
