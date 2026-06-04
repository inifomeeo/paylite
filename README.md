# Paylite

ASP.NET Core payment processing API with payment lifecycle management, idempotency, webhook verification, and API key security.

Data is backed by Microsoft SQL Server via Entity Framework Core.

## Endpoints

- `POST /api/v1/payments`
- `GET /api/v1/payments/{id}`
- `POST /api/v1/webhooks/psp`
- `GET /actuator/health`
- `GET /swagger-ui/index.html`
- `GET /v3/api-docs/v1.json`

## Security

- `X-API-Key` is required for payment endpoints.
- `Idempotency-Key` is required for payment creation.
- `X-PSP-Signature` is required for PSP webhooks and is verified with HMAC-SHA256.

## Run

Create a SQL Server database locally with name set to paylitedb, then run

```bash
dotnet ef database update
```

and

```bash
dotnet run --project api.csproj
```

Swagger UI is available at `http://localhost:8080/swagger-ui/index.html`.