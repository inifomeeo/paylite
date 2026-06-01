# Paylite

ASP.NET Core payment processing API with payment lifecycle management, idempotency, webhook verification, API key security, and Docker Compose support.

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

```bash
docker compose up --build
```

Swagger UI is available at `http://localhost:8080/swagger-ui/index.html`.