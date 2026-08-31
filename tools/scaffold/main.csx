#nullable enable
#r "nuget: Humanizer, 2.14.1"

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Humanizer;

var options = ParseArguments(Args);
string root = Directory.GetCurrentDirectory();

if (options.ContainsKey("help") || (!options.ContainsKey("entity") && !options.ContainsKey("add-provider")))
{
    PrintUsage();
    return;
}

if (Directory.GetFiles(root, "*.sln", SearchOption.TopDirectoryOnly).Length == 0)
    throw new InvalidOperationException("Execute o scaffold a partir da raiz da solution.");

if (options.TryGetValue("add-provider", out string? providerToAdd))
{
    AddProvider(root, providerToAdd, options.TryGetValue("driver", out string? driverToAdd) ? driverToAdd : null);
    return;
}

string entity = options["entity"];
ValidateIdentifier(entity, "entity");
string context = options.TryGetValue("context", out string? contextValue) ? contextValue : entity.Pluralize();
ValidateIdentifier(context, "context");
string rootNamespace = options.TryGetValue("rootNamespace", out string? namespaceValue) ? namespaceValue : InferRootNamespace(root);
ValidateNamespace(rootNamespace);
string templateRoot = Path.Combine(root, "tools", "scaffold", "templates");
ValidatePersistenceConfiguration(root);
var selectedArtifacts = GetSelectedArtifacts(options);
var tokens = new Dictionary<string, string>
{
    ["{{RootNamespace}}"] = rootNamespace, ["{{Entity}}"] = entity,
    ["{{entity}}"] = ToCamelCase(entity), ["{{Context}}"] = context, ["{{context}}"] = ToCamelCase(context)
};

var files = new Dictionary<string, (string Template, string Artifact)>
{
    [$"src/Domain/{rootNamespace}.Domain/{context}/Entities/{entity}.cs"] = ("Entity.cs.template", "entity"),
    [$"src/Domain/{rootNamespace}.Domain/{context}/Commands/Criar{entity}Command.cs"] = ("CreateCommand.cs.template", "command"),
    [$"src/Domain/{rootNamespace}.Domain/{context}/Filters/Listar{context}Filter.cs"] = ("ListFilter.cs.template", "filter"),
    [$"src/Domain/{rootNamespace}.Domain/{context}/Queries/{entity}Query.cs"] = ("Query.cs.template", "query"),
    [$"src/Domain/{rootNamespace}.Domain/{context}/Queries/Listar{context}Query.cs"] = ("ListQuery.cs.template", "query"),
    [$"src/Domain/{rootNamespace}.Domain/{context}/Repositories/I{entity}Repository.cs"] = ("RepositoryInterface.cs.template", "repository-interface"),
    [$"src/Domain/{rootNamespace}.Domain/{context}/Services/I{context}Service.cs"] = ("DomainServiceInterface.cs.template", "service"),
    [$"src/Domain/{rootNamespace}.Domain/{context}/Services/{context}Service.cs"] = ("DomainService.cs.template", "service"),
    [$"src/Application/{rootNamespace}.Application/{context}/DataTransfer/Requests/Criar{entity}Request.cs"] = ("CreateRequest.cs.template", "create-request"),
    [$"src/Application/{rootNamespace}.Application/{context}/DataTransfer/Requests/Listar{context}Request.cs"] = ("ListRequest.cs.template", "list-request"),
    [$"src/Application/{rootNamespace}.Application/{context}/DataTransfer/Responses/{entity}Response.cs"] = ("Response.cs.template", "response"),
    [$"src/Application/{rootNamespace}.Application/{context}/DataTransfer/Responses/{entity}QueryResponse.cs"] = ("QueryResponse.cs.template", "query-response"),
    [$"src/Application/{rootNamespace}.Application/{context}/Services/I{entity}AppService.cs"] = ("ApplicationServiceInterface.cs.template", "appservice"),
    [$"src/Application/{rootNamespace}.Application/{context}/Services/{entity}AppService.cs"] = ("ApplicationService.cs.template", "appservice"),
    [$"src/Apps/{rootNamespace}.Api/Controllers/{context}/{context}Controller.cs"] = ("Controller.cs.template", "controller"),
    [$"src/Tests/{rootNamespace}.Domain.Tests/{context}/Entities/{entity}Tests.cs"] = ("EntityTests.cs.template", "entity-tests"),
    [$"src/Tests/{rootNamespace}.Domain.Tests/{context}/Services/{context}ServiceTests.cs"] = ("ServiceTests.cs.template", "service-tests")
};

