# NotificationsAPI

> Legado da Fase 2, fora do Compose atual. A entrega das Fases 3 e 4 usa Outbox Processor,
> SQS e Notifications Lambda. Ver o [README principal](../../README.md) e a
> [documentação atual](https://github.com/posgraduacaofiapnet/FCG-Orchestration/blob/master/docs/FASE4.md).

Microsservico responsavel por simular envio de e-mails via logs de console.

## Endpoints

- `GET /health`

## Eventos

- Consome: `UserCreatedEvent`
- Consome: `PaymentProcessedEvent`

## Variaveis

- `RabbitMq__Host`
- `RabbitMq__Username`
- `RabbitMq__Password`
- `RabbitMq__UserCreatedQueue`
- `RabbitMq__PaymentProcessedQueue`

Este projeto já pertence ao repositório separado `FCG-NotificationsAPI`. Os eventos e variáveis
acima descrevem a implementação histórica e exigem publicadores compatíveis para execução isolada.
