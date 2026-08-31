# Scaffold de contexto

Requer o `dotnet-script`:

```powershell
dotnet tool install --global dotnet-script
```

Execute a partir da raiz do repositório:

```powershell
dotnet script .\tools\scaffold\main.csx -- --entity Evento --context Eventos
```

Por padrão, todos os artefatos são considerados. Para gerar somente alguns:

```powershell
dotnet script .\tools\scaffold\main.csx -- `
  --entity Evento `
  --context Eventos `
  --generate controller
```

Também é possível combinar artefatos:

```powershell
dotnet script .\tools\scaffold\main.csx -- `
  --entity Evento `
  --context Eventos `
  --generate entity,service,tests
```

Os aliases `domain`, `application`, `tests` e `all` estão disponíveis.

Também estão disponíveis os aliases `repository`, `requests` e `responses`.
O scaffold gera somente o contrato de repositório no Domain. A implementação
concreta deve ser criada na Infrastructure, pois depende do provider escolhido.

Para uma solution com outro namespace raiz:

```powershell
dotnet script .\tools\scaffold\main.csx -- `
  --entity Evento `
  --context Eventos `
  --rootNamespace Empresa.Projeto
```

Arquivos existentes não são sobrescritos. Use `--force` somente depois de
revisar os arquivos que serão substituídos.

O script valida os nomes de entidade, contexto e namespace. Arquivos já
existentes são informados como `Ignorado (já existe)`; portanto, executar o
comando novamente não recria testes ou outros artefatos sem `--force`.

Os artefatos disponíveis são `entity`, `command`, `filter`, `query`,
`repository-interface`, `service`, `create-request`, `list-request`,
`response`, `query-response`, `appservice`, `controller`, `entity-tests` e
`service-tests`.