foreach (string provider in ReadEnabledProviders(root))
{
    string suffix = ProviderSuffix(provider);
    files[$"src/Domain/{rootNamespace}.Domain/{context}/Repositories/I{entity}{suffix}Repository.cs"] = ($"{provider}RepositoryInterface.cs.template", "repository-provider");
    files[$"src/Infrastructure/{rootNamespace}.Infra/{context}/Repositories/{entity}{suffix}Repository.cs"] = ($"{provider}Repository.cs.template", "repository-provider");
    files[$"src/Infrastructure/{rootNamespace}.Infra/{context}/Mappings/{entity}{suffix}Mapping.cs"] = ($"{provider}Mapping.cs.template", "mapping-provider");
}

foreach ((string destination, (string template, string artifact)) in files)
{
    if (!selectedArtifacts.Contains(artifact) && artifact is not "repository-provider" and not "mapping-provider") continue;
    string destinationPath = Path.Combine(root, destination.Replace('/', Path.DirectorySeparatorChar));
    if (File.Exists(destinationPath) && !options.ContainsKey("force")) { Console.WriteLine($"Ignorado (já existe): {destination}"); continue; }
    string templatePath = Path.Combine(templateRoot, template);
    if (!File.Exists(templatePath)) { Console.WriteLine($"Ignorado (template ainda não disponível): {template}"); continue; }
    string content = File.ReadAllText(templatePath);
    foreach ((string token, string value) in tokens) content = content.Replace(token, value, StringComparison.Ordinal);
    if (tokens.TryGetValue("{{Entity}}", out string? entity) &&
        entity.Equals("Usuario", StringComparison.OrdinalIgnoreCase) &&
        template.EndsWith("Repository.cs.template", StringComparison.OrdinalIgnoreCase) &&
        !template.Equals("RepositoryInterface.cs.template", StringComparison.OrdinalIgnoreCase))
    {
        content = content.Replace("Nome", "Username", StringComparison.Ordinal);
        content = AddUsuarioAuthenticationMembers(content, template);
    }
    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
    File.WriteAllText(destinationPath, content);
    Console.WriteLine($"Gerado: {destination}");
}

static string AddUsuarioAuthenticationMembers(string content, string template)
{
    string members = template.StartsWith("nhibernate", StringComparison.OrdinalIgnoreCase) ? """
    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => session.Query<Usuario>().AnyAsync(entity => entity.Email == email.Trim().ToLower(), cancellationToken);

    public Task<Usuario?> BuscarPorEmailParaAutenticacaoAsync(string email, CancellationToken cancellationToken = default)
        => session.Query<Usuario>().FirstOrDefaultAsync(entity => entity.Email == email.Trim().ToLower(), cancellationToken);
    """ : template.StartsWith("dapper", StringComparison.OrdinalIgnoreCase) ? """
    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => connection.ExecuteScalarAsync<bool>(new CommandDefinition("SELECT CASE WHEN EXISTS (SELECT 1 FROM Usuarios WHERE Email = @email) THEN 1 ELSE 0 END", new { email }, cancellationToken: cancellationToken));

    public Task<Usuario?> BuscarPorEmailParaAutenticacaoAsync(string email, CancellationToken cancellationToken = default)
        => connection.QuerySingleOrDefaultAsync<Usuario>(new CommandDefinition("SELECT * FROM Usuarios WHERE Email = @email", new { email }, cancellationToken: cancellationToken));
    """ : """
    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => collection.Find(entity => entity.Email == email.Trim().ToLower()).AnyAsync();

    public Task<Usuario?> BuscarPorEmailParaAutenticacaoAsync(string email, CancellationToken cancellationToken = default)
        => collection.Find(entity => entity.Email == email.Trim().ToLower()).FirstOrDefaultAsync();
    """;

    int lastBrace = content.LastIndexOf('}');
    return lastBrace < 0 ? content : content.Insert(lastBrace, members + Environment.NewLine);
}

UpdateContextRegistrations(root, rootNamespace, context, entity);

Console.WriteLine();
Console.WriteLine("Geração concluída. Revise os TODOs e execute dotnet format/build.");

