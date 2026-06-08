# CustomCodeFramework

CustomCodeFramework is an opinionated .NET framework designed to accelerate the development of APIs, modular monoliths and microservices by providing reusable architectural components, integrations and conventions.

The goal is to eliminate repetitive infrastructure code and allow developers to focus on business logic.

---

## Objectives

- Reduce boilerplate code.
- Standardize project architecture.
- Promote Clean Architecture and DDD principles.
- Simplify CQRS implementation.
- Provide reusable integrations for common technologies.
- Accelerate the creation of new services.
- Improve maintainability and consistency across projects.

---

## Features

### Core

Provides common primitives used across applications:

- Result pattern
- Error handling
- Value Objects
- Domain Events
- Aggregate Roots
- Entities
- Auditable Entities
- Correlation IDs
- Pagination models
- Shared abstractions

---

### CQRS

Simplifies command and query handling.

Features:

- Commands
- Queries
- Command Handlers
- Query Handlers
- Pipeline Behaviors
- Validation Pipeline
- Logging Pipeline
- Transaction Pipeline

---

### Validation

Built-in validation support.

Features:

- FluentValidation integration
- Validation behaviors
- Standardized validation responses
- Validation exception handling

---

### Redis

Provides caching and distributed lock abstractions.

Features:

- Distributed Cache
- Cache Key Builders
- Redis Health Checks
- Distributed Locks

---

### MongoDB

MongoDB integration helpers.

Features:

- Collection registration
- Repository abstractions
- Index creation helpers
- Read model support

---

### PostgreSQL

PostgreSQL integration helpers.

Features:

- DbContext registration
- Outbox support
- Inbox support
- Unit of Work abstractions
- Health checks

---

### Messaging

Messaging and event-driven architecture support.

Features:

- Integration Events
- Event Envelope
- Event Metadata
- Event Serialization
- Outbox Pattern
- Inbox Pattern

---

### Observability

Monitoring and diagnostics.

Features:

- Structured Logging
- OpenTelemetry integration
- Tracing
- Metrics
- Correlation tracking

---

## Project Structure

```txt
CustomCodeFramework
│
├── src
│   ├── CustomCodeFramework.Core
│   ├── CustomCodeFramework.Cqrs
│   ├── CustomCodeFramework.Validation
│   ├── CustomCodeFramework.Redis
│   ├── CustomCodeFramework.Mongo
│   ├── CustomCodeFramework.Postgres
│   ├── CustomCodeFramework.Messaging
│   ├── CustomCodeFramework.Observability
│
├── tests
│
├── samples
│
└── docs
```

---

## Design Principles

- Clean Architecture
- Domain-Driven Design (DDD)
- SOLID
- Vertical Slice Architecture
- Event-Driven Architecture
- Modular Design
- Convention over Configuration

---

## Long-Term Vision

CustomCodeFramework aims to become the foundation used across all internal and client projects, providing a consistent, maintainable and scalable development experience.

The framework should allow new services to be created with minimal setup while maintaining architectural standards and development best practices.

---

## License

Private Internal Framework.
