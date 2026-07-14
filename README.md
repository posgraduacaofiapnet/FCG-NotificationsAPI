# FCG-NotificationsAPI

Microserviço responsável por simular o envio de e-mails — e-mail de boas-vindas no cadastro de usuário e e-mail de confirmação de compra após pagamento aprovado — registrando as notificações no console via Serilog.

Parte do **FIAP Cloud Games (FCG)** — Tech Challenge Fase 2.

---

## Tecnologias

- .NET 10 / ASP.NET Core
- MassTransit + RabbitMQ
- Swagger / OpenAPI
- Serilog (logs estruturados em JSON)

---

## Endpoints

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `GET` | `/health` | Health check | Não |

> A NotificationsAPI é totalmente orientada a eventos — não possui endpoints expostos ao usuário. Ela reage exclusivamente a eventos publicados pela UsersAPI e pela PaymentsAPI.

---

## Eventos

| Direção | Evento | Comportamento |
|---------|--------|---------------|
| Consome | `UserCreatedEvent` | Loga um e-mail de boas-vindas simulado |
| Consome | `PaymentProcessedEvent` | Loga e-mail de confirmação de compra apenas quando `Status` é `Approved` |

---

## Fluxo Event-Driven

```
UsersAPI publica UserCreatedEvent
  → NotificationsAPI consome
    → Loga: "E-mail de boas-vindas enviado para <email>"

PaymentsAPI publica PaymentProcessedEvent
  → NotificationsAPI consome
    → se Approved: Loga "E-mail de confirmação de compra enviado para <email>"
    → se Rejected: Loga a rejeição, nenhum e-mail simulado
```

Todos os logs incluem `CorrelationId`, permitindo rastrear toda a cadeia de eventos de uma mesma operação nos logs do cluster.

---

## Variáveis de Ambiente

| Variável | Descrição |
|----------|-----------|
| `RabbitMq__Host` | Hostname do RabbitMQ |
| `RabbitMq__Username` | Usuário do RabbitMQ |
| `RabbitMq__Password` | Senha do RabbitMQ |
| `RabbitMq__UserCreatedQueue` | Nome da fila para `UserCreatedEvent` |
| `RabbitMq__PaymentProcessedQueue` | Nome da fila para `PaymentProcessedEvent` |

---

## Executando Localmente

### Docker Compose (via FCG-Orchestration)

```bash
cd FCG-Orchestration
docker compose up --build
```

Acompanhar logs de notificações:

```bash
docker compose logs -f notifications-api
```

### Kubernetes

```bash
# 1. Build da imagem local
cd FCG-NotificationsAPI
docker build -t fcg-notifications-api:latest -f services/NotificationsAPI/Dockerfile .

# 2. Aplique a infra (RabbitMQ) primeiro
cd ../FCG-Orchestration/k8s
kubectl apply -f .

# 3. Aplique os manifestos da NotificationsAPI
cd ../../FCG-NotificationsAPI/k8s
kubectl apply -f .

# 4. Verifique os pods
kubectl get pods
kubectl get services

# 5. Acompanhe os logs (notificações aparecem aqui)
kubectl logs -f deployment/notifications-api
```

#### Manifestos Kubernetes

| Arquivo | Tipo | Descrição |
|---------|------|-----------|
| `deployment.yaml` | Deployment | Define o Pod com 1 réplica, imagem, probes e referência a ConfigMap/Secret |
| `service.yaml` | Service | Expõe a API internamente no cluster na porta 80 |
| `configmap.yaml` | ConfigMap | Configurações não-sensíveis (RabbitMQ host/username, nomes das filas) |
| `secret.yaml` | Secret | Dados sensíveis em base64 (RabbitMQ password) |

As **readinessProbe** e **livenessProbe** do Deployment apontam para `/health` — o pod só recebe tráfego após o healthcheck passar.

---

## Testes Unitários

```bash
cd FCG-NotificationsAPI
dotnet test FCG-NotificationsAPI.sln
```

Os testes utilizam **xUnit** e **Bogus** para geração de dados fictícios.

---

## Estrutura da Solution

```
FCG-NotificationsAPI/
├── FCG-NotificationsAPI.sln
├── contracts/
│   └── FCG.Contracts/           # Contratos de eventos compartilhados
├── services/
│   └── NotificationsAPI/        # Projeto principal do serviço
├── tests/
│   └── NotificationsAPI.Tests/  # Testes unitários (xUnit)
└── k8s/                         # Manifestos Kubernetes
    ├── deployment.yaml
    ├── service.yaml
    ├── configmap.yaml
    └── secret.yaml
```

---

## Repositórios Relacionados

- [FCG-Orchestration](https://github.com/posgraduacaofiapnet/FCG-Orchestration) — Docker Compose + infraestrutura K8s global
- [FCG-UsersAPI](https://github.com/posgraduacaofiapnet/FCG-UsersAPI)
- [FCG-CatalogAPI](https://github.com/posgraduacaofiapnet/FCG-CatalogAPI)
- [FCG-PaymentsAPI](https://github.com/posgraduacaofiapnet/FCG-PaymentsAPI)
