Containerized E-Commerce Microservices Backend project for the CSCI 6844 | Programming for the Internet course at Fairleigh Dickinson University.

All services and processes are set according to the project requirements given by the course's professor.

The application consists of a containerized distributed system for a simplified e-commerce platform using ASP.NET Core, EF Core, Blazor WebAssembly, Docker, and Docker Compose.

## Services

| Service | Port | Description |
|---|---|---|
| API Gateway (Ocelot) | 5000 | Centralized entry point; routes all API calls |
| CustomerService | 5001 | Manages customer records (name, email, phone number) |
| OrderService | 5002 | Creates and tracks orders; orchestrates validation and shipment |
| ProductService | 5003 | Manages product catalog (name, price, stock) |
| ShippingService | 5004 | Auto-creates a shipment record when an order is placed |
| Frontend (Blazor WASM) | 5005 | Web UI for browsing and managing data |
| RabbitMQ Management | 15672 | Message broker dashboard |

## Architecture

- **API Gateway**: All browser and service-to-service communication goes through the Ocelot gateway on port 5000. The frontend calls `http://localhost:5000/api/...` for all requests.
- **Async Messaging**: Services communicate via RabbitMQ events (e.g., `customer_created`, `order_placed`) for decoupled inter-service workflows.
- **DTO Design**: All public-facing endpoints use Data Transfer Objects to reduce coupling and control exposed data.
- **Aggregated Endpoint**: `GET /api/aggregated/order-details/{id}` returns a combined view of order + customer + product data.

## Running the project

```bash
docker compose up --build
```

The frontend will be available at **http://localhost:5005**.

## Frontend

The Blazor WebAssembly frontend provides three pages:

- **Products** (`/products`): Lists all products and allows adding new ones.
- **Customers** (`/customers`): Lists all customers and allows adding new ones (name, email, phone number).
- **Orders** (`/orders`): Lists all orders and allows placing new ones.

All form submissions call the API Gateway at port 5000 and persist data to the respective microservice database.

## Changelog

### Latest deliverable
- Added Blazor WebAssembly frontend with Products, Customers, and Orders pages
- Implemented POST functionality for all three entities via the API Gateway
- Added `PhoneNumber` field to CustomerService (model, DTOs, migration, controller)
- Added `Frontend` service to `docker-compose.yml` (port 5005, served by nginx)
- Created `Dockerfile` for the Frontend using a two-stage build (SDK → nginx:alpine)
- Fixed Ocelot route priority conflict that prevented `GET /api/customers` from resolving
- Added `.gitignore` to exclude build artifacts and SQLite runtime databases

### Previous deliverable
- Implementation of asynchronous messaging using RabbitMQ
- Implementation of an API Gateway for centralized access (Ocelot)
- DTO-based design to improve data handling and reduce coupling
- ProductService stock decremented automatically by OrderService
- Aggregated endpoint: order + product + customer data
- Updated `docker-compose.yml`
- Implementation of Order Deletion

