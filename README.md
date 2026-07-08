# RabbitMqPlayground

Projeto de exemplo em .NET 10 com comunicação baseada em RabbitMQ.

> ⚠️ Este projeto foi criado para estudo e demonstração. Não deve ser usado em produção sem ajustes, revisão de segurança e práticas adequadas.

## Visão geral

O repositório contém dois serviços:

- `Producer.Api`: API responsável por publicar mensagens de pedidos no RabbitMQ.
- `Consumer.Api`: serviço de background que consome mensagens do RabbitMQ e processa pedidos.

Também há uma biblioteca compartilhada `Shared` com tipos de mensagem e constantes de configuração.

## Arquitetura

- `Producer.Api` expõe um endpoint HTTP `POST /orders` que cria um `OrderCreatedMessage` e publica no exchange de pedidos.
- `Consumer.Api` executa um `BackgroundService` (`OrderConsumerService`) que consome mensagens da fila `OrdersQueue`.
- A fila está configurada com DLQ (`OrdersDeadLetterQueue`) e um exchange de dead-letter para mensagens rejeitadas.
- A configuração de RabbitMQ é carregada via `RabbitMqOptions` em `appsettings.json`.

## Requisitos

- .NET 10 SDK
- Docker (para rodar RabbitMQ via `docker-compose`)

## Como executar

1. Inicie o RabbitMQ com Docker Compose:

```bash
docker compose up -d
```

2. Execute as APIs:

```bash
dotnet run --project src/Producer.Api
```

```bash
dotnet run --project src/Consumer.Api
```

3. Acesse a interface de gerenciamento do RabbitMQ em:

- `http://localhost:15672`

Credenciais padrão:

- Usuário: `guest`
- Senha: `guest`

## Endpoints

### Producer.Api

`POST /orders`

Request body JSON:

```json
{
  "customer": "Cliente Exemplo",
  "total": 123.45
}
```

Resposta:

- `202 Accepted` com o payload do pedido criado.

## Configuração RabbitMQ

A configuração padrão está em `src/Producer.Api/appsettings.json` e `src/Consumer.Api/appsettings.json`:

```json
"RabbitMq": {
  "Host": "localhost",
  "Port": 5672,
  "User": "guest",
  "Password": "guest"
}
```

## Componentes principais

- `Producer.Api/Messaging/Services/OrderPublisher.cs`: publica mensagens no exchange `OrdersExchange`.
- `Producer.Api/Endpoints/OrderEndpoints.cs`: expõe o endpoint de criação de pedidos.
- `Consumer.Api/BackgroundServices/OrderConsumerService.cs`: consome mensagens e realiza o processamento.
- `Shared/Messages/OrderCreatedMessage.cs`: modelo da mensagem compartilhada.
- `Shared/Messages/RabbitMqConstants.cs`: constantes de exchange, fila e routing key.

## Observações

- O projeto usa `IHostedService` no `Consumer.Api` para manter a escuta de mensagens.
- O `Producer.Api` cria exchange e filas de forma idempotente ao publicar a mensagem.

## Teste rápido

1. Envie um pedido ao produtor:

```bash
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"customer":"João","total":199.99}'
```

2. Verifique os logs do `Consumer.Api` para ver o processamento do pedido.

## Docker Compose

O arquivo `docker-compose.yml` inicia um container RabbitMQ com:

- `5672`: porta AMQP
- `15672`: painel de gerenciamento

```yaml
services:
  rabbitmq:
    image: rabbitmq:4-management
    ports:
      - "5672:5672"
      - "15672:15672"
```