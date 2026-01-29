
# FoodHub User Service Microservice

A production-ready user management microservice for FoodHub, built on .NET 9 using **Clean Architecture** and microservice best practices. Designed for independent deployment, scalability, and maintainability.

## Project Overview

**Architecture Style**: Clean Architecture, Microservice  
**Database Strategy**: Cosmos DB, user container  
**API Pattern**: GraphQL-first with Hot Chocolate  
**Observability**: Structured logging with distributed tracing  
**Deployment Model**: Standalone microservice

## High-Level Architecture

### Core Principles

- **Strict Layer Boundaries**: Dependencies flow inward toward Domain
- **Service Autonomy**: User service owns its data and business logic
- **Infrastructure Abstraction**: Domain entities never leak to external layers
- **Event-Driven Ready**: Architecture supports future event sourcing patterns

### Layered Structure

```
┌──────────────┐    GraphQL     ┌──────────────┐
│   Client     │ ──────────────►│ FoodHub.Api  │ (Presentation)
└──────────────┘                └──────────────┘
                 │
             ┌───────┼───────┐
             ▼       ▼       ▼
           Application  Domain  Infrastructure
             │                │
             ▼                ▼
            Cosmos DB         Logging
```


## Folder & Project Structure


```
FoodHub-User-Service/
├── src/
│   ├── FoodHub.Api/                       # Presentation Layer (GraphQL, DI, config)
│   └── FoodHub.User/
│       ├── FoodHub.User.Domain/            # Domain logic (entities, value objects)
│       ├── FoodHub.User.Application/       # Application layer (use cases, interfaces)
│       └── FoodHub.User.Infrastructure/    # Infrastructure (Cosmos DB, persistence)
├── test/
│   ├── FoodHub.User.Application.Tests/     # Application layer tests
│   └── FoodHub.User.Domain.Tests/          # Domain layer tests
├── foodhub-user.sln                       # Solution file
├── README.md
└── ...
```


## Execution Flow

### Request Processing Pipeline

```
1. Client Request
  └─► /graphql (POST)

2. API Layer (FoodHub.Api)
  ├─► Correlation ID Middleware (injects X-Correlation-ID)
  ├─► Hot Chocolate GraphQL Engine
  └─► Query/Mutation Resolver (UserQuery/UserMutation)

3. Application Layer (FoodHub.User.Application)
  ├─► Use Case Command/Query (e.g., CreateUserCommand.ExecuteAsync())
  ├─► Input Validation & Business Rule Application
  └─► Repository Interface Invocation (IUserRepository.AddAsync())

4. Infrastructure Layer (FoodHub.User.Infrastructure)
  ├─► CosmosContext (resolves container from configuration)
  ├─► Domain Entity → Document Model Mapping (User → UserDocument)
  ├─► Cosmos DB SDK Operations (CreateItemAsync, QueryIterator)
  └─► Document Model → Domain Entity Mapping (UserDocument → User)

5. Response Pipeline
  ├─► Domain Entity → DTO Mapping (User → UserDto)
  ├─► GraphQL Response Serialization
  └─► HTTP Response with Correlation ID Header
```

### Cross-Module Communication Flow

```
Menu Module (CreateMenuCommand)
├─► Validates Restaurant existence
├─► Calls IRestaurantReadRepository.ExistsAsync(restaurantId)
├─► DI Container resolves to Restaurant.Infrastructure.RestaurantRepository
├─► RestaurantRepository.ExistsAsync() queries Restaurants container
└─► Returns boolean result to Menu module
```


## Cosmos DB Design & Partitioning

### Database Architecture

```
Cosmos Account: FoodHub-Production
├─► Database: FoodHubDb
    └─► Container: Users
        ├─► Partition Key: /id
        ├─► Documents: UserDocument
        └─► Typical Size: 100K-1M users
```

### Partitioning Strategy

**Users Container (`/id`)**:
- **Rationale**: Even distribution across user IDs
- **Query Patterns**: Point reads by user ID, cross-partition scans for GetAll
- **Scaling**: Horizontal scale based on user count

### Document Model

**UserDocument**:
```json
{
  "id": "user-guid",
  "email": "user@email.com",
  "name": "John Doe",
  "isActive": true
}
```


## GraphQL Design

### API Surface

**Endpoint**: `/graphql`  
**Development UI**: `/graphql` (Banana Cake Pop embedded)  
**Schema Introspection**: Enabled in Development only  

### Query Operations

```graphql
type Query {
  # User Queries
  getAllUsers: [UserDto!]!
  getUserById(id: ID!): UserDto
  getUserByEmail(email: String!): UserDto
}
```

### Mutation Operations

```graphql
type Mutation {
  # User Mutations
  createUser(input: CreateUserDto!): ID!
  updateUser(input: UpdateUserDto!): Void
  deactivateUser(id: ID!): Void
}
```


### Error Handling
# FoodHub Restaurant Service Microservice

A production-ready restaurant management microservice for FoodHub, built on .NET 9 using **Clean Architecture** and microservice best practices. Designed for independent deployment, scalability, and maintainability.

## Project Overview

