# Notification Worker

Serviço de notificações que consome mensagens de uma fila RabbitMQ e envia notificações multi-canal (Email, SMS e Telegram).

## Para que serve

Este worker processa mensagens de notificação de forma assíncrona, enviando alertas aos usuários através de:
- **Email** - via SMTP (MailKit)
- **SMS** - via Twilio
- **Telegram** - via Telegram Bot API

O serviço integra-se com uma API externa de usuários para obter preferências de canal de notificação.

## Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) e Docker Compose (para ambiente local)
- RabbitMQ (fornecido via Docker Compose)
- Credenciais de serviços externos:
  - Servidor SMTP para envio de emails
  - Conta Twilio para SMS
  - Bot Token do Telegram

## Como desenvolver e colaborar

### 1. Configuração inicial

Clone o repositório:
```bash
git clone <repository-url>
cd final-challenge-grupo-118-notification
```

### 2. Configurar variáveis de ambiente

Edite `src/Adapters/Driving/Notification.Worker/appsettings.Development.json` com suas credenciais:

```json
{
  "UserApi": {
    "BaseUrl": "https://api-users.url",
    "UserApiHash": "seu-hash-api"
  },
  "RabbitMqSettings": {
    "ConnectionString": "amqp://guest:guest@localhost:5672",
    "QueueName": "notification"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "seu-email@gmail.com",
    "Username": "seu-usuario",
    "Password": "sua-senha"
  },
  "SmsSettings": {
    "AccountSid": "seu-twilio-sid",
    "AuthToken": "seu-twilio-token",
    "FromPhoneNumber": "+1234567890"
  }
}
```

### 3. Executar com Docker Compose

```bash
docker-compose up -d
```

### 4. Executar localmente (desenvolvimento)

Inicie o RabbitMQ:
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Restaure e execute o projeto:
```bash
dotnet restore
dotnet run --project src/Adapters/Driving/Notification.Worker/Notification.Worker.csproj
```

### 5. Executar testes

```bash
dotnet test
```

### Estrutura do projeto

O projeto segue a arquitetura hexagonal (Ports & Adapters):

```
src/
├── Core/
│   ├── Domain/      # Entidades e portas
│   └── Application/ # Casos de uso
└── Adapters/
    ├── Driving/     # Adaptadores de entrada (Worker)
    └── External/    # Adaptadores de saída (APIs externas)
```

### Contribuindo

1. Crie uma branch para sua feature: `git checkout -b feature/minha-feature`
2. Commit suas mudanças: `git commit -m 'feat: adiciona nova feature'`
3. Push para a branch: `git push origin feature/minha-feature`
4. Abra um Pull Request

### Observabilidade

O serviço inclui suporte a OpenTelemetry para traces e métricas.

