# Filoroch .NET Template

Template de solution .NET baseado em DDD, organização por contexto e composição modular de dependências.

## Estado atual

O primeiro incremento usa SQLite com EF Core e contém o contexto de `Usuarios`, API, observabilidade, tratamento global de exceções e testes de domínio.

## Executar

```bash
dotnet run --project src/Apps/Filoroch.Template.Api
```

Exemplo:

```bash
curl -X POST http://localhost:5000/api/usuarios \
  -H "Content-Type: application/json" \
  -d '{"nome":"Filipe Rocha","email":"filipe@email.com"}'
```

## Gerar uma solution a partir do template

```bash
dotnet new install .
dotnet new filoroch-solution --name Empresa.Projeto --output ./Empresa.Projeto
```

O valor de `--name` substitui `Filoroch.Template` nos nomes de arquivos, projetos e namespaces.

## Swagger

Com a API em execução, a UI Swagger fica disponível em
`http://localhost:5196/swagger` e o documento OpenAPI em
`http://localhost:5196/swagger/v1/swagger.json`. Título, descrição, versão,
rota e URLs da API são configurados nos settings centralizados da IoC.
