namespace Filoroch.Template.IoC.Settings;

public sealed class PersistenceSettings
{
    public const string SectionName = "Persistence";

    public string DefaultProvider { get; set; } = PersistenceProviders.EfCore;
    public string? SelectedProviders { get; set; }
    public string? SelectedDrivers { get; set; }
    public PersistenceProvidersSettings Providers { get; set; } = new();

    public string GetDriver(string provider, string fallback)
    {
        string[] providers = (SelectedProviders ?? string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string[] drivers = (SelectedDrivers ?? string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        int index = Array.FindIndex(providers, value => value.Equals(provider, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index < drivers.Length ? drivers[index] : fallback;
    }
}

public sealed class PersistenceProvidersSettings
{
    public PersistenceProviderSettings EfCore { get; set; } = new();
    public PersistenceProviderSettings NHibernate { get; set; } = new();
    public PersistenceProviderSettings Dapper { get; set; } = new();
    public MongoPersistenceSettings Mongo { get; set; } = new();
}

public static class PersistenceProviders
{
    public const string EfCore = "efcore";
    public const string NHibernate = "nhibernate";
    public const string Dapper = "dapper";
    public const string Mongo = "mongo";
}

public sealed class PersistenceProviderSettings
{
    public bool Enabled { get; set; }
    public string Driver { get; set; } = "Sqlite";
    public string? Dialect { get; set; }
    public string ConnectionString { get; set; } = "__SET_VIA_DOTNET_USER_SECRETS__";
}

public sealed class MongoPersistenceSettings
{
    public bool Enabled { get; set; }
    public string Driver { get; set; } = "Mongodb";
    public string ConnectionString { get; set; } = "__SET_VIA_DOTNET_USER_SECRETS__";
    public string Database { get; set; } = "Template";
}
