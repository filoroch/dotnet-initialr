# Filoroch .NET Template

Template de solution .NET baseado em DDD, organização por contexto e composição modular de dependências.

## Estado atual

O primeiro incremento usa SQLite com EF Core e contém o contexto de `Usuarios`, API, observabilidade, tratamento global de exceções e testes de domínio.

OpenTelemetry também instrumenta EF Core e SqlClient. As variáveis padrão
`OTEL_*` podem ser usadas para enviar traces e métricas ao Grafana Cloud; não
coloque headers de autorização em arquivos versionados.

## Executar

```bash
dotnet run --project src/Apps/Filoroch.Template.Api
```

Exemplo:

```bash
curl -X POST http://localhost:5000/api/usuarios \
  -H "Content-Type: application/json" \
  -d '{"username":"Filipe Rocha","email":"filipe@email.com","senha":"Senha123!"}'
```

## Gerar uma solution a partir do template

```bash
dotnet new install .
dotnet new filoroch-scaffold-solution --name Empresa.Projeto --output ./Empresa.Projeto
```

O valor de `--name` substitui `Filoroch.Template` nos nomes de arquivos, projetos e namespaces.

A persistência inicial pode ser informada na criação da solution:

```powershell
dotnet new filoroch-scaffold-solution --name Empresa.Projeto `
  --provider efcore `
  --driver sqlite
```

O padrão é `efcore` com `sqlite`. Os providers disponíveis são `efcore`,
`nhibernate`, `dapper` e `mongo`; o driver deve ser compatível com o provider.
Para adicionar um provider posteriormente:

```powershell
dotnet script .\tools\scaffold\main.csx -- `
  --add-provider nhibernate `
  --driver postgresql
```

## Gerar um novo contexto

O scaffold inicial fica em `tools/scaffold` e pode ser executado com:

```powershell
dotnet script .\tools\scaffold\main.csx -- --entity Evento --context Eventos
```

Use `--rootNamespace` para solutions que não usam `Filoroch.Template` e `--force`
para permitir sobrescrita de arquivos existentes. Os arquivos gerados contêm
`TODO`s para as decisões específicas do contexto.

Para gerar somente artefatos específicos, use `--generate`, por exemplo:

```powershell
dotnet script .\tools\scaffold\main.csx -- `
  --entity Evento `
  --context Eventos `
  --generate controller,tests
```

Arquivos existentes são preservados por padrão. O contrato de repositório é
gerado no Domain; sua implementação concreta deve ser criada na Infrastructure
conforme os providers habilitados. O fluxo de consulta usa `Filter -> Query de
domínio -> ListarAsync`, sem expor `IQueryable` no contrato comum.

O template já contém os adaptadores runtime dos quatro providers, módulos de
IoC separados e settings tipados. A criação inicial aceita múltiplos providers;
o `--add-provider` complementa uma solution existente sem sobrescrever
repositories ou mappings já presentes.

As combinações selecionadas ficam registradas em `Persistence.SelectedProviders`
e `Persistence.SelectedDrivers`, mantendo o pareamento pela posição.

## Docker

Cada entrypoint possui um Dockerfile próprio. Os comandos devem ser executados
na raiz da solution:

```bash
docker build -f src/Apps/Filoroch.Template.Api/Dockerfile -t filoroch-template-api .
docker build -f src/Apps/Filoroch.Template.Workers/Dockerfile -t filoroch-template-workers .
docker build -f src/Apps/Filoroch.Template.Jobs/Dockerfile -t filoroch-template-jobs .
docker build -f src/Apps/Filoroch.Template.Consumers/Dockerfile -t filoroch-template-consumers .
docker build -f src/Apps/Filoroch.Template.Mcp/Dockerfile -t filoroch-template-mcp .
```

## Swagger

Com a API em execução, a UI Swagger fica disponível em
`http://localhost:5196/swagger` e o documento OpenAPI em
`http://localhost:5196/swagger/v1/swagger.json`. Título, descrição, versão,
rota e URLs da API são configurados nos settings centralizados da IoC.

## MCP

O executável MCP usa transporte `stdio` e expõe as operações disponíveis do
contexto de usuários para clientes compatíveis:

- `criar_usuario`: cria um usuário aplicando as mesmas validações e regras da API.
- `listar_usuarios`: lista usuários com filtros por nome, e-mail e status, além de paginação.

Para conectar um cliente MCP local, execute `dotnet run --project
src/Apps/Filoroch.Template.Mcp`. A saída padrão é reservada ao protocolo MCP;
os logs são enviados para `stderr`.

### Configuração local do JWT

Defina a chave fora do repositório antes de iniciar a API:

```powershell
dotnet user-secrets --project src/Apps/Filoroch.Template.Api set "Jwt:SigningKey" "uma-chave-local-com-pelo-menos-32-caracteres"
```

O endpoint `POST /api/auth/login` é público. O endpoint `/health` também é
público; os demais endpoints da API exigem um Bearer token válido.

O login exige que o usuário possua `SenhaHash` BCrypt e `Perfil` persistidos.
A criação do usuário recebe uma senha obrigatória, que é transformada em hash
antes da persistência. A alteração de senha segue o mesmo fluxo.

## Próximos passos

- Validar a visualização de spans SQL no Grafana Cloud.
- Evoluir o scaffold para atualizar registros de DI e referências específicas.
