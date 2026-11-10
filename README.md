# FCG-NotificationsAPI

> **Legado da Fase 2.** Este repositório não participa da stack atual do Orchestration.
> Na evolução das Fases 3 e 4, UsersAPI/CatalogAPI persistem notificações em outbox SQL;
> `FCG-Outbox-Processor` publica na SQS e `FCG-Notifications-Lambda` processa as mensagens.
> O contrato atual de pagamento não deve ser tratado como compatível automaticamente com esta
> cópia histórica. Os fluxos abaixo descrevem a implementação antiga, não a entrega da Fase 4.

Para a entrega atual, consultar a [documentação central](https://github.com/posgraduacaofiapnet/FCG-Orchestration)
e a [matriz da Fase 4](https://github.com/posgraduacaofiapnet/FCG-Orchestration/blob/master/docs/FASE4.md).
Não usar este serviço junto do fluxo novo para comprovar notificações sem preparar um ambiente
legado isolado e conferir os contratos.

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

> A NotificationsAPI histórica é orientada a eventos e possui somente endpoint de saúde.
> A UsersAPI atual não publica `UserCreatedEvent` diretamente no RabbitMQ.

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

### Stack atual

O Compose atual do `FCG-Orchestration` não define `notifications-api`; portanto não há
comando `docker compose logs notifications-api` nesse ambiente. Para executar o fluxo novo,
seguir o README do Orchestration com os repositórios do worker e da Lambda presentes.

### Kubernetes

Somente para inspeção ou ambiente legado isolado. O procedimento abaixo não é implantação da
Fase 4 nem garante compatibilidade com os publicadores atuais. A infraestrutura compartilhada
usa Kustomize, e a imagem local deve estar acessível ao cluster com o nome usado pelo Deployment.

```bash
# 1. Build da imagem local
cd FCG-NotificationsAPI
docker build -t fcg-notifications-api:latest -f services/NotificationsAPI/Dockerfile .

# 2. Aplique a infra (RabbitMQ) primeiro
cd ../FCG-Orchestration
kubectl apply -k k8s

# 3. Aplique os manifestos da NotificationsAPI
cd ../FCG-NotificationsAPI/k8s
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
| `secret.yaml` | Secret | Configuração sensível histórica; não é solução de zero credenciais fixas para a Fase 4 |

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
│   └── NotificationsAPI.UnitTests/ # Testes unitários (xUnit)
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
