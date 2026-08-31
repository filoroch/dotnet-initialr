namespace Filoroch.Template.IoC.Settings;

public static class PersistenceSettingsValidator
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> SupportedDrivers =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [PersistenceProviders.EfCore] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sqlite", "sqlserver", "postgresql"
            },
            [PersistenceProviders.NHibernate] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sqlite", "sqlserver", "postgresql"
            },
            [PersistenceProviders.Dapper] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sqlite", "sqlserver", "postgresql"
            },
            [PersistenceProviders.Mongo] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "mongodb"
            }
        };

    public static void Validate(PersistenceSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        List<(string Name, bool Enabled, string Driver, string? ConnectionString)> configured =
        [
            (PersistenceProviders.EfCore, settings.Providers.EfCore.Enabled,
                settings.GetDriver(PersistenceProviders.EfCore, settings.Providers.EfCore.Driver), settings.Providers.EfCore.ConnectionString),
            (PersistenceProviders.NHibernate, settings.Providers.NHibernate.Enabled,
                settings.GetDriver(PersistenceProviders.NHibernate, settings.Providers.NHibernate.Driver), settings.Providers.NHibernate.ConnectionString),
            (PersistenceProviders.Dapper, settings.Providers.Dapper.Enabled,
                settings.GetDriver(PersistenceProviders.Dapper, settings.Providers.Dapper.Driver), settings.Providers.Dapper.ConnectionString),
            (PersistenceProviders.Mongo, settings.Providers.Mongo.Enabled,
                settings.GetDriver(PersistenceProviders.Mongo, settings.Providers.Mongo.Driver), settings.Providers.Mongo.ConnectionString)
        ];

        if (!SupportedDrivers.ContainsKey(settings.DefaultProvider))
        {
            throw new InvalidOperationException(
                $"Provider padrão inválido: '{settings.DefaultProvider}'. Providers suportados: {string.Join(", ", SupportedDrivers.Keys)}.");
        }

        List<(string Name, bool Enabled, string Driver, string? ConnectionString)> enabled =
            configured.Where(item => item.Enabled).ToList();

        if (enabled.Count == 0)
        {
            throw new InvalidOperationException("Pelo menos um provider de persistência deve estar habilitado.");
        }

        if (!enabled.Any(item => string.Equals(item.Name, settings.DefaultProvider, StringComparison.OrdinalIgnoreCase)))
        {
            enabled.Add(configured.First(item => string.Equals(item.Name, settings.DefaultProvider, StringComparison.OrdinalIgnoreCase)));
        }

        foreach ((string name, bool _, string driver, string? connectionString) in enabled)
        {
            if (string.IsNullOrWhiteSpace(driver) || !SupportedDrivers[name].Contains(driver))
            {
                throw new InvalidOperationException(
                    $"Driver '{driver}' não é compatível com o provider '{name}'. Drivers suportados: {string.Join(", ", SupportedDrivers[name])}.");
            }

            if (string.Equals(name, settings.DefaultProvider, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrWhiteSpace(connectionString) || connectionString.StartsWith("__SET_", StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    $"A connection string do provider padrão '{name}' precisa ser configurada no ambiente ou via dotnet user-secrets.");
            }

            if (name.Equals(PersistenceProviders.Mongo, StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(settings.Providers.Mongo.Database))
            {
                throw new InvalidOperationException("Persistence:Providers:Mongo:Database é obrigatório.");
            }
        }
    }
}
