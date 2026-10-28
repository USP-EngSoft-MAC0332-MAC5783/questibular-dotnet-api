# API - DDD and Hexagonal Architecture Blueprint

Portfolio API is designed for **.NET 10** and follows the principles of **Hexagonal Architecture (Ports and Adapters)** combined with **Tactical Domain-Driven Design (DDD)**. 

The primary architectural goal is to establish strict boundaries, ensuring that the core business rules (the Domain) remain completely isolated from external concerns such as databases, web frameworks, and third-party libraries. This document outlines the planned structural design.

## Proposed Solution Structure

The system is planned to be divided into well-defined layers:

*   **Portfolio.Domain**: The core of the application. It will contain the Entities, Domain Events, and the **Ports** (Interfaces). It is designed to have zero external dependencies (no database frameworks, no web hosting logic).
*   **Portfolio.DataAccess**: The **Infrastructure** layer. This project will be responsible for persistence (planned to use PostgreSQL via EF Core), implementing the Repositories (Adapters), and handling data mapping.
*   **Portfolio.ApiBase**: A cross-cutting internal framework. It will provide base configurations for Swagger, Kestrel, CORS, and utility classes. Key planned components include the `BaseApiController`, which will standardize controller behaviors and logging, and the `WarmupService`, which will handle fail-fast initialization (like verifying database connectivity) before the application starts accepting traffic.
*   **Portfolio.Api**: The application Host. It will contain the HTTP Controllers, Application Services orchestrating the use cases, the Composition Root (Dependency Injection), and Background Workers.

## Planned DDD Concepts

### 1. Rich Entities and Strict Encapsulation
The domain entities are planned to be robust models, not just data bags (anemic models). They will protect their own state:
*   Properties will use `private set` or `protected set` to prevent unauthorized external mutations.
*   Constructors will be private to enforce the use of **Factory Methods**. These factories will ensure that all business rules and invariants are applied at the exact moment an object is created.
*   State modifications will only occur through explicit business methods, ensuring the object remains in a valid state throughout its lifecycle.

### 2. Safe Reconstitution
To allow the infrastructure layer to read data from the database and recreate domain entities without breaking encapsulation, a specific reconstitution pattern is planned. The data layer will retrieve the raw data and pass it through a dedicated internal route that restores the exact domain state, bypassing standard creation validations but maintaining object integrity.

### 3. Persistence Isolation
To avoid leaky abstractions, the Domain layer will remain entirely unaware of the database schema. 
The plan is to use anemic **Data Models** specifically within the DataAccess layer. These models will mirror the database schema and will be mapped to the rich Domain entities. Automated mapping tools will be utilized to translate between these layers, ensuring the domain logic stays clean and isolated from persistence concerns.

### 4. Domain Events and Asynchronous Processing
Secondary actions (such as sending notifications or updating audit logs) will not block the main HTTP request thread.
*   The application layer will dispatch domain events upon successful use-case execution.
*   The infrastructure will use in-memory queues to hold these events temporarily.
*   A Background Worker will consume these queues and execute the corresponding event handlers asynchronously, ensuring high performance and responsiveness for the end user.

### 5. Application Services
The outer API layer will act strictly as an orchestrator. The application services will not contain core business rules; instead, they will coordinate the flow between the Domain and Infrastructure. They will fetch data via repository interfaces, invoke the rich domain entities to validate and execute business logic, and trigger the appropriate events.