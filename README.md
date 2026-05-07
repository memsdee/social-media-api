nhieuchuyen.online

Demo Production-grade social platform backend — .NET 10

## Tech Stack
- *.NET Core 10*
- *Kafka*
- *Debezium*
- *PostgreSQL*
- *MongoDB*
- *Redis*
- *Hangfire*
- *xUnit*
- *Docker & Nginx* for deployment

## Architecture
- **Clean Architecture** — Domain → Application → Infrastructure → API, strict dependency direction
- **Vertical Slice Architecture with MediatR** - Command/Query, Handler, DTO, and all other components of a feature are organized in the same folder
- **CQRS** — commands write to PostgreSQL, queries read from MongoDB

## Core Patterns
- **Outbox Pattern** — Debezium CDC → Kafka → MongoDB sync
- **Repository Pattern** — Purpose-specific interfaces, no generic `IRepository<T>`
- **Unit of Work** — Transaction abstraction over EF Core
- **Dependency Injection (DI)** — constructor-based service injection
- **Pipeline Behaviors** — MediatR + FluentValidation
- **Options Pattern** — strongly-typed configuration via IOptions<T> injected into handlers

## Practices
- **Cursor Pagination** — Composite cursor on MongoDB
- **OffSet Pagination** — Used for features within the dashboard
- **Dead Letter Topic** — BaseKafkaConsumer with 3-attempt retry + DLT routing
- **Global Exception Middleware** — centralized domain exception → HTTP status mapping

