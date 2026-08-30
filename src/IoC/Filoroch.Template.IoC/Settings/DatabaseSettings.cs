namespace Filoroch.Template.IoC.Settings;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";
    public string ConnectionString { get; set; } = "Data Source=filoroch-template.db";
}
