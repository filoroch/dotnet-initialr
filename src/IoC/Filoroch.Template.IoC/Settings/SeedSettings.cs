namespace Filoroch.Template.IoC.Settings;

public sealed class SeedSettings
{
    public const string SectionName = "Seed";

    public string AdminUsername { get; set; } = "admin";
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