**Architecture Style**: Clean Architecture, Microservice  
**Database Strategy**: Cosmos DB, restaurant container  
**API Pattern**: GraphQL-first with Hot Chocolate  
**Observability**: Structured logging with distributed tracing  
**Deployment Model**: Standalone microservice

## High-Level Architecture

### Core Principles

- **Strict Layer Boundaries**: Dependencies flow inward toward Domain
- **Service Autonomy**: Restaurant service owns its data and business logic
- **Infrastructure Abstraction**: Domain entities never leak to external layers
- **Event-Driven Ready**: Architecture supports future event sourcing patterns

### Layered Structure

```
┌──────────────┐    GraphQL     ┌──────────────┐
│   Client     │ ─────────────►│ FoodHub.Api  │ (Presentation)
└──────────────┘                └──────────────┘
                 │
             ┌───────┬───────┐
             ▼       ▼       ▼
           Application  Domain  Infrastructure
             │                │
             ▼                ▼
            Cosmos DB         Logging
```

## Folder & Project Structure

```
FoodHub-Restaurant-Service/
├── src/
│   ├── FoodHub.Restaurant/
│   │   ├── FoodHub.Api/                       # Presentation Layer (GraphQL, DI, config)
│   │   ├── FoodHub.Restaurant.Domain/         # Domain logic (entities, value objects)
│   │   ├── FoodHub.Restaurant.Application/    # Application layer (use cases, interfaces)
│   │   └── FoodHub.Restaurant.Infrastructure/ # Infrastructure (Cosmos DB, persistence)
├── test/
│   ├── FoodHub.Restaurant.Application.Tests/  # Application layer tests
│   └── FoodHub.Restaurant.Domain.Tests/       # Domain layer tests
├── foodhub-restaurant-service.sln             # Solution file
├── README.md
└── ...
```

## Execution Flow

### Request Processing Pipeline

```
1. Client Request
  └─► /graphql (POST)

2. API Layer (FoodHub.Api)
  ├─► Correlation ID Middleware (injects X-Correlation-ID)
  ├─► Hot Chocolate GraphQL Engine
  └─► Query/Mutation Resolver (RestaurantQuery/RestaurantMutation)

3. Application Layer (FoodHub.Restaurant.Application)
  ├─► Use Case Command/Query (e.g., CreateRestaurantCommand.ExecuteAsync())
  ├─► Input Validation & Business Rule Application
  └─► Repository Interface Invocation (IRestaurantRepository.AddAsync())

4. Infrastructure Layer (FoodHub.Restaurant.Infrastructure)
  ├─► CosmosContext (resolves container from configuration)
  ├─► Domain Entity → Document Model Mapping (Restaurant → RestaurantDocument)
  ├─► Cosmos DB SDK Operations (CreateItemAsync, QueryIterator)
  └─► Document Model → Domain Entity Mapping (RestaurantDocument → Restaurant)

5. Response Pipeline
  ├─► Domain Entity → DTO Mapping (Restaurant → RestaurantDto)
  ├─► GraphQL Response Serialization
  └─► HTTP Response with Correlation ID Header
```

### Cross-Module Communication Flow

```
Menu Module (CreateMenuCommand)
├─► Validates Restaurant existence
├─► Calls IRestaurantReadRepository.ExistsAsync(restaurantId)
├─► DI Container resolves to Restaurant.Infrastructure.RestaurantRepository
├─► RestaurantRepository.ExistsAsync() queries Restaurants container
└─► Returns boolean result to Menu module
```

## Cosmos DB Design & Partitioning

### Database Architecture

```
Cosmos Account: FoodHub-Production
├─► Database: FoodHubDb
    └─► Container: Restaurants
        ├─► Partition Key: /id
        ├─► Documents: RestaurantDocument
        └─► Typical Size: 10K-100K restaurants
```

### Partitioning Strategy

**Restaurants Container (`/id`)**:
- **Rationale**: Even distribution across restaurant IDs
- **Query Patterns**: Point reads by restaurant ID, cross-partition scans for GetAll
- **Scaling**: Horizontal scale based on restaurant count

### Document Model

**RestaurantDocument**:
```json
{
  "id": "restaurant-guid",
  "name": "Pizza Hut",
  "city": "New York",
  "isActive": true
}
```

## GraphQL Design

### API Surface

**Endpoint**: `/graphql`  
**Development UI**: `/graphql` (Banana Cake Pop embedded)  
**Schema Introspection**: Enabled in Development only  

### Query Operations

```graphql
type Query {
  # Restaurant Queries
  getAllRestaurants: [RestaurantDto!]!
  getRestaurantById(id: ID!): RestaurantDto
}
```

### Mutation Operations

```graphql
type Mutation {
  # Restaurant Mutations
  createRestaurant(input: CreateRestaurantDto!): ID!
  updateRestaurant(input: UpdateRestaurantDto!): Void
  deactivateRestaurant(id: ID!): Void
}
```

### Error Handling

- **Domain Exceptions**: Mapped to GraphQL field errors with appropriate error codes
- **Validation Errors**: Input validation failures return structured error messages
- **Infrastructure Failures**: Cosmos exceptions mapped to generic GraphQL errors (details logged with Correlation ID)

