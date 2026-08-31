namespace Filoroch.Template.IoC.Settings;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}
