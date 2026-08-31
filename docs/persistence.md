# Persistência

## Configuração

A configuração fica centralizada na IoC, em `PersistenceSettings`:

```json
{
  "Persistence": {
    "DefaultProvider": "efcore",
    "SelectedProviders": "efcore|dapper",
    "SelectedDrivers": "sqlite|postgresql",
    "Providers": {
      "EfCore": {
        "Enabled": true,
        "Driver": "sqlite",
        "ConnectionString": "Data Source=app.db"
      },
      "NHibernate": {
        "Enabled": false,
        "Driver": "sqlite",
        "Dialect": "SQLite",
        "ConnectionString": "__SET_VIA_DOTNET_USER_SECRETS__"
      },
      "Dapper": {
        "Enabled": false,
        "Driver": "sqlite",
        "ConnectionString": "__SET_VIA_DOTNET_USER_SECRETS__"
      },
      "Mongo": {
        "Enabled": false,
        "Driver": "mongodb",
        "ConnectionString": "__SET_VIA_DOTNET_USER_SECRETS__",
        "Database": "Template"
      }
    }
  }
}
```

Connection strings e credenciais devem ser configuradas com
`dotnet user-secrets`.

## Providers

- EF Core usa o provider de banco configurado, com SQLite como padrão.
- NHibernate usa NHibernate + FluentNHibernate e o driver/dialect compatível.
- Dapper usa Dapper e um driver ADO.NET compatível.
- MongoDB usa `MongoDB.Driver`.

As implementações concretas ficam na Infrastructure. O Domain contém apenas
os contratos comuns (`IRepository`/`IQueryRepository`) e os contratos do
contexto, como `IUsuarioEfRepository` e `IUsuarioNHibernateRepository`.
Cada provider possui seu próprio Unit of Work; o contrato `IUnitOfWork` é
resolvido para o provider definido em `Persistence:DefaultProvider`.

Em múltiplas seleções, os itens de `SelectedProviders` e `SelectedDrivers`
são pareados pela mesma posição. Assim, `efcore|dapper` com
`sqlite|postgresql` significa EF Core/SQLite e Dapper/PostgreSQL.

No NHibernate, os mappings são exclusivamente FluentNHibernate. No Dapper,
a consulta é SQL parametrizada; no MongoDB, a tradução usa filtros e
projeções do driver. Nenhum contrato comum expõe `IQueryable`.

## Scaffold

Para adicionar uma persistência a uma solution existente:

```powershell
dotnet script .\tools\scaffold\main.csx -- `
  --add-provider nhibernate `
  --driver postgresql
```

Depois, a geração de um contexto lê os providers habilitados e cria os
repositories e mappings correspondentes. Arquivos existentes são preservados,
referências não são duplicadas e mappings customizados não são substituídos.

O modo `--add-provider` é incremental: ele atualiza o manifesto de
persistência, as referências, o runtime necessário, os registros de IoC e os
artefatos dos contextos existentes. Não remove provider ou código existente.
Na criação inicial, múltiplas combinações podem ser informadas repetindo
`--provider` e `--driver`; os módulos de IoC são separados por provider e
podem coexistir na mesma solution.
