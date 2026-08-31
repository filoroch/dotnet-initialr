# Observabilidade

Serilog é usado para logs estruturados. OpenTelemetry é usado para traces e métricas, exportados via OTLP.

A instrumentação atual cobre:

- ASP.NET Core e HttpClient;
- EF Core, incluindo spans de comandos SQL;
- SqlClient, para acessos diretos via Dapper ou NHibernate quando o provider
  utilizado for SQL Server.

As configurações ficam nos arquivos centralizados em
`src/IoC/Filoroch.Template.IoC/Settings`, nas seções `Serilog`, `OpenTelemetry`
e `Database`. Cada entrypoint carrega esses arquivos através de
`AddProjectAppSettings` antes de configurar Serilog e as dependências.

## Grafana Cloud

As variáveis padrão `OTEL_*` sobrescrevem os valores do `appsettings`:

```text
OTEL_SERVICE_NAME
OTEL_EXPORTER_OTLP_ENDPOINT
OTEL_EXPORTER_OTLP_TRACES_ENDPOINT
OTEL_EXPORTER_OTLP_METRICS_ENDPOINT
OTEL_EXPORTER_OTLP_PROTOCOL
OTEL_EXPORTER_OTLP_HEADERS
OTEL_RESOURCE_ATTRIBUTES
```

Com `http/protobuf`, a IoC acrescenta automaticamente `/v1/traces` e
`/v1/metrics` ao endpoint base. O header de autorização deve ser fornecido
somente por variável de ambiente ou secret do ambiente, nunca por arquivo
versionado.

Os spans de banco podem exibir o texto SQL. Parâmetros não são enviados por
padrão, pois podem conter dados sensíveis. A instrumentação EF Core é beta e
deve ser revisada quando o template adotar novos providers.