## Logging & Observability

### Logging Architecture

**Provider**: Serilog with structured logging  
**Sinks**: Console (structured JSON), Debug  
**Context Enrichment**: Correlation ID, operation metadata  

### Correlation & Tracing

```
Request Flow Tracing:
┌─────────────────┐ X-Correlation-ID: abc-123
│   HTTP Request  │ ──────────────────────────────┐
└─────────────────┘                             │
                                               ▼
┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ Serilog LogContext (Per-Request Scope)                     │
│ CorrelationId: abc-123                                     │
│ ├─► [API] Begin: CreateRestaurant mutation                 │
│ ├─► [Application] Use Case: Creating restaurant            │
│ ├─► [Infrastructure] Calling Cosmos DB to insert document  │
│ ├─► [Infrastructure] Successfully inserted restaurant      │
│ ├─► [Application] Successfully created restaurant          │
│ └─► [API] Success: Created restaurant with ID xyz          │
└────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Logging Boundaries

**API Layer**: Request entry/exit, mutation/query results, error responses  
**Application Layer**: Use case execution start/completion  
**Infrastructure Layer**: Database operations, external service calls  
**Domain Layer**: NO LOGGING (pure business logic)  

### Sample Log Entry

```json
{
  "@timestamp": "2026-01-17T10:30:00.123Z",
  "@level": "Information", 
  "@messageTemplate": "Success: Created restaurant {RestaurantName} with Id {RestaurantId}",
  "RestaurantName": "Pizza Hut",
  "RestaurantId": "550e8400-e29b-41d4-a716-446655440000",
  "CorrelationId": "abc-123-def-456",
  "SourceContext": "FoodHub.Api.GraphQL.Mutations.RestaurantMutation"
}
```

## Microservice Readiness

### Decomposition Strategy

This service is architected for easy independent deployment and future event-driven evolution:

1. **High Cohesion**: All restaurant logic in `FoodHub.Restaurant` namespace
2. **Loose Coupling**: No direct dependencies on other modules
3. **Data Isolation**: Separate Cosmos container for restaurants
4. **API Contracts**: GraphQL schema serves as stable API contract

### Extraction Process (Example: Restaurant Module)

```
Step 1: Create New Microservice Solution
├─► Copy FoodHub.Restaurant.* projects
├─► Add new FoodHub.Api project
└─► Configure independent Cosmos DB access

Step 2: Update Original Monolith  
├─► Replace RestaurantQuery/RestaurantMutation with HTTP client calls
├─► Update IRestaurantReadRepository implementation to call REST API
└─► Remove Restaurant module projects

Step 3: Deploy & Route
├─► Deploy Restaurant microservice independently
├─► Update API Gateway routing (/graphql/restaurant → Restaurant service)
└─► Maintain GraphQL federation or schema stitching
```

### Service Boundaries

**Restaurant Service**: Restaurant aggregate, onboarding, management  
**Menu Service**: Menu/MenuItem aggregates, pricing, inventory  
**Order Service** (Future): Order processing, cart management, checkout  
**Payment Service** (Future): Payment processing, billing, refunds  

## Local Development Setup

### Prerequisites

- .NET 9 SDK
- Azure Cosmos DB Emulator OR Azure Cosmos DB account
- Visual Studio 2022 / VS Code / Rider

### Configuration Setup

1. **Cosmos DB Configuration** (`appsettings.json`):
```json
{
  "Cosmos": {
    "Endpoint": "https://localhost:8081",  // Emulator
    "Key": "cosmos key",
    "DatabaseName": "FoodHubDb",
    "Containers": {
      "Restaurant": { "Name": "Restaurants" }
    }
  }
}
```

2. **Container Creation** (Azure Portal or Emulator):
```
Database: FoodHubDb
└─► Container: Restaurants (Partition: /id)
```

### Build & Run Commands

```bash
# Clean build
dotnet clean
dotnet build

# Run API
cd src/FoodHub.Restaurant/FoodHub.Api  
dotnet run

# Access GraphQL Playground
# Navigate to: https://localhost:7161/graphql
```

### Sample Development Workflow

```bash
# 1. Create Restaurant
curl -X POST https://localhost:7161/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "mutation { createRestaurant(input: {name: \"Pizza Hut\", city: \"New York\"}) }"}'

# 2. Query Restaurant by Id
curl -X POST https://localhost:7161/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "query { getRestaurantById(id: \"550e8400-e29b-41d4-a716-446655440000\") { id name city isActive } }"}'
```

## Current Status & Roadmap

### ✅ Completed

- Restaurant domain entities and business rules
- CRUD operations via GraphQL
- Cosmos DB persistence with document mapping
- Clean Architecture and microservice structure

### 🔄 In Progress

- Build verification and integration testing
- GraphQL schema optimization

### 📋 Planned

- Event-driven integration
- Advanced patterns (CQRS, Event Sourcing)

---

**Architecture Review Status**: ✅ Senior Engineer Ready | ✅ Tech Lead Ready | ✅ Architect Interview Ready
