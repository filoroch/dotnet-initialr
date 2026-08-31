# Arquitetura

## Camadas

- `Domain`: entidades, comandos, filtros, queries, serviços e contratos de repositório.
- `Application`: orquestração, requests, responses e mapeamentos.
- `Infrastructure`: persistência e implementações por contexto.
- `CrossCutting`: exceções, paginação, Unit of Work e bibliotecas genéricas de persistência.
- `IoC`: composição modular, settings e bootstrap compartilhado.

## Persistência

A persistência inicial é escolhida pelo template com `--provider` e `--driver`.
EF Core com SQLite é o padrão compatível com a solution atual. Providers
adicionais podem ser incorporados posteriormente pelo scaffold com
`--add-provider`; a operação é incremental e preserva customizações existentes.

Os contratos de repositório ficam no Domain e não expõem tipos de EF Core,
NHibernate, Dapper ou MongoDB. A listagem segue o fluxo `Filter -> Query de
domínio -> ListarAsync`; cada adapter traduz a query para seu provider.

Os mappings específicos permanecem na Infrastructure. NHibernate utiliza
FluentNHibernate; não são gerados mappings XML por padrão.

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

Cada entrypoint possui seu próprio Dockerfile em `src/Apps`. A imagem publica
somente o projeto correspondente, mantendo settings, Serilog e OpenTelemetry
compostos pela IoC. O build usa a raiz da solution como contexto:

```bash
docker build -f src/Apps/Filoroch.Template.Api/Dockerfile .
```

## Geração de contextos

`tools/scaffold/main.csx` gera a estrutura inicial de um contexto a partir de
`--entity` e `--context`, incluindo artefatos de Domain, Application, API e
Tests. Ele lê os providers habilitados no `Persistence` da IoC para gerar os
repositories e mappings correspondentes. O modo `--add-provider` adiciona
dependências e settings de um provider/driver à solution sem duplicar ou
remover configurações existentes.

## Usuários

O domínio possui `Entities`, `Commands`, `Filters`, `Queries`, `Repositories` e `Services`. A API converte requests em comandos/filtros e usa a `Application` para orquestração.

## Transações

O limite transacional é o caso de uso na `Application`. Serviços de domínio e repositórios não iniciam nem confirmam transações; múltiplos serviços podem participar da mesma Unit of Work.

## Consultas

O repositório separa a construção da consulta da sua execução: `Filtrar` recebe o filtro do contexto e retorna a query; `ListarAsync` recebe essa query e aplica ordenação, paginação e materialização.
