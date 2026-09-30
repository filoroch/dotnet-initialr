# Harness local de observabilidade (NÃO vai para o template gerado)

Stack: `otel-collector -> loki (logs) + tempo (traces) + prometheus (métricas) -> grafana`.

## Subir

```bash
docker compose -f tools/observability/docker-compose.yml up -d
dotnet run --project src/Apps/Filoroch.Template.Api
```

Os apps exportam OTLP `grpc` para `http://localhost:4317` por padrão
(`OpenTelemetry:Endpoint` ou `OTEL_EXPORTER_OTLP_ENDPOINT`).

## Gerar tráfego

```bash
curl -X POST http://localhost:5196/api/usuarios \
  -H "Content-Type: application/json" \
  -d '{"username":"Obs Test","email":"obs@test.com","senha":"Senha123!"}'
curl "http://localhost:5196/api/usuarios?Page=1&Quantity=5"
```

## Validar

- Grafana `http://localhost:3000` (login anônimo): Explore `Tempo` (traces
  `POST /api/usuarios` com spans EF Core SQL), `Loki` (logs correlacionados
  por `traceId`), `Prometheus` (`http_server_request_duration_*`, runtime).
- Aspire (alternativa rápida, sem Grafana):

```bash
docker compose -f tools/observability/docker-compose.yml --profile aspire up -d aspire-dashboard
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:18889 dotnet run --project src/Apps/Filoroch.Template.Api
# Dashboard em http://localhost:8081
```

## Derrubar

```bash
docker compose -f tools/observability/docker-compose.yml down
```

Nunca versione `OTEL_EXPORTER_OTLP_HEADERS` nem secrets — use env/secret local.
