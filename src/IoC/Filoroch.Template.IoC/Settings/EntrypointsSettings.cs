namespace Filoroch.Template.IoC.Settings;

public sealed class EntrypointsSettings
{
    public const string SectionName = "Entrypoints";

    public ApiEntrypointSettings Api { get; set; } = new();
}

public sealed class ApiEntrypointSettings
{
    public string Name { get; set; } = "API";
    public string[] Urls { get; set; } = [];
}