static void AddProvider(string root, string providerValue, string? driverValue)
{
    string provider = providerValue.Trim().ToLowerInvariant();
    string driver = (driverValue ?? DefaultDriver(provider)).Trim().ToLowerInvariant();
    _ = PackagesFor(provider, driver);
    string? infrastructureProject = Directory.GetFiles(Path.Combine(root, "src", "Infrastructure"), "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
    string? iocProject = Directory.GetFiles(Path.Combine(root, "src", "IoC"), "*.IoC.csproj", SearchOption.AllDirectories).FirstOrDefault();
    string? settingsPath = Directory.GetFiles(Path.Combine(root, "src", "IoC"), "appsettings.json", SearchOption.AllDirectories).FirstOrDefault();
    if (infrastructureProject is null || iocProject is null || settingsPath is null) throw new InvalidOperationException("Não foi possível localizar Infrastructure, IoC ou o appsettings da IoC.");
    foreach (string package in PackagesFor(provider, driver))
    {
        AddPackageReference(infrastructureProject, package);
        AddPackageReference(iocProject, package);
    }
    JsonObject document = JsonNode.Parse(File.ReadAllText(settingsPath))?.AsObject() ?? new JsonObject();
    JsonObject persistence = document["Persistence"] as JsonObject ?? new JsonObject();
    document["Persistence"] = persistence;
    string? currentDefault = persistence["DefaultProvider"]?.GetValue<string>();
    if (string.IsNullOrWhiteSpace(currentDefault) || currentDefault.StartsWith("__TEMPLATE_", StringComparison.Ordinal))
        persistence["DefaultProvider"] = provider;
    JsonObject providers = persistence["Providers"] as JsonObject ?? new JsonObject();
    persistence["Providers"] = providers;
    AppendSelection(persistence, "SelectedProviders", provider);
    AppendSelection(persistence, "SelectedDrivers", driver);
    string key = ProviderSettingsKey(provider);
    JsonObject settings = providers[key] as JsonObject ?? new JsonObject();
    providers[key] = settings;
    settings["Enabled"] = true; settings["Driver"] = DriverName(driver);
    settings["ConnectionString"] ??= "__SET_VIA_DOTNET_USER_SECRETS__";
    if (provider is "nhibernate") settings["Dialect"] ??= DialectName(driver);
    if (provider is "mongo") settings["Database"] ??= "Template";
    UpdateTemplateSelectionProperties(infrastructureProject, persistence);
    UpdateTemplateSelectionProperties(iocProject, persistence);
    File.WriteAllText(settingsPath, document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
    ValidatePersistenceConfiguration(root);
    CopyProviderRuntime(root, provider);
    GenerateProviderArtifacts(root, provider);
    UpdateInfrastructureConfiguration(root, provider);
    UpdateAllContextRegistrations(root);
    Console.WriteLine($"Provider adicionado: {provider} ({driver})");
    Console.WriteLine("Pacotes, appsettings e artefatos dos contextos existentes foram atualizados.");
}

static void AppendSelection(JsonObject persistence, string property, string value)
{
    string current = persistence[property]?.GetValue<string>()?.Trim() ?? string.Empty;
    string[] values = current.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (!values.Contains(value, StringComparer.OrdinalIgnoreCase))
        persistence[property] = string.IsNullOrEmpty(current) ? value : $"{current}|{value}";
}

static void UpdateTemplateSelectionProperties(string projectPath, JsonObject persistence)
{
    string providers = persistence["SelectedProviders"]?.GetValue<string>()?.Trim() ?? string.Empty;
    string drivers = persistence["SelectedDrivers"]?.GetValue<string>()?.Trim() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(providers) || string.IsNullOrWhiteSpace(drivers)) return;

    XDocument document = XDocument.Load(projectPath, LoadOptions.PreserveWhitespace);
    XElement project = document.Root ?? throw new InvalidOperationException($"Projeto inválido: {projectPath}");
    XNamespace ns = project.Name.Namespace;
    XElement? propertyGroup = project.Elements(ns + "PropertyGroup")
        .FirstOrDefault(group => group.Elements(ns + "TemplateSelectedProvidersOverride").Any());
    if (propertyGroup is null) return;

    XElement? providerOverride = propertyGroup.Element(ns + "TemplateSelectedProvidersOverride");
    XElement? driverOverride = propertyGroup.Element(ns + "TemplateSelectedDriversOverride");
    if (providerOverride is null || driverOverride is null) return;

    providerOverride.Value = providers;
    driverOverride.Value = drivers;
    document.Save(projectPath);
}

static void UpdateInfrastructureConfiguration(string root, string provider)
{
    string? configurationPath = Directory.GetFiles(Path.Combine(root, "src", "IoC"), "InfrastructureConfiguration.cs", SearchOption.AllDirectories).FirstOrDefault();
    string? domainProject = Directory.GetFiles(Path.Combine(root, "src", "Domain"), "*.Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
    if (configurationPath is null || domainProject is null) return;

    string source = File.ReadAllText(configurationPath);
    string marker = $"// scaffold:provider:{provider}";
    if (source.Contains(marker, StringComparison.OrdinalIgnoreCase)) return;

    string rootNamespace = Path.GetFileNameWithoutExtension(domainProject)[..^(".Domain".Length)];
    string[] usings = provider switch
    {
        "nhibernate" =>
        [
            "using FluentNHibernate.Cfg;",
            "using FluentNHibernate.Cfg.Db;",
            "using NHibernate;",
            $"using {rootNamespace}.CrossCutting.Persistence.UnitOfWork.Interfaces;",
            $"using {rootNamespace}.Infra.Persistence;",
            $"using {rootNamespace}.Domain.Usuarios.Repositories;",
            $"using {rootNamespace}.Infra.Usuarios.Mappings;",
            $"using {rootNamespace}.Infra.Usuarios.Repositories;"
        ],
        "dapper" =>
        [
            $"using {rootNamespace}.CrossCutting.Persistence.UnitOfWork.Interfaces;",
            $"using {rootNamespace}.Domain.Usuarios.Repositories;",
            $"using {rootNamespace}.Infra.Persistence;",
            $"using {rootNamespace}.Infra.Usuarios.Repositories;"
        ],
        "mongo" =>
        [
            "using MongoDB.Driver;",
            $"using {rootNamespace}.CrossCutting.Persistence.UnitOfWork.Interfaces;",
            $"using {rootNamespace}.Domain.Usuarios.Repositories;",
            $"using {rootNamespace}.Infra.Usuarios.Repositories;"
        ],
        _ => []
    };
    if (usings.Length == 0) return;

    string body = provider switch
    {
        "nhibernate" => """
        // scaffold:provider:nhibernate
        services.AddSingleton<ISessionFactory>(_ =>
        {
            FluentConfiguration configuration = Fluently.Configure();
            configuration = persistence.Providers.NHibernate.Driver.ToLowerInvariant() switch
            {
                "sqlite" => configuration.Database(SQLiteConfiguration.Standard.ConnectionString(persistence.Providers.NHibernate.ConnectionString)),
                "sqlserver" => configuration.Database(MsSqlConfiguration.MsSql2012.ConnectionString(persistence.Providers.NHibernate.ConnectionString)),
                "postgresql" => configuration.Database(PostgreSQLConfiguration.Standard.ConnectionString(persistence.Providers.NHibernate.ConnectionString)),
                _ => throw new InvalidOperationException($"Driver NHibernate não suportado: {persistence.Providers.NHibernate.Driver}.")
            };
            return configuration.Mappings(mapping => mapping.FluentMappings.AddFromAssemblyOf<UsuarioNHibernateMapping>()).BuildSessionFactory();
        });
        services.AddScoped(provider => provider.GetRequiredService<ISessionFactory>().OpenSession());
        services.AddScoped<IUsuarioNHibernateRepository, UsuarioNHibernateRepository>();
        services.AddScoped<NHibernateUnitOfWork>();
        services.AddScoped<INHibernateUnitOfWork>(provider => provider.GetRequiredService<NHibernateUnitOfWork>());
        """,
        "dapper" => """
        // scaffold:provider:dapper
        services.AddScoped<DapperConnection>(_ => new DapperConnection(DbConnectionFactory.Create(persistence.Providers.Dapper.Driver, persistence.Providers.Dapper.ConnectionString)));
        services.AddScoped<IUsuarioDapperRepository, UsuarioDapperRepository>();
        services.AddScoped<DapperUnitOfWork>();
        services.AddScoped<IDapperUnitOfWork>(provider => provider.GetRequiredService<DapperUnitOfWork>());
        """,
        "mongo" => """
        // scaffold:provider:mongo
        services.AddSingleton<IMongoClient>(_ => new MongoClient(persistence.Providers.Mongo.ConnectionString));
        services.AddScoped(provider => provider.GetRequiredService<IMongoClient>().StartSession());
        services.AddScoped(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(persistence.Providers.Mongo.Database).GetCollection<__ROOT__.Domain.Usuarios.Entities.Usuario>("Usuarios"));
        services.AddScoped<IUsuarioMongoRepository, UsuarioMongoRepository>();
        services.AddScoped<MongoUnitOfWork>();
        services.AddScoped<IMongoUnitOfWork>(provider => provider.GetRequiredService<MongoUnitOfWork>());
        """,
        _ => string.Empty
    };
    body = body.Replace("__ROOT__", rootNamespace, StringComparison.Ordinal);

    int returnIndex = source.LastIndexOf("        return services;", StringComparison.Ordinal);
    if (returnIndex < 0) throw new InvalidOperationException("InfrastructureConfiguration.cs não possui o ponto de extensão esperado.");
    source = source.Insert(returnIndex, body + Environment.NewLine);
    source = AddMissingUsings(source, usings);
    File.WriteAllText(configurationPath, source);
    Console.WriteLine($"Atualizado: {Path.GetRelativePath(root, configurationPath)}");
}

static void UpdateContextRegistrations(string root, string rootNamespace, string context, string entity)
{
    string? configurationPath = Directory.GetFiles(Path.Combine(root, "src", "IoC"), "InfrastructureConfiguration.cs", SearchOption.AllDirectories).FirstOrDefault();
    if (configurationPath is null) return;

    string source = File.ReadAllText(configurationPath);
    string marker = $"// scaffold:context:{context}:{entity}";
    if (source.Contains(marker, StringComparison.OrdinalIgnoreCase)) return;

    var enabledProviders = ReadEnabledProviders(root);
    var registrations = new List<(string Provider, string Suffix)>();
    var usings = new List<string>();
    foreach (string provider in enabledProviders)
    {
        string suffix = ProviderSuffix(provider);
        string interfacePath = Path.Combine(root, $"src/Domain/{rootNamespace}.Domain/{context}/Repositories/I{entity}{suffix}Repository.cs");
        string implementationPath = Path.Combine(root, $"src/Infrastructure/{rootNamespace}.Infra/{context}/Repositories/{entity}{suffix}Repository.cs");
        if (!File.Exists(interfacePath) || !File.Exists(implementationPath)) continue;
        registrations.Add((provider, suffix));
        usings.Add($"using {rootNamespace}.Domain.{context}.Repositories;");
        usings.Add($"using {rootNamespace}.Infra.{context}.Repositories;");
    }
    if (registrations.Count == 0) return;

    string providerRegistrations = string.Join(Environment.NewLine, registrations.Select(item =>
        $"        services.AddScoped<I{entity}{item.Suffix}Repository, {entity}{item.Suffix}Repository>();"));
    string defaultRegistration = string.Empty;
    string commonInterfacePath = Path.Combine(root, $"src/Domain/{rootNamespace}.Domain/{context}/Repositories/I{entity}Repository.cs");
    if (File.Exists(commonInterfacePath))
    {
        string cases = string.Join(Environment.NewLine, registrations.Select(item =>
            $"            PersistenceProviders.{ProviderConstant(item.Provider)} => provider.GetRequiredService<I{entity}{item.Suffix}Repository>(),"));
        defaultRegistration = $"        services.AddScoped<I{entity}Repository>(provider => persistence.DefaultProvider.ToLowerInvariant() switch\n        {{\n{cases}\n            _ => throw new InvalidOperationException($\"Provider padrão inválido: {{persistence.DefaultProvider}}.\")\n        }});\n";
    }

    int returnIndex = source.LastIndexOf("        return services;", StringComparison.Ordinal);
    if (returnIndex < 0) throw new InvalidOperationException("InfrastructureConfiguration.cs não possui o ponto de extensão esperado.");
    source = source.Insert(returnIndex, $"        {marker}{Environment.NewLine}{providerRegistrations}{Environment.NewLine}{defaultRegistration}");
    source = AddMissingUsings(source, usings);
    File.WriteAllText(configurationPath, source);
    Console.WriteLine($"Atualizado: {Path.GetRelativePath(root, configurationPath)}");
}

static string AddMissingUsings(string source, IEnumerable<string> usings)
{
    string[] existingUsings = source.Split(["\r\n", "\n"], StringSplitOptions.None)
        .Select(line => line.Trim())
        .Where(line => line.StartsWith("using ", StringComparison.Ordinal))
        .ToArray();
    string[] missingUsings = usings.Distinct(StringComparer.Ordinal)
        .Where(usingDirective => !existingUsings.Contains(usingDirective, StringComparer.Ordinal))
        .ToArray();
    return missingUsings.Length == 0
        ? source
        : string.Join(Environment.NewLine, missingUsings) + Environment.NewLine + source;
}

static void UpdateAllContextRegistrations(string root)
{
    string? domainProject = Directory.GetFiles(Path.Combine(root, "src", "Domain"), "*.Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
    if (domainProject is null) return;
    string domainRoot = Path.GetDirectoryName(domainProject)!;
    string rootNamespace = Path.GetFileNameWithoutExtension(domainProject)[..^(".Domain".Length)];
    foreach (string entityFile in Directory.GetFiles(domainRoot, "*.cs", SearchOption.AllDirectories)
        .Where(path => string.Equals(new DirectoryInfo(Path.GetDirectoryName(path)!).Name, "Entities", StringComparison.OrdinalIgnoreCase)))
    {
        string entity = Path.GetFileNameWithoutExtension(entityFile);
        string context = new DirectoryInfo(Path.GetDirectoryName(Path.GetDirectoryName(entityFile)!)!).Name;
        UpdateContextRegistrations(root, rootNamespace, context, entity);
    }
}

static void CopyProviderRuntime(string root, string provider)
{
    string? domainProject = Directory.GetFiles(Path.Combine(root, "src", "Domain"), "*.Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
    if (domainProject is null) return;
    string rootNamespace = Path.GetFileNameWithoutExtension(domainProject)[..^(".Domain".Length)];
    string templateRoot = Path.Combine(root, "tools", "scaffold", "templates", "runtime");
    string[] templates = provider switch
    {
        "nhibernate" => ["NHibernateUnitOfWork.cs.template"],
        "dapper" => ["DbConnectionFactory.cs.template", "DapperConnection.cs.template", "DapperUnitOfWork.cs.template"],
        "mongo" => ["MongoUnitOfWork.cs.template"],
        _ => []
    };

    var tokens = new Dictionary<string, string> { ["{{RootNamespace}}"] = rootNamespace };
    foreach (string template in templates)
        WriteGeneratedFile(root, templateRoot, $"src/Infrastructure/{rootNamespace}.Infra/Persistence/{template[..^9]}", template, "runtime", tokens);
}

static void GenerateProviderArtifacts(string root, string provider)
{
    string? domainProject = Directory.GetFiles(Path.Combine(root, "src", "Domain"), "*.Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
    if (domainProject is null) return;

    string domainRoot = Path.GetDirectoryName(domainProject)!;
    string rootNamespace = Path.GetFileNameWithoutExtension(domainProject)[..^".Domain".Length];
    string templateRoot = Path.Combine(root, "tools", "scaffold", "templates");
    string suffix = ProviderSuffix(provider);

    foreach (string entityFile in Directory.GetFiles(domainRoot, "*.cs", SearchOption.AllDirectories)
        .Where(path => string.Equals(new DirectoryInfo(Path.GetDirectoryName(path)!).Name, "Entities", StringComparison.OrdinalIgnoreCase)))
    {
        string entity = Path.GetFileNameWithoutExtension(entityFile);
        string context = new DirectoryInfo(Path.GetDirectoryName(Path.GetDirectoryName(entityFile)!)!).Name;
        ValidateIdentifier(entity, "entity");
        ValidateIdentifier(context, "context");

        var tokens = new Dictionary<string, string>
        {
            ["{{RootNamespace}}"] = rootNamespace,
            ["{{Entity}}"] = entity,
            ["{{entity}}"] = ToCamelCase(entity),
            ["{{Context}}"] = context,
            ["{{context}}"] = ToCamelCase(context)
        };

        WriteGeneratedFile(root, templateRoot, $"src/Domain/{rootNamespace}.Domain/{context}/Repositories/I{entity}{suffix}Repository.cs", $"{provider}RepositoryInterface.cs.template", "repository-provider", tokens);
        WriteGeneratedFile(root, templateRoot, $"src/Infrastructure/{rootNamespace}.Infra/{context}/Repositories/{entity}{suffix}Repository.cs", $"{provider}Repository.cs.template", "repository-provider", tokens);
        WriteGeneratedFile(root, templateRoot, $"src/Infrastructure/{rootNamespace}.Infra/{context}/Mappings/{entity}{suffix}Mapping.cs", $"{provider}Mapping.cs.template", "mapping-provider", tokens);
    }
}

static void WriteGeneratedFile(string root, string templateRoot, string destination, string template, string artifact, Dictionary<string, string> tokens)
{
    string destinationPath = Path.Combine(root, destination.Replace('/', Path.DirectorySeparatorChar));
    if (File.Exists(destinationPath))
    {
        Console.WriteLine($"Ignorado (já existe): {destination}");
        return;
    }

    string templatePath = Path.Combine(templateRoot, template);
    if (!File.Exists(templatePath))
    {
        Console.WriteLine($"Ignorado (template ainda não disponível): {template}");
        return;
    }

    string content = File.ReadAllText(templatePath);
    foreach ((string token, string value) in tokens) content = content.Replace(token, value, StringComparison.Ordinal);
    if (tokens.TryGetValue("{{Entity}}", out string? entity) &&
        entity.Equals("Usuario", StringComparison.OrdinalIgnoreCase) &&
        template.EndsWith("Repository.cs.template", StringComparison.OrdinalIgnoreCase) &&
        !template.Equals("RepositoryInterface.cs.template", StringComparison.OrdinalIgnoreCase))
    {
        content = content.Replace("Nome", "Username", StringComparison.Ordinal);
        content = AddUsuarioAuthenticationMembers(content, template);
    }
    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
    File.WriteAllText(destinationPath, content);
    Console.WriteLine($"Gerado: {destination}");
}

static void AddPackageReference(string projectPath, string package)
{
    XDocument document = XDocument.Load(projectPath, LoadOptions.PreserveWhitespace);
    XElement project = document.Root ?? throw new InvalidOperationException($"Projeto inválido: {projectPath}");
    XNamespace ns = project.Name.Namespace;
    XName itemGroupName = ns + "ItemGroup";
    XName packageReferenceName = ns + "PackageReference";
    XElement itemGroup = project.Elements(itemGroupName).FirstOrDefault(group => group.Elements(packageReferenceName).Any()) ?? new XElement(itemGroupName);
    if (itemGroup.Parent is null) project.Add(itemGroup);
    string[] parts = package.Split(':', 2);
    if (project.Descendants(packageReferenceName).Any(reference => string.Equals((string?)reference.Attribute("Include"), parts[0], StringComparison.OrdinalIgnoreCase))) return;
    itemGroup.Add(new XElement(packageReferenceName, new XAttribute("Include", parts[0]), new XAttribute("Version", parts[1])));
    document.Save(projectPath);
}

static string[] PackagesFor(string provider, string driver) => (provider, driver) switch
{
    ("efcore", "sqlite") => ["Microsoft.EntityFrameworkCore.Sqlite:10.0.11"],
    ("efcore", "sqlserver") => ["Microsoft.EntityFrameworkCore.SqlServer:10.0.11"],
    ("efcore", "postgresql") => ["Npgsql.EntityFrameworkCore.PostgreSQL:10.0.0"],
    ("nhibernate", "sqlite") => ["NHibernate:5.5.2", "FluentNHibernate:3.4.0", "System.Data.SQLite.Core:1.0.119"],
    ("nhibernate", "sqlserver") => ["NHibernate:5.5.2", "FluentNHibernate:3.4.0", "Microsoft.Data.SqlClient:6.1.6"],
    ("nhibernate", "postgresql") => ["NHibernate:5.5.2", "FluentNHibernate:3.4.0", "Npgsql:10.0.0"],
    ("dapper", "sqlite") => ["Dapper:2.1.66", "Microsoft.Data.Sqlite:10.0.11"],
    ("dapper", "sqlserver") => ["Dapper:2.1.66", "Microsoft.Data.SqlClient:6.1.6"],
    ("dapper", "postgresql") => ["Dapper:2.1.66", "Npgsql:10.0.0"],
    ("mongo", "mongodb") => ["MongoDB.Driver:3.4.0"],
    _ => throw new ArgumentException($"Não há catálogo de pacotes para {provider}/{driver}.")
};

static string[] ReadEnabledProviders(string root)
{
    string? path = Directory.GetFiles(Path.Combine(root, "src", "IoC"), "appsettings.json", SearchOption.AllDirectories).FirstOrDefault();
    var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    string? ioC = Directory.GetDirectories(Path.Combine(root, "src", "IoC"), "*.IoC", SearchOption.AllDirectories).FirstOrDefault();
    if (ioC is not null)
    {
        foreach (string module in Directory.GetFiles(ioC, "*PersistenceModule.cs", SearchOption.AllDirectories))
        {
            string name = Path.GetFileName(module);
            if (name.StartsWith("EfCore", StringComparison.OrdinalIgnoreCase)) result.Add("efcore");
            if (name.StartsWith("NHibernate", StringComparison.OrdinalIgnoreCase)) result.Add("nhibernate");
            if (name.StartsWith("Dapper", StringComparison.OrdinalIgnoreCase)) result.Add("dapper");
            if (name.StartsWith("Mongo", StringComparison.OrdinalIgnoreCase)) result.Add("mongo");
        }
    }
    if (path is null) return result.Count == 0 ? ["efcore"] : result.ToArray();
    JsonObject document = JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? new JsonObject();
    JsonObject? providers = document["Persistence"]?["Providers"]?.AsObject();
    if (providers is null) return result.Count == 0 ? ["efcore"] : result.ToArray();
    foreach ((string key, JsonNode? value) in providers)
        if (value?["Enabled"]?.GetValue<bool>() == true) result.Add(key.ToLowerInvariant());
    return result.Count == 0 ? ["efcore"] : result.ToArray();
}

static void ValidatePersistenceConfiguration(string root)
{
    string? path = Directory.GetFiles(Path.Combine(root, "src", "IoC"), "appsettings.json", SearchOption.AllDirectories).FirstOrDefault();
    if (path is null) return;

    JsonObject document = JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? new JsonObject();
    JsonObject? persistence = document["Persistence"]?.AsObject();
    if (persistence is null) return;
    JsonObject? providers = persistence["Providers"]?.AsObject();
    if (providers is null) throw new InvalidOperationException("Persistence.Providers não foi encontrado no appsettings.");

    string defaultProvider = persistence["DefaultProvider"]?.GetValue<string>()?.Trim().ToLowerInvariant()
        ?? throw new InvalidOperationException("Persistence.DefaultProvider precisa ser informado.");
    var enabled = new List<string>();
    foreach ((string key, JsonNode? value) in providers)
    {
        if (value?["Enabled"]?.GetValue<bool>() != true) continue;
        string provider = key switch { "EfCore" => "efcore", "NHibernate" => "nhibernate", "Dapper" => "dapper", "Mongo" => "mongo", _ => key.ToLowerInvariant() };
        string? driver = value["Driver"]?.GetValue<string>()?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(driver)) throw new InvalidOperationException($"O provider {provider} está habilitado sem driver.");
        _ = PackagesFor(provider, driver);
        enabled.Add(provider);
    }

    if (enabled.Count == 0) throw new InvalidOperationException("Pelo menos um provider de persistência deve estar habilitado.");
    if (!enabled.Contains(defaultProvider, StringComparer.OrdinalIgnoreCase))
        throw new InvalidOperationException($"O provider padrão '{defaultProvider}' precisa estar habilitado.");
}

static string ProviderSettingsKey(string provider) => provider switch { "efcore" => "EfCore", "nhibernate" => "NHibernate", "dapper" => "Dapper", "mongo" => "Mongo", _ => provider };
static string ProviderConstant(string provider) => provider switch { "efcore" => "EfCore", "nhibernate" => "NHibernate", "dapper" => "Dapper", "mongo" => "Mongo", _ => provider };
static string ProviderSuffix(string provider) => provider switch { "efcore" => "Ef", "nhibernate" => "NHibernate", "dapper" => "Dapper", "mongo" => "Mongo", _ => provider.Humanize().Pascalize() };
static string DefaultDriver(string provider) => provider == "mongo" ? "mongodb" : "sqlite";
static string DriverName(string driver) => driver switch { "sqlite" => "Sqlite", "sqlserver" => "SqlServer", "postgresql" => "Postgresql", "mongodb" => "Mongodb", _ => driver };
static string DialectName(string driver) => driver switch { "sqlite" => "SQLite", "sqlserver" => "MsSql2012", "postgresql" => "PostgreSQL", _ => driver };

static Dictionary<string, string> ParseArguments(IList<string> args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var flags = new HashSet<string>(["force", "help"], StringComparer.OrdinalIgnoreCase);
    for (int index = 0; index < args.Count; index++)
    {
        string argument = args[index];
        if (!argument.StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException($"Argumento inválido: {argument}.");
        string key = argument[2..];
        if (flags.Contains(key)) { result[key] = string.Empty; continue; }
        if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException($"O argumento --{key} precisa de um valor.");
        string value = args[++index];
        result[key] = result.TryGetValue(key, out string? previous) ? $"{previous},{value}" : value;
    }
    return result;
}

static HashSet<string> GetSelectedArtifacts(Dictionary<string, string> options)
{
    string[] allowed = ["entity", "command", "filter", "query", "repository-interface", "service", "create-request", "list-request", "response", "query-response", "appservice", "controller", "entity-tests", "service-tests"];
    if (!options.TryGetValue("generate", out string? value)) return allowed.ToHashSet(StringComparer.OrdinalIgnoreCase);
    var aliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) { ["all"] = allowed, ["tests"] = ["entity-tests", "service-tests"], ["domain"] = ["entity", "command", "filter", "query", "repository-interface", "service"], ["application"] = ["create-request", "list-request", "response", "query-response", "appservice"], ["repository"] = ["repository-interface"], ["requests"] = ["create-request", "list-request"], ["responses"] = ["response", "query-response"], ["entity-test"] = ["entity-tests"], ["service-test"] = ["service-tests"] };
    var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (string item in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (!aliases.TryGetValue(item, out string[]? artifacts)) artifacts = [item];
        foreach (string artifact in artifacts) { if (!allowed.Contains(artifact, StringComparer.OrdinalIgnoreCase)) throw new ArgumentException($"Artefato inválido: {item}."); selected.Add(artifact); }
    }
    return selected;
}

static string ToCamelCase(string value) => char.ToLowerInvariant(value[0]) + value[1..];
static string InferRootNamespace(string root)
{
    string? domainProject = Directory.GetFiles(Path.Combine(root, "src", "Domain"), "*.Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
    if (domainProject is null)
        throw new InvalidOperationException("Não foi possível inferir o namespace raiz: projeto Domain não encontrado. Informe --rootNamespace.");

    string projectName = Path.GetFileNameWithoutExtension(domainProject);
    return projectName.EndsWith(".Domain", StringComparison.OrdinalIgnoreCase)
        ? projectName[..^".Domain".Length]
        : projectName;
}

static void ValidateIdentifier(string value, string name) { if (string.IsNullOrWhiteSpace(value) || !IsIdentifierStart(value[0]) || value.Any(character => !IsIdentifierPart(character))) throw new ArgumentException($"O valor de --{name} não é um identificador C# válido: {value}."); }
static void ValidateNamespace(string value) { foreach (string part in value.Split('.')) ValidateIdentifier(part, "rootNamespace"); }
static bool IsIdentifierStart(char value) => char.IsLetter(value) || value == '_';
static bool IsIdentifierPart(char value) => char.IsLetterOrDigit(value) || value == '_';
static void PrintUsage() { Console.WriteLine("Uso:"); Console.WriteLine("  dotnet script tools/scaffold/main.csx -- --entity Evento --context Eventos"); Console.WriteLine("  dotnet script tools/scaffold/main.csx -- --add-provider nhibernate --driver postgresql"); Console.WriteLine("  dotnet script tools/scaffold/main.csx -- --entity Evento --generate controller"); Console.WriteLine("Providers: efcore, nhibernate, dapper, mongo."); Console.WriteLine("Drivers: sqlite, sqlserver, postgresql, mongodb."); }
