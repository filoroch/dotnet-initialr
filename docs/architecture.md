# Arquitetura

## Camadas

- `Domain`: entidades, comandos, filtros, queries, serviços e contratos de repositório.
- `Application`: orquestração, requests, responses e mapeamentos.
- `Infrastructure`: persistência e implementações por contexto.
- `CrossCutting`: exceções, paginação, Unit of Work e bibliotecas genéricas de persistência.
- `IoC`: composição modular, settings e bootstrap compartilhado.

## Configuração

Os arquivos `appsettings.json` e `appsettings.{Environment}.json` ficam em
`src/IoC/Filoroch.Template.IoC/Settings`. Os entrypoints não mantêm cópias
locais. Apps que utilizam configuração devem chamar, no início do bootstrap,
`builder.Configuration.AddProjectAppSettings(builder.Environment)`. API e
Workers já utilizam essa convenção; Jobs, Consumers e MCP ainda são
placeholders mínimos.

As classes tipadas de configuração, como `DatabaseSettings` e
`OpenTelemetrySettings`, também pertencem à IoC e são registradas pelas
configurações modulares correspondentes. Sobrescritas específicas de ambiente
devem ser feitas no arquivo de ambiente centralizado, por variáveis de ambiente
ou por argumentos de linha de comando.

As URLs dos entrypoints HTTP ficam em `Entrypoints.Api.Urls`; o bootstrap da
API aplica essas URLs antes da inicialização do host. O `launchSettings.json`
continua útil para perfis locais, mas não é a fonte principal da configuração.
- `Apps`: API, Workers, Jobs, Consumers e MCP.

## Usuários

O domínio possui `Entities`, `Commands`, `Filters`, `Queries`, `Repositories` e `Services`. A API converte requests em comandos/filtros e usa a `Application` para orquestração.

## Transações

O limite transacional é o caso de uso na `Application`. Serviços de domínio e repositórios não iniciam nem confirmam transações; múltiplos serviços podem participar da mesma Unit of Work.

## Consultas

O repositório separa a construção da consulta da sua execução: `Filtrar` recebe o filtro do contexto e retorna a query; `ListarAsync` recebe essa query e aplica ordenação, paginação e materialização.
