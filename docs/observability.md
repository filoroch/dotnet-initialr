# Observabilidade

Serilog é usado para logs estruturados. OpenTelemetry é usado para traces e métricas, exportados via OTLP quando um collector estiver disponível.

As configurações ficam nos arquivos centralizados em
`src/IoC/Filoroch.Template.IoC/Settings`, nas seções `Serilog`, `OpenTelemetry`
e `Database`. Cada entrypoint carrega esses arquivos através de
`AddProjectAppSettings` antes de configurar Serilog e as dependências.
