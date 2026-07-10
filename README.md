# FCG-NotificationsAPI

Microservice responsible for simulating the sending of e-mails — welcome e-mails on user registration and purchase confirmation e-mails on approved payments — by logging them to the console.

Part of **FIAP Cloud Games (FCG)** — Tech Challenge Phase 2.

## Tech Stack

- .NET 10 / ASP.NET Core
- MassTransit + RabbitMQ
- Swagger / OpenAPI

## Endpoints

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| `GET` | `/health` | Health check | No |

> NotificationsAPI is fully event-driven — it has no user-facing endpoints. It only reacts to events published by UsersAPI and PaymentsAPI.

## Events

| Direction | Event | Trigger |
|-----------|-------|---------|
| Consumes | `UserCreatedEvent` | Logs a simulated welcome e-mail |
| Consumes | `PaymentProcessedEvent` | Logs a simulated purchase confirmation e-mail, only when `Status` is `Approved` |

## Event-Driven Flow

```
UsersAPI publishes UserCreatedEvent
  → NotificationsAPI consumes and logs "welcome e-mail sent"

PaymentsAPI publishes PaymentProcessedEvent
  → NotificationsAPI consumes
    → if Approved: logs "purchase confirmation e-mail sent"
    → if Rejected: logs the rejection, no e-mail simulated
```

## Environment Variables

| Variable | Description |
|----------|-------------|
| `RabbitMq__Host` | RabbitMQ hostname |
| `RabbitMq__Username` | RabbitMQ username |
| `RabbitMq__Password` | RabbitMQ password |
| `RabbitMq__UserCreatedQueue` | Queue name for `UserCreatedEvent` |
| `RabbitMq__PaymentProcessedQueue` | Queue name for `PaymentProcessedEvent` |

## Running Locally

### Docker Compose (via FCG-Orchestration)

```bash
cd FCG-Orchestration
docker compose up --build
```

Watch notification logs:

```bash
docker compose logs -f notifications_api
```

### Kubernetes

```bash
# Build the image first
cd FCG-NotificationsAPI
docker build -t fcg-notifications-api:latest -f services/NotificationsAPI/Dockerfile .

# Apply manifests
cd k8s
kubectl apply -f .

# Verify
kubectl get pods
kubectl logs -f deployment/notifications-api
```

## Solution Structure

```
FCG-NotificationsAPI/
├── FCG-NotificationsAPI.sln
├── contracts/
│   └── FCG.Contracts/        # Shared event contracts
├── services/
│   └── NotificationsAPI/     # Main service project
└── k8s/                      # Kubernetes manifests
    ├── deployment.yaml
    ├── service.yaml
    ├── configmap.yaml
    └── secret.yaml
```

## Related Repositories

- [FCG-Orchestration](https://github.com/posgraduacaofiapnet/FCG-Orchestration) — Docker Compose + global K8s infra
- [FCG-UsersAPI](https://github.com/posgraduacaofiapnet/FCG-UsersAPI)
- [FCG-CatalogAPI](https://github.com/posgraduacaofiapnet/FCG-CatalogAPI)
- [FCG-PaymentsAPI](https://github.com/posgraduacaofiapnet/FCG-PaymentsAPI)
